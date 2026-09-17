# ASP.NET Identity + JWT 登入/登出(後端)— 已完成

> 目標:做出可以透過伺服器登入/登出帳號的第一個垂直切片。**不含** Google 登入、Email 驗證、忘記密碼、前端串接。
>
> **狀態(2026/09/14):Stage 0～7 全部完成並驗證過(`.http` 測試拿到 `Set-Cookie`)。** 下一步是前端串接,見本檔最下方「前端 Google 登入/登出串接」章節。

## 為什麼

單字卡練習模式的 mock 進度(存 localStorage)已經做完,要變成真的存進 DB 需要 `UserId`,SM-2 學習紀錄也一樣依賴會員系統——這是往下走的硬性依賴。

## 分階段步驟(照順序,一步做完驗證完再下一步)

### Stage 0 — 安裝套件 -- done

```
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 10.0.9
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 10.0.9
```

不用裝 `Identity.UI`(那是 Razor Pages 用的)。驗證:`dotnet build` 過。

### Stage 1 — Identity 模型形狀

自訂 `AppUser : IdentityUser<int>`、`AppRole : IdentityRole<int>`(不用預設 Guid 主鍵,對齊現有 `Words`/`Categories` 的 int 主鍵慣例)。

- 新增 `Models/AppUser.cs`、`Models/AppRole.cs`(空殼繼承,先不加欄位)
- `Data/AppDbContext.cs` 改繼承 `IdentityDbContext<AppUser, AppRole, int>`,`OnModelCreating` 第一行要呼叫 `base.OnModelCreating(modelBuilder)`(容易漏掉)

驗證:`dotnet build` 過,先不跑 migration。

### Stage 2 — `Program.cs` 接線

- `AddIdentityCore<AppUser>()`(不是 `AddIdentity<>`,避免多註冊一個跟 JWT 衝突的 cookie scheme)+ `.AddRoles<AppRole>()` + `.AddEntityFrameworkStores<AppDbContext>()` + `.AddSignInManager()` + `.AddDefaultTokenProviders()`
- `AddAuthentication().AddJwtBearer(...)`:驗證 issuer/audience/signing key(存在 `appsettings.Development.json` 新增的 `Jwt:Issuer`/`Jwt:Audience`/`Jwt:Key`),用 `JwtBearerEvents.OnMessageReceived` 從 `access_token` cookie 讀 token(不是預設 Authorization header——前端 JS 讀不到 httpOnly cookie,本來就沒辦法自己塞 header)
- `AddAuthorization()`
- middleware pipeline 補 `app.UseAuthentication()`,位置在 `UseCors` 之後、`UseAuthorization` 之前(目前完全沒有這行)

驗證:`dotnet build` + `dotnet run` 正常啟動,行為跟現在一樣。

### Stage 3 — Migration

```
dotnet ef migrations add AddIdentity
dotnet ef database update
```

套用前先看一眼產生的 migration 檔,確認只有 `CREATE TABLE`,沒動到 `Words`/`Categories`。套用後去 Supabase table editor 確認 7 張新表都在、`AspNetUsers.Id`/`AspNetRoles.Id` 是 `integer`。

### Stage 4 — Role 種子資料

`Program.cs` 裡 `app.Run()` 之前,用 scope 拿 `RoleManager<AppRole>`,跑一段 `foreach` 建 `Admin`/`User` 兩個角色(`RoleExistsAsync` 檔重複建立)。不用另外包 seeding service class。

驗證:重跑一次 app,`AspNetRoles` 剛好兩筆,不會重複新增。

### Stage 5 — Google 登入串接準備 (0908進度到這)

- 到 Google Cloud Console 建立 OAuth Client ID(Web application 類型),先設定授權的 JavaScript 來源 `http://localhost:3000`(給前端之後用),取得 Client ID
- 後端安裝 `Google.Apis.Auth` 套件(提供 `GoogleJsonWebSignature.ValidateAsync`,驗證前端傳來的 Google ID Token 用)
- `appsettings.Development.json` 新增 `Google:ClientId`
- `Models/AuthDtos.cs`:不需要 `RegisterRequest`/`LoginRequest`,改成 `record GoogleLoginRequest(string IdToken)`

