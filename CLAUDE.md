# Nooka

英文單字卡片學習應用(flashcard learning app),需要會員登入機制。

## 問答溝通偏好

- 使用者(Derek)在學習後端概念,問題會一個一個接續著問。**只回答被問到的問題本身,不要延伸額外概念、不要主動補充「你可能也會想知道」的內容**,避免增加認知負荷。
- 使用者會自己把他的理解回饋過來確認對不對,由他主導要往哪個方向延伸,不用預先幫他鋪陳。

## 現況

- `backend/Nooka.Api`:已接上 Supabase PostgreSQL(EF Core + Npgsql),唯讀 API 正常運作
  - `Models/Words.cs`:欄位 `Term`(英文單字)、`DefinitionCN`(中文意思)、`DefinitionEN`(英文釋義)、`PartOfSpeech`、`Examples`;`DefinitionCN`/`DefinitionEN` 是從原本單一 `Definition` 欄位拆出來的(migration rename 時工具猜錯方向,`Definition` 被重新命名成 `DefinitionEN`,`DefinitionCN` 是新欄位,已手動 SQL 修正資料位置)
  - `Models/Category.cs`:欄位 `Name`、`Description`、`CreatedAt`、`UpdatedAt`(DB 端 `DEFAULT now()`,`AppDbContext.OnModelCreating` 用 Fluent API 設定,手動在 Supabase insert/update 時不用帶這兩欄)
  - `Data/AppDbContext.cs`:EF Core DbContext,定義 `Words`、`Categories` 資料表
  - `Repositories/IWordRepository.cs` + `EfWordRepository.cs`:Repository 介面 + EF Core 實作(已取代 InMemoryWordRepository)
  - `Migrations/`:EF Core 初版 migration(InitialCreate),已套用到 Supabase
  - `Controllers/WordsController.cs`:唯讀 API,`GET /api/words`、`GET /api/words/{id}`、`GET /api/words/category/{categoryId}`
  - `Program.cs`:已加上 CORS(`NuxtDev` policy,允許 `http://localhost:3000`),`UseCors` 放在 `UseAuthorization` 之前
  - `Repositories/ICategoryRepository.cs` + `EfCategoryRepository.cs`:分類 Repository,仿照 Word 版本 pattern,已完成 `GetAllAsync()`、`GetByIdAsync(int)`;`CategoriesController.cs` 與 `Program.cs` 的 DI 註冊還沒做(見下方「進行中」)
- `frontend/`:Nuxt 4,已啟用 file-based routing(`app.vue` 改用 `NuxtPage`)
  - `design_handoff_velorah_hero/`(專案根目錄):Claude design 工具輸出的靜態 HTML/CSS 設計交付稿(品牌名「Velorah」是設計稿本身的,跟 Nooka 無關),`DESIGN_TOKENS.md` 記錄色票與 liquid-glass 玻璃質感元件規格,正在依此改造成 Nuxt 頁面,文字改成 Nooka 佔位字
  - `app/pages/index.vue`:首頁,採**單頁式**方向 — 上方 100vh 影片 Hero(nav + 標題 + CTA),往下滾動接深藍色 Night Palette 內容區塊(分類/練習模式/學習紀錄卡片),不做「首頁只放 3D 圖、完整介紹另外開一頁」的方案(討論過兩個方向,選了這個,效果不理想可能會 roll back)
  - `app/components/AppNav.vue`:共用 nav 元件(首頁/練習/學習紀錄/登入),`active` 連結用 `useRoute()` 自動判斷,不用手動寫死;`index.vue`、`practice/` 系列頁面都已改用這個元件
  - 這個首頁是行銷 / 訪客用的 landing page,跟下方「首頁書架」使用者故事(登入後瀏覽分類的書架頁)是不同頁面,兩者尚未串接
  - `app/pages/practice/index.vue`:書架頁,改版後**合併**了原本「分類單字列表頁」的角色 — 左側書架卡片選書,右側面板即時顯示該分類單字預覽,點「選擇題」開 `UModal` 選練習方向 + 題數(`USlider`),按「開始練習」才 `router.push` 帶 `?direction=&count=` query 跳到 `choice.vue`;書架清單、右側單字預覽已改回串真的 `GET /api/categories`、`GET /api/words/category/{id}`(書封顏色讀 `Category.Color`,音標讀 `Word.Ipa`),不再是 mock 資料;`posColors` 補齊 8 種詞性(形容詞/副詞/動詞/名詞/代名詞/介系詞/連接詞/感嘆詞),`GET /api/categories` 載入中的畫面還沒做(見下方「UIUX 待辦」)
  - `app/pages/practice/[categoryId]/index.vue`:**已刪除**(原本的「分類單字列表頁」),書架版面改版時整併進 `practice/index.vue`,不再有獨立的中間頁
  - `app/pages/practice/[categoryId]/choice.vue`:選擇題測驗頁,完整測驗邏輯已完成(抓單字、`buildQuizQuestions` 出題、答對/答錯判斷、答錯詳解彈窗、答對特效、鍵盤方向鍵選答);`mockWords` fallback 已移除,只依賴後端資料,fetch 失敗時顯示「載入失敗」

