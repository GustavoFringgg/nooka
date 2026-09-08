# 明天 TODO — ASP.NET Identity + JWT 登入/登出

> 目標:做出可以透過伺服器登入/登出帳號的第一個垂直切片。**不含** Google 登入、Email 驗證、忘記密碼、前端串接。

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
