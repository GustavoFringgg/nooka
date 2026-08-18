# Nooka

英文單字卡片學習應用(flashcard learning app),需要會員登入機制。

## 問答溝通偏好

- 使用者(Derek)在學習後端概念,問題會一個一個接續著問。**只回答被問到的問題本身,不要延伸額外概念、不要主動補充「你可能也會想知道」的內容**,避免增加認知負荷。
- 使用者會自己把他的理解回饋過來確認對不對,由他主導要往哪個方向延伸,不用預先幫他鋪陳。

## 現況

- `backend/Nooka.Api`:已接上 Supabase PostgreSQL(EF Core + Npgsql),唯讀 API 正常運作
  - `Models/Words.cs`、`Models/Category.cs`:資料模型(欄位:`Term`、`Definition`、`PartOfSpeech`、`Examples` 等)
  - `Data/AppDbContext.cs`:EF Core DbContext,定義 `Words`、`Categories` 資料表
  - `Repositories/IWordRepository.cs` + `EfWordRepository.cs`:Repository 介面 + EF Core 實作(已取代 InMemoryWordRepository)
  - `Migrations/`:EF Core 初版 migration(InitialCreate),已套用到 Supabase
  - `Controllers/WordsController.cs`:唯讀 API,`GET /api/words`、`GET /api/words/{id}`、`GET /api/words/category/{categoryId}`
  - `Program.cs`:已加上 CORS(`NuxtDev` policy,允許 `http://localhost:3000`),`UseCors` 放在 `UseAuthorization` 之前
  - `Repositories/ICategoryRepository.cs` + `EfCategoryRepository.cs`:分類 Repository,仿照 Word 版本 pattern,已完成 `GetAllAsync()`、`GetByIdAsync(int)`;`CategoriesController.cs` 與 `Program.cs` 的 DI 註冊還沒做(見下方「進行中」)
- `frontend/`:Nuxt 4,已啟用 file-based routing(`app.vue` 改用 `NuxtPage`)
  - `design_handoff_velorah_hero/`(專案根目錄):Claude design 工具輸出的靜態 HTML/CSS 設計交付稿(品牌名「Velorah」是設計稿本身的,跟 Nooka 無關),`DESIGN_TOKENS.md` 記錄色票與 liquid-glass 玻璃質感元件規格,正在依此改造成 Nuxt 頁面,文字改成 Nooka 佔位字
  - `app/pages/index.vue`:首頁,採**單頁式**方向 — 上方 100vh 影片 Hero(nav + 標題 + CTA),往下滾動接深藍色 Night Palette 內容區塊(分類/練習模式/學習紀錄卡片),不做「首頁只放 3D 圖、完整介紹另外開一頁」的方案(討論過兩個方向,選了這個,效果不理想可能會 roll back)
  - `app/pages/home.vue`:單頁式方向確定前先做的獨立介紹頁原型,內容現在跟 `index.vue` 重複,先保留,之後可能移除或另作他用
  - `app/assets/scss/_tokens.scss`、`_liquid-glass.scss`:共用設計 token(色票、字型變數、liquid-glass mixin),CSS 統一用 SCSS 管理,之後其他頁面要維持同一視覺系統可直接複用
  - 這個首頁是行銷 / 訪客用的 landing page,跟下方「首頁書架」使用者故事(登入後瀏覽分類的書架頁)是不同頁面,兩者尚未串接

## 進行中:練習頁第一步 — 選擇題模式(串真實後端)

練習功能的第一個垂直切片。流程:進入 `/practice` 練習頁 → 選擇要練習的書(分類)→ 進入該分類的選擇題測驗。分類清單直接串真的後端 API(不寫死在前端),因為 Derek 決定手動在 Supabase 補 `Categories`/`Words` 測試資料。

**採分階段教學方式進行**(不是自動一次做完),每一步由 Derek 自己動手改,確認後才進下一步。詳細計畫存在 `C:\Users\USER\.claude\plans\reflective-growing-eagle.md`。

### 這個切片刻意不做的事