## 進行中:練習頁第一步 — 選擇題模式(串真實後端)

練習功能的第一個垂直切片。流程:進入 `/practice` 練習頁 → 選擇要練習的書(分類)→ 進入該分類的選擇題測驗。分類清單直接串真的後端 API(不寫死在前端),因為 Derek 決定手動在 Supabase 補 `Categories`/`Words` 測試資料。

**採分階段教學方式進行**(不是自動一次做完),每一步由 Derek 自己動手改,確認後才進下一步。詳細計畫存在 `C:\Users\USER\.claude\plans\reflective-growing-eagle.md`。

**選擇題規格(2026/08/18 定案)**:

- 4 選 1(1 正解 + 3 隨機干擾項),分類底下單字數保證 > 4,不做不足提示
- 練習方向可選:「看英文選中文」或「看中文選英文」(`QuizDirection = 'enToCn' | 'cnToEn'`)
  - **定案(2026/08/19)**:方向選擇的 `UModal` 放在 `[categoryId]/index.vue`(單字列表頁),不是 `choice.vue` 本身 —— 點「選擇題」只開 dialog、不換路由,避免取消時要重新導航導致重打 API(有延遲/閃爍);按「開始測試」才把選好的方向用 `?direction=` query 帶進 `choice.vue` 的網址
- 答錯要跳出視窗:上半顯示使用者選到的(錯誤)選項完整內容,下半顯示正確答案完整內容(含例句、詞性)—— 所以 `QuizOption`/`QuizQuestion`(`types/practice.ts`)都各自帶完整 `Word` 物件,不是只存字串,答錯/答對都能直接拿到完整資料不用重新查找

### 這個切片刻意不做的事

- 不新增狀態管理套件(Pinia 等),測驗狀態用 `ref` 就夠
- 不做作答結果的後端持久化(還沒有 SM-2 / 會員系統,這階段是純前端 session 狀態)
- `app/components/` 原則上不建(單一使用位置的東西不抽元件),但 `AppNav.vue` 是例外 —— 因為 nav 已經有 2 個頁面在用(`index.vue`、`practice/index.vue`),符合「重複出現才抽元件」的原則

## 技術棧