驗證:`dotnet build` 過。

### Stage 6 — 登入/登出(Google)

- `POST /api/auth/google-login`:用 `GoogleJsonWebSignature.ValidateAsync(request.IdToken, validationSettings)` 驗證 token(`validationSettings.Audience` 要設成自己的 `Google:ClientId`,不然任何人拿自己的 Google token 都能登進來)→ 驗證通過後從回傳的 payload 拿 `Email` → `UserManager.FindByEmailAsync` 查帳號,查不到就 `UserManager.CreateAsync(user)`(不帶密碼參數)新建一個 `AppUser` + `AddToRoleAsync(user, "User")` → 撈 roles → 產 JWT(claims 帶 `NameIdentifier`/`Email`/`Role`)→ `Response.Cookies.Append("access_token", token, new CookieOptions { HttpOnly = true, Secure = false, SameSite = Lax, Expires = ... })`(本地先 `Secure=false`,上線再依 CLAUDE.md 定案改 `SameSite=None;Secure`)
- `GenerateJwtToken` 先寫成 `AuthController` 裡的 private method,不用另外拆 service
- `POST /api/auth/logout`:`Response.Cookies.Delete("access_token")`

**明確不做**:refresh token 輪替、帳號綁定多個第三方登入方式。cookie 命名先用 `access_token`,以後加 `refresh_token` 不用改名。

### Stage 7 — 驗證