- 不建立 `app/components/`(卡片/選項按鈕目前都只有單一使用位置,等第二種練習模式出現重複再抽出元件)
- 不新增狀態管理套件(Pinia 等),測驗狀態用 `ref` 就夠
- 不做作答結果的後端持久化(還沒有 SM-2 / 會員系統,這階段是純前端 session 狀態)

## 技術棧

- **Backend**: ASP.NET Core (.NET 10), `backend/Nooka.Api`
- **Frontend**: Nuxt 4 (Vue 3), `frontend/`
- **Solution**: `Nooka.slnx`(目前只包含 backend 專案)

## 架構決策

- **會員登入**:自建,使用 ASP.NET Identity(不用第三方 Auth 服務)
  - 需要 Role 機制(至少 `Admin` / `User`),供後台卡片管理權限判斷
- **資料庫**:PostgreSQL,託管在 Supabase(只用它的 Postgres,不用 Supabase Auth)
  - 需要 `Npgsql.EntityFrameworkCore.PostgreSQL` 作為 EF Core provider
  - 會員資料(帳號、Email、密碼 hash)存在這個 DB 裡的 Identity 相關資料表
- **Session 機制**:JWT,搭配 httpOnly cookie 存放(前端 JS 讀不到,防 XSS),並有 refresh token 機制
- **部署**:前端 Vercel、後端 Google Cloud Run(不同網域)
  - Cookie 需設 `SameSite=None; Secure`
  - 後端 CORS 需允許 credentials,並明確列出 Vercel 網域(不可用萬用字元)
  - Refresh token cookie 建議限縮 `Path`(例如只在 `/api/auth/refresh` 路徑送出)
- **學習演算法**:間隔重複,採用 **SM-2**
  - 使用者輸入簡化為「答對 / 答錯」二選一(不做 0~5 分難易度自評 UI)
  - 答對 / 答錯各對應一組固定的 quality 分數,餵進 SM-2 公式計算下次複習間隔(interval)與 ease factor
  - 答對不代表不用再複習,只是複習間隔會拉長
- **開發階段**:先做 MVP(會員 + 卡片學習 + SM-2),AI 功能列為 Phase 2,MVP 完成上線後再疊加
- **Word ↔ Category 關聯**:MVP 採方案 A(一個 Word 只屬於一個 Category,用單一 `CategoryId` 外鍵),未來視需求改為方案 B(多對多,加 `WordCategories` 中介表)

## 使用者故事(功能待辦 — 使用者看得到 / 用得到的東西)

### 會員(Membership)

- [ ] **註冊**:使用者可以用 Email + 密碼建立帳號
  - [ ] 註冊後需完成 **Email 驗證**(收驗證信、點連結)才算正式啟用帳號
- [ ] **登入**:使用者可以用 Email + 密碼登入
- [ ] **登出**:使用者可以登出,清除登入狀態
- [ ] **忘記密碼**:使用者可透過 Email 收到重設密碼連結,導向前端重設密碼頁面完成重設(token 帶在網址上)
- [ ] _(未來)_ **Google 登入**:使用者可綁定 / 用 Google 帳號登入
  - 透過 Google 登入的帳號視為 Email 已驗證,不需再走 Email 驗證流程

### 單字卡學習(Flashcards)

- [ ] **建立分類(書)管理(後台)**:Admin 可透過後台管理介面新增/編輯分類(書),例如「航空用」「工程用」「資安用」「多益用」— 卡片必須屬於某個分類,所以要先有分類才能建卡片
- [ ] **卡片內容管理(後台)**:Admin 可透過後台管理介面新增/編輯卡片(不是寫死在程式碼裡),卡片需歸屬到一個分類(書)
  - 後台路由規劃在同一個 Nuxt 專案裡(例如 `/admin`),用 middleware 檢查 `Admin` 角色才能進入
  - 後端需要一組限 `Admin` 權限的分類 CRUD API 與卡片 CRUD API