- **Backend**: ASP.NET Core (.NET 10), `backend/Nooka.Api`
- **Frontend**: Nuxt 4 (Vue 3), `frontend/`
  - **樣式**:Tailwind CSS v4(2026/08/19 從 SCSS 全面轉過來,`app/assets/scss/` 已刪除),色票/字型定義在 `app/assets/css/main.css` 的 `@theme`(`night-bg`/`night-panel`/`night-accent`/`night-fg`/`night-muted`、`hero-bg`/`hero-fg`/`hero-muted`、`font-display`/`font-body`),liquid-glass 玻璃質感效果改成 `@layer components` 的 `.liquid-glass`/`.liquid-glass-cta`/`.liquid-glass-hover` class
  - **UI 元件庫**:Nuxt UI(`@nuxt/ui`,底層是 Reka UI + Tailwind),用於 `UModal`、`URadioGroup`、`UButton` 等互動元件;純版面/排版還是手刻 Tailwind class,不是每個東西都套件化
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
  - **AI 功能方向(2026/08/20 討論)**:鎖定以下兩個,都是「後台/演算法輔助,使用者無感」性質
    1. **AI 輔助產生卡片內容**:Admin 貼文章/單字清單,AI 生成中英釋義、例句、詞性,人工審核後才發布,降低卡片內容 curate 成本(對應上面「片語需人工 curate 內容成本高」的痛點)
    2. **語意向量(embedding)挑選干擾項**:每個單字的釋義預先算好 embedding 存 DB,出題時用 cosine similarity 挑語意相近的單字當選擇題干擾項,取代目前隨機挑同分類單字的做法;這步是一次性預計算 + 純數學比對,不用每次出題都即時呼叫 AI API
    - **刻意不做**:AI 造句批改(使用者直接跟 AI 互動的功能)— 這種會被大量呼叫、目前沒有收費計畫,先不考慮
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
  - **定案(2026/08/19)**:「首頁書架」(登入後瀏覽分類)不另外做頁面,直接沿用 `/practice` 選書頁承擔這個角色,不做兩個平行的選書畫面。已先把 nav 上的「分類」項目拿掉(`AppNav.vue`),只留「練習」一個入口。現在先不加登入保護(middleware),等會員系統做出來再補
  - **架構變更(2026/08/31 確認)**:原本規劃的三段式流程(`/practice` 選書 → `/practice/[categoryId]` 分類單字列表頁 → 選練習模式 → `/practice/[categoryId]/choice`)在書架版面改版時被簡化成兩段式 —— `[categoryId]/index.vue` 列表頁已刪除,`/practice` 書架頁直接在右側面板顯示單字預覽 + 開 `UModal` 選方向/題數,確認後直接跳 `/practice/[categoryId]/choice`。跟 Derek 討論後**維持這個合併式流程**,不救回獨立列表頁
- [ ] **書架單字預覽**(取代原本的分類單字列表頁):書架頁右側面板即時顯示選中分類的單字預覽,點「選擇題」開方向選擇 `UModal`(見上方「進行中」章節定案),按「開始練習」才切換路由進 `/practice/[categoryId]/choice?direction=...&count=...`;目前預覽面板還是 mock 資料,見下方「目前進度 2026/08/31」

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

### UIUX 待辦

- [ ] `practice/index.vue` 書架 loading 畫面:`GET /api/categories` 載入中的畫面長相 — `pending` 已解構出來(`categoriesPending`)但還沒用在 template 上,先欠著,想好 UI 長相再補

---

### 目前進度 2026/08/31

書架版面改版(米白配色、書架排版)後回頭盤點,發現 08/18-08/19 完成的「接回真實 API」被退回 mock、「分類單字列表頁」被整頁刪除。跟 Derek 討論後決定:維持現在的兩段式合併流程(書架頁直接含單字預覽,不救回獨立列表頁),書封顏色/單字音標改成後端加欄位(不用前端演算法生色),詳細規劃存在 `C:\Users\USER\.claude\plans\todo-18-19-replicated-cascade.md`。今天訂出 5 個 todo,採分階段教學方式,Derek 自己動手、確認後再進下一步:

- [x] 後端:`Category` 加 `Color` 欄位(存 hex),`dotnet ef migrations add AddCategoryColor` 套用到 Supabase,手動補現有分類的色票值
- [x] 後端:`Word` 加 `Ipa` 欄位(KK 音標),migration 套用到 Supabase,手動補現有 6 筆單字的音標
- [x] 前端:`practice/index.vue` 書架卡片改回串真的 `GET /api/categories`(顏色讀 `color` 欄位,書脊色前端對 `color` 加深處理即可,不用後端另存)
- [x] 前端:`practice/index.vue` 右側單字預覽改回串真的 `GET /api/words/category/{id}`(隨選書切換,音標讀 `ipa` 欄位);抓回來的 `Word[]` 直接在 template 用(`w.term`/`w.definitionCN`/`w.partOfSpeech`/`w.ipa`),沒有另外包一層 `previewWords` computed 轉欄位名 —— 討論後覺得多一層沒必要
- [x] 前端:`choice.vue` 移除 `mockWords`/`effectiveWords` fallback,恢復 fetch 失敗時的「載入失敗」錯誤處理

5 個 todo 全數完成(loading 畫面移到「UIUX 待辦」小節單獨追蹤)。練習頁選擇題模式垂直切片(書架選書 → 選方向/題數 → 選擇題測驗)已全部串接真實後端。