因為沒有帳密可以直接寫死在 `.http` 檔測試,測 `POST /api/auth/google-login` 前要先拿到一個真的 Google ID Token——用 Google 官方的 [OAuth Playground](https://developers.google.com/oauthplayground)或寫一個最小的測試頁(用 Google Identity Services JS)登入拿 token,貼進 `.http` 檔的 request body。

`Nooka.Api.http` 補上 google-login(帶真的 ID Token)→ 呼叫既有 API(如 `GET /api/categories`,確認沒壞掉)→ logout。確認 google-login 回應有 `Set-Cookie: access_token=...; httponly`,logout 回應的 `Set-Cookie` 是過期值。**明天不用加任何 `[Authorize]`**,那是下一步。

## 決策點總表

| 決策                               | 選擇                                                         | 理由                                          |
| ---------------------------------- | ------------------------------------------------------------ | --------------------------------------------- |
| Guid vs int 主鍵                   | 自訂 `IdentityUser<int>`/`IdentityRole<int>`                 | 對齊現有表的 int 主鍵慣例                     |
| `AddIdentity` vs `AddIdentityCore` | `AddIdentityCore` + `.AddSignInManager()`                    | 避免多註冊一個跟 JWT 衝突的 cookie scheme     |
| JWT 放 header vs cookie            | httpOnly cookie(`OnMessageReceived` 讀)                      | 架構定案 httpOnly cookie,前端 JS 本來就讀不到 |
| 登入方式                           | 只做 Google 登入,不做 Email+密碼                             | 統一登入方式,省掉密碼儲存/忘記密碼整組流程    |
| Google token 驗證                  | `Google.Apis.Auth` 的 `GoogleJsonWebSignature.ValidateAsync` | 官方套件,內建拿 Google 公鑰驗簽章,不用自己刻  |
| 新帳號建立時機                     | 登入 API 裡查無帳號就順便建(無密碼)                          | 沒有獨立註冊流程,Google 登入即註冊            |
| Token 產生邏輯放哪                 | `AuthController` 裡的 private method                         | MVP 先別過度抽象,等加 refresh token 才抽出來  |

## 涉及檔案

- `backend/Nooka.Api/Program.cs`(修改:DI 註冊 + middleware pipeline)
- `backend/Nooka.Api/Data/AppDbContext.cs`(修改:改繼承 `IdentityDbContext`)
- `backend/Nooka.Api/Models/AppUser.cs`、`AppRole.cs`、`AuthDtos.cs`(新增)
- `backend/Nooka.Api/Controllers/AuthController.cs`(新增)
- `backend/Nooka.Api/Nooka.Api.csproj`(套件)
- `backend/Nooka.Api/appsettings.Development.json`(新增 `Jwt:*`、`Google:ClientId` 設定)
- `backend/Nooka.Api/Nooka.Api.http`(新增測試請求)

## 驗證方式

`dotnet build` 每個 stage 都要過;Stage 3 migration 套用後去 Supabase table editor 肉眼確認新表結構跟既有表沒被動到;Stage 7 用 `.http` 檔或 curl(帶一個從 OAuth Playground 拿到的真 Google ID Token)跑過 google-login(確認 `Set-Cookie` header)→ 呼叫既有 API 沒壞 → logout(確認 cookie 被清空)。全程不用動前端。

---

# 前端 Google 登入/登出串接

> 目標:使用者可以在前端畫面上,透過 Google 帳號真的登入/登出,`AppNav.vue` 反映真實登入狀態,且重新整理頁面後登入狀態不會消失。

## 為什麼

後端 Google 登入/登出已經完成並測試過(見上方章節)。前端目前是 `useDemoAuth.ts` 這個假的 `useState<boolean>` 在 `AppNav.vue` 切換登入/登出文字,沒有真的呼叫後端、沒有 Google 登入 SDK、也沒有處理 httpOnly cookie 需要的 `credentials: 'include'`。

登入畫面的視覺設計稿使用者會另外整理一個資料夾丟進來,目前還沒到位——Stage 4(套用設計稿的登入按鈕)要等設計稿到位才能真的動工,但其他 Stage 不受影響,可以先做。

## 已確認的決策

- **登入狀態持久化**:新增 `GET /api/auth/me`(後端,`[Authorize]`),前端啟動時呼叫一次來判斷是否已登入——因為 token 在 httpOnly cookie 裡,前端 JS 本來就讀不到,不加這支 API 的話 reload 頁面就會忘記登入狀態。
- **Google 互動方式**:官方 Google Identity Services 的 `renderButton`(不裝第三方 npm 包裝套件,如 `vue3-google-login`)。
- **前端狀態管理**:沿用現有 `useState` pattern(`useDemoAuth.ts` 已經是這樣寫),**不新增 Pinia**——專案目前沒裝 Pinia,MVP 階段不需要為了一個 auth state 多引入一個狀態管理套件。
- **套用設計稿(Stage 4)由 Claude 直接實作**,其餘 Stage 維持一步一步來、使用者動手為主的節奏(跟上方後端 Stage 拆法一致)。

## 分階段步驟(照順序,一步做完驗證完再下一步)

### Stage 0 — 環境變數

- `frontend/.env`、`.env.example` 新增 `NUXT_PUBLIC_GOOGLE_CLIENT_ID`(值是後端 `appsettings.Development.json` 裡同一個 `Google:ClientId`)
- `frontend/nuxt.config.ts` 的 `runtimeConfig.public` 加 `googleClientId`,比照現有 `apiBase` 的寫法
- 確認 Google Cloud Console 該 OAuth Client 的「已授權的 JavaScript 來源」已經有 `http://localhost:3000`(後端 Stage 5 應該已經設過)

### Stage 1 — 載入 Google Identity Services script

- `frontend/app/app.vue`(或 `nuxt.config.ts` 的 `app.head`)用 `useHead` 加 `<script src="https://accounts.google.com/gsi/client" async defer>`

驗證:瀏覽器 devtools console 打 `window.google.accounts.id`,有東西不是 `undefined`。

### Stage 2 — 後端補強:CORS + `/api/auth/me`

- `backend/Nooka.Api/Program.cs`:CORS policy `NuxtDev` 加 `.AllowCredentials()`(目前只有 `WithOrigins().AllowAnyHeader().AllowAnyMethod()`,缺這個的話瀏覽器不會讓帶 cookie 的跨網域請求過)
- `backend/Nooka.Api/Controllers/AuthController.cs` 新增 `[Authorize] [HttpGet("me")]`,從 `User`(`ClaimsPrincipal`)讀 `NameIdentifier`/`Email`/`Role` claims 組一個回傳物件——不用查 DB,JWT claims 裡已經有這些資訊

驗證:登入後帶著瀏覽器 cookie 呼叫 `GET /api/auth/me` 回 200 + 使用者資訊;沒帶 cookie(或 cookie 過期)呼叫回 401。

### Stage 3 — 前端 `useAuth` composable

- 比照 `app/composables/useApi.ts` 的 `useApiUrl`,新增一個小 helper 讓需要帶 cookie 的請求統一加上 `credentials: 'include'`(`google-login`、`logout`、`me` 三支都要用到)
- 新增 `frontend/app/composables/useAuth.ts`,取代 `useDemoAuth.ts`:
  - `useAuthUser()` → `useState<AuthUser | null>("authUser", () => null)`
  - `fetchMe()` → 呼叫 `/api/auth/me`,更新 state(app 啟動時呼叫一次,例如 `app.vue` 的 `onMounted` 或一個 plugin)
  - `loginWithGoogle(idToken)` → 呼叫 `/api/auth/google-login`,成功後呼叫 `fetchMe()` 拿使用者資訊
  - `logout()` → 呼叫 `/api/auth/logout`,清空 state
- 新增 `AuthUser` 型別(放 `app/types/practice.ts` 旁邊新開一個 `app/types/auth.ts`)

驗證:先不接 UI,在頁面上放一顆測試按鈕呼叫這幾個 function,console.log 確認狀態有正確變化。

### Stage 4 — Google 登入按鈕(套用設計稿)—— 等設計稿資料夾準備好後由 Claude 來刻

- 依設計稿寫登入按鈕/登入區塊的樣式
- 用 `google.accounts.id.initialize({ client_id, callback })` + `google.accounts.id.renderButton(el, options)` 掛上官方按鈕
- callback 拿到 `response.credential`(就是 Google ID Token)→ 呼叫 `useAuth().loginWithGoogle(idToken)`

驗證:點按鈕 → 跳出 Google 帳號選擇畫面 → 選完後畫面反應登入成功(可以先看 Stage 3 留的測試輸出,UI 串接留到 Stage 5)。

### Stage 5 — `AppNav.vue` 串接真實登入狀態

- 把 `useDemoLoggedIn()` 換成 `useAuthUser()`
- 未登入:顯示登入按鈕(或連到登入頁);已登入:顯示使用者資訊 + 登出按鈕(呼叫 `useAuth().logout()`)
- `useDemoAuth.ts` 確認沒有其他地方(目前已知 `overview.vue` 有讀)還在用,清掉或一併換成 `useAuthUser()`

驗證(整條流程跑一次):首次進站 → nav 顯示未登入 → 點登入 → Google 選帳號 → 導回後 nav 顯示已登入 → reload 頁面登入狀態還在 → 點登出 → nav 變回未登入。

## 涉及檔案

**前端**

- `frontend/.env`、`.env.example`(新增)
- `frontend/nuxt.config.ts`
- `frontend/app/app.vue`
- `frontend/app/composables/useAuth.ts`(新增,取代 `useDemoAuth.ts`)
- `frontend/app/composables/useApi.ts`(擴充 credentials helper)
- `frontend/app/components/AppNav.vue`
- `frontend/app/types/auth.ts`(新增)
- 登入按鈕元件(Stage 4 才會確定檔名,依設計稿結構而定)

**後端**

- `backend/Nooka.Api/Program.cs`(CORS 加 `AllowCredentials`)
- `backend/Nooka.Api/Controllers/AuthController.cs`(新增 `me` action)

---

TODO: 待學習資訊

ASP.NET Identity + JWT 登入/登出

1. 為什麼是在 Nooka.Api.http 測試 為什麼是在這裡測試 Nooka.Api.http 在這個專案扮演什麼樣的角色
2. http第二個 應該只是測試api 還沒有帶token進去 去驗證
3. 登出 要怎麼判斷這個 token 過期
4. 那這樣我 httpOnly 我實際是把 token 存在哪裡? 機制是什麼 檢查token的機制是怎麼看得