- [ ] **查看單字卡資訊**:使用者可以看到一張卡片的資訊(單字、意思等)
- [ ] **學習紀錄**:帳號的核心用途是記錄使用者自己的學習進度,採 SM-2 間隔重複(見上方架構決策),而非單純「學過/沒學過」的二元標記
- [ ] **練習模式 - 選擇題**
- [ ] **練習模式 - 消消樂(配對)**
- [ ] **練習模式 - 打字拼寫**
- [ ] _(未來)_ 開放一般使用者自行建立卡片 / 卡片組
- [ ] _(未來,考慮中)_ 卡片加入「常用片語 / 慣用搭配詞」(collocation)欄位,加強記憶效果 — MVP 先用多筆例句(`Examples`)頂著,片語需人工 curate 內容成本高,暫不排入 MVP

### 介面(UI)

- [ ] **首頁書架**:以「書架」概念呈現,每本書 = 一個單字分類(旅遊、工作、商務等)
  - 書封暫不顯示學習進度(32/120、進度條等),先簡單呈現就好,是否需要之後再看
- [ ] **分類單字列表頁**:點進一本書先看到該分類**所有單字的內容列表**,再從中選擇練習模式(消消樂/選擇題/打字拼寫)

## 系統代辦事項(開發 / 基礎建設,非使用者故事)

### 資料庫 / 基礎建設

- [ ] 接上 Supabase PostgreSQL(EF Core + Npgsql)
- [x] 設計單字卡資料模型(Word / Category)— 已建立 C# model,目前搭配 mock JSON + InMemoryRepository,尚未接真實 DB
- [ ] 建立初版 migration 並套用到資料庫
- [ ] 新增 `EfWordRepository`(接 Supabase 後取代 `InMemoryWordRepository`,`Program.cs` DI 註冊改一行即可)
- [ ] 規劃常用查詢欄位索引(如 `UserId + NextReviewDate`、`CategoryId`)— 讀多寫少,先靠索引優化,暫不做讀寫分離(read replica),等實測有瓶頸再評估

### 會員系統底層

- [ ] 導入 ASP.NET Identity + Role 機制(Admin / User)
- [ ] JWT + httpOnly cookie + refresh token 機制
- [ ] 本機密鑰改用 .NET User Secrets 管理(連線字串、JWT signing key),取代寫死在 appsettings.json — 非 MVP 必要項,先以 MVP 功能為主,之後再補

### 部署

- [ ] 前端 Vercel 設定
- [ ] 後端 Google Cloud Run 設定
- [ ] CORS / Cookie(SameSite=None; Secure)設定

### 快取(暫緩)

- [ ] 導入 Redis 做快取 / refresh token 撤銷名單 — 非 MVP 必要項,先跳過;等 Cloud Run 開多 instance 或需要 token 撤銷機制時再評估,現階段如需快取可用內建 `IMemoryCache`

---

### 目前進度 2026/08/18

- [x] 後端:`Program.cs` 加 CORS(`NuxtDev` policy)
- [x] 後端:`ICategoryRepository.cs` + `EfCategoryRepository.cs`(`GetAllAsync`、`GetByIdAsync`)
- [x] 後端:`CategoriesController.cs`(暴露 `GET /api/categories`)
- [x] 後端:`Program.cs` 註冊 `ICategoryRepository` → `EfCategoryRepository` 的 DI
- [ ] 前端:`nuxt.config.ts` 加 `runtimeConfig.public.apiBase`(`http://localhost:5016`)
- [ ] 前端:`app/composables/useApi.ts`(`useApiUrl` helper)
- [ ] 前端:`app/types/practice.ts`(`Category`、`Word` 型別,對齊後端 camelCase JSON)
- [ ] 前端:`app/utils/quiz.ts`(`buildQuizQuestions`,含 Fisher–Yates `shuffle`)
- [ ] 前端:`app/pages/practice/index.vue`(選書頁,抓 `GET /api/categories`,Night 色票 + liquid-glass 卡片)
- [ ] 前端:`app/pages/practice/[categoryId]/choice.vue`(選擇題測驗頁,抓 `GET /api/words/category/{categoryId}`,單字 < 2 筆顯示不足提示,答題/計分/結束畫面)
- [ ] 前端:`app/pages/index.vue` 的 `練習` nav 連結接上 `NuxtLink to="/practice"`
- [ ] Derek 手動在 Supabase 補資料:`Categories` 至少 1 筆、`Words` 同分類至少 4 筆(目前只有 1 筆測試單字 `vicarious`,categoryId 1,`Categories` 表是空的)
