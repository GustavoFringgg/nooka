# Nooka

英文單字卡片學習應用(flashcard learning app),需要會員登入機制。

## 問答溝通偏好

- 使用者(Derek)在學習後端概念,問題會一個一個接續著問。**只回答被問到的問題本身,不要延伸額外概念、不要主動補充「你可能也會想知道」的內容**,避免增加認知負荷。
- 使用者會自己把他的理解回饋過來確認對不對,由他主導要往哪個方向延伸,不用預先幫他鋪陳。

## 技術棧

- **Backend**: ASP.NET Core (.NET 10), `backend/Nooka.Api`
- **Frontend**: Nuxt 4 (Vue 3), `frontend/`
  - **樣式**:Tailwind CSS v4(2026/08/19 從 SCSS 全面轉過來,`app/assets/scss/` 已刪除),色票/字型定義在 `app/assets/css/main.css` 的 `@theme`(`night-bg`/`night-panel`/`night-accent`/`night-fg`/`night-muted`、`hero-bg`/`hero-fg`/`hero-muted`、`font-display`/`font-body`),liquid-glass 玻璃質感效果改成 `@layer components` 的 `.liquid-glass`/`.liquid-glass-cta`/`.liquid-glass-hover` class
  - **UI 元件庫**:Nuxt UI(`@nuxt/ui`,底層是 Reka UI + Tailwind),用於 `UModal`、`URadioGroup`、`UButton` 等互動元件;純版面/排版還是手刻 Tailwind class,不是每個東西都套件化
- **Solution**: `Nooka.slnx`(目前只包含 backend 專案)

## 架構決策

- **會員登入**:自建 session 機制(JWT + ASP.NET Identity 存帳號),但登入方式**只走 Google 登入**(不做 Email+密碼註冊/登入/忘記密碼)
  - 後端用 `GoogleJsonWebSignature.ValidateAsync` 驗證前端傳來的 Google ID Token,驗證過後查/建 `AppUser`(無密碼)
  - 需要 Role 機制(至少 `Admin` / `User`),供後台卡片管理權限判斷
- **資料庫**:PostgreSQL,託管在 Supabase(只用它的 Postgres,不用 Supabase Auth)
  - 需要 `Npgsql.EntityFrameworkCore.PostgreSQL` 作為 EF Core provider
  - 會員資料(帳號、Email、Google subject id)存在這個 DB 裡的 Identity 相關資料表,不存密碼
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
    1. **AI 輔助產生卡片內容**:AI 讀 DB 現有分類/單字(避免重複、抓上下文)生成新單字的中英釋義、例句、詞性,存 DB 標記「待審核」,審核管道兩個都要保留:(a)內容管理 Admin 後台 UI(見下方 C 大項),(b)LINE Messaging API 推播通知 + 按鈕核准(用 postback 夾帶單字 ID,不 parse 純文字回覆,避免多筆待審時對不上),核准後才正式發布
    2. **語意向量(embedding)挑選干擾項**:每個單字的釋義預先算好 embedding 存 DB,出題時用 cosine similarity 挑語意相近的單字當選擇題干擾項,取代目前隨機挑同分類單字的做法;這步是一次性預計算 + 純數學比對,不用每次出題都即時呼叫 AI API
    - **刻意不做**:AI 造句批改(使用者直接跟 AI 互動的功能)— 這種會被大量呼叫、目前沒有收費計畫,先不考慮
- **Word ↔ Category 關聯**:已改為方案 B(多對多,`WordCategories` 中介表,複合主鍵 `WordId`+`CategoryId`,2026/09/17 完成遷移),`Word` 不再有 `CategoryId` 外鍵
- **首頁書架**(2026/08/19 定案):「首頁書架」(登入後瀏覽分類)不另外做頁面,直接沿用 `/practice` 選書頁承擔這個角色,不做兩個平行的選書畫面,nav 只留「練習」一個入口;暫不加登入保護 middleware,等會員系統做出來再補
  - **流程簡化(2026/08/31 確認)**:原規劃的三段式流程(選書 → 分類單字列表頁 → 選練習模式 → 測驗頁)簡化成兩段式,獨立的分類單字列表頁已刪除,書架頁右側面板直接顯示單字預覽 + 開 `UModal` 選方向/題數,確認後直接跳測驗頁 — 維持這個合併式流程,不救回獨立列表頁

## 已知坑

- `Words.DefinitionCN`/`DefinitionEN` 是從原本單一 `Definition` 欄位拆出來的,migration rename 時工具猜錯方向,`Definition` 被重新命名成 `DefinitionEN`,`DefinitionCN` 是新欄位,已手動 SQL 修正資料位置 — 之後若再動這兩欄要留意。
- `Category.CreatedAt`/`UpdatedAt` 由 DB 端 `DEFAULT now()` 產生,`AppDbContext.OnModelCreating` 用 Fluent API 設定,手動在 Supabase insert/update 時不用帶這兩欄。

## 已完成的基礎建設

- Word / Category 資料模型 + Supabase 串接(EF Core + Npgsql),初版 migration 已套用
- Word ↔ Category 多對多關聯(`WordCategories` 中介表),`EfWordRepository`、`EfCategoryRepository`(已取代 InMemory 版本)
- 唯讀 API:`GET /api/words`、`GET /api/words/{id}`、`GET /api/words/category/{categoryId}`、`GET /api/categories`
- `AppNav.vue` 共用 nav 元件
- ASP.NET Identity(`AppUser`/`AppRole`,int 主鍵)+ JWT(httpOnly `access_token` cookie)基礎建設,`Admin`/`User` 角色種子資料
- Google 登入/登出:`POST /api/auth/google-login`(`GoogleJsonWebSignature.ValidateAsync` 驗證 + 查/建 `AppUser`)、`POST /api/auth/logout`、`GET /api/auth/me`(前端 `useAuth.ts` 已串接,`AppNav.vue` 反映真實登入狀態);詳細分階段記錄見 git 歷史上的 `plan.md`(該檔案內容會隨新任務覆寫,舊紀錄留在 commit 裡)
- 單字卡 Lv1~5 分級邏輯(初學三選一 / 複習升級 / Lv5 滿級彈窗)前端已完成,見下方「A-1 單字卡練習模式」
- `WordProgress` 資料表(複合鍵 `UserId`+`WordId`)+ 後端 API 全部完成:`GET /api/progress/category/{categoryId}`、`POST /api/progress/batch`、`GET /api/progress/summary`;`useFlashcardProgress.ts` 已改寫成打這幾支 API,不再用 localStorage,細節見 `plan.md`

---

## A. Nooka 學習核心(練習模式)

狀態:書架、選擇題已完成串真實後端;打字拼寫前端第一版完成;單字卡分級(Lv1~5)前端邏輯+ UI 已完成,進度串 DB 正在進行(見 `plan.md`)

1. 單字卡 — 書架瀏覽 + 單字卡資訊(`practice/index.vue`,已完成)
2. 選擇題 — 出題/作答/計分/答錯詳解(`choice.vue`,已完成)
3. 打字拼寫 — 逐字母輸入/發音/複習清單(`typing.vue`,前端邏輯完成)
4. 單字卡分級練習 — 翻牌 + 熟悉度三選一 + Leitner Lv1~5 複習(`flashcard.vue`+`useFlashcardProgress.ts`,前端邏輯完成,規則見下方「A-1」)

0903 mvp 完成 尚缺

1. 單字卡進度目前存在 local storage,DB 化 + 未登入純瀏覽模式 + 會員進度總覽頁面進行中,見 `plan.md`
2. 單字卡沒有發音功能
3. 單字卡顯示內容優化
4. RWD
5. 書架 loading 畫面、答對/答錯音效還沒做

## B. 會員系統(Membership)

狀態:Google 登入/登出(前後端)已完成,詳見「已完成的基礎建設」;尚缺 refresh token(見下方「MVP 之後可優化方向」)
架構:已在上方「架構決策」定案

1. Google 登入(唯一登入方式)/ 登出 — 已完成
2. **不做**:Email + 密碼註冊、Email 驗證、忘記密碼(密碼流程整組不需要,因為沒有密碼)

## C. 內容管理(Admin 後台)

狀態:未開始

1. 分類(書)CRUD API + 後台 UI
2. 卡片 CRUD API + 後台 UI
3. `/admin` middleware(限 `Admin` 角色)

(未來)開放一般使用者自行建立卡片/卡片組;卡片加入「常用片語/慣用搭配詞」(collocation)欄位 — MVP 先用多筆例句(`Examples`)頂著,片語需人工 curate 內容成本高,暫不排入 MVP。

## D. 學習紀錄(SM-2)

狀態:未開始,細項待補

1. SM-2 學習紀錄 API(答對/答錯 → quality 分數 → interval / ease factor 計算)
2. 複習排程呈現(非單純學過/沒學過二元標記)
3. 常用查詢欄位索引規劃(如 `UserId + NextReviewDate`、`CategoryId`)

## E. 介面(Landing + 書架殼)

狀態:landing page、書架頁改版已完成

1. Landing page(單頁式:影片 Hero + Night Palette 內容區,已完成)
2. 書架頁殼(米白配色、書架排版,已完成;loading 畫面、答對/答錯音效待補)
3. `design_handoff_velorah_hero/` 設計稿其餘頁面套用(待處理)
4. 吉祥物狗狗(Nooka):全域浮動元件掛在 `app.vue` 根層,跨頁面持續存在;平常在畫面上亂跑(GSAP 隨機移動),進測驗頁(choice/typing)時切成待機不動(用全域狀態如 Pinia store 控制模式),待實作規劃

## F. 測試與 CI/CD

1. 單元測試(Unit):後端用 xUnit 測商業邏輯(例如之後的 SM-2 計算),前端用 Vitest 測 utils/quiz.ts 這類純函式(buildQuizQuestions/buildTypingQuestions)
2. 整合測試(Integration):後端 API 層,用 WebApplicationFactory 打真實/測試用 DB,驗證 Controller + EF Core + Repository 串起來對不對
3. E2E 測試:前端用 Playwright 之類的工具,模擬使用者走完一個流程(登入 → 選書 → 練習 → 看結果)

CI/CD 把關方式:GitHub Actions 在 PR 階段跑上述測試,擋合併進 main;Vercel(前端)、Cloud Run(後端,需另寫 build+deploy workflow)都只監聽 main 的 push 自動部署,等於用「擋合併」間接把關部署,不是讓 Vercel 自己跑測試。

## G. 部署與基礎建設

狀態:未開始

1. 前端 Vercel 設定
2. 後端 Google Cloud Run 設定
3. CORS / Cookie(`SameSite=None; Secure`)設定
4. (暫緩)Redis 快取 / refresh token 撤銷名單 — 等 Cloud Run 開多 instance 或需要撤銷機制時再評估,現階段可用內建 `IMemoryCache`

---

### A-1 單字卡練習模式(前端已完成,進度 DB 化進行中)

**核心設計**(已定案並實作):

- 核心互動:翻牌 + 自評,不是單純瀏覽
- 進入方式:比照選擇題/打字拼寫,點「單字卡」跳 UModal;跟選擇題/打字拼寫不同的是**沒有題數 slider**,改成「學習新單字」/「複習已學過的單字」二選一(依 `useFlashcardProgress` 算出的 new/due 數量顯示)
- 切換下一張:按鈕 + 鍵盤方向鍵(跟 `choice.vue` 一致)
- 翻牌動畫:GSAP(`rotateY` + `backface-visibility: hidden`,驗證於 `cardTest.vue` 原型頁,正式實作在 `flashcard.vue`)
- 複習演算法:**不用 SM-2**——SM-2 留給 D 大項(學習紀錄)給選擇題/打字拼寫用,單字卡另外用一套自訂等級(Leitner 分級)系統,兩者並存、互不取代

**實作現況**:

- 前端邏輯 + UI 已完成:`frontend/app/composables/useFlashcardProgress.ts`(分級狀態機)+ `frontend/app/pages/practice/[categoryId]/flashcard.vue`(翻牌互動、三選一/複習按鈕、Lv5 滿級彈窗)+ `frontend/app/types/practice.ts` 的 `FlashcardLevel`/`FlashcardProgress` 型別
- 進度目前存在瀏覽器 `localStorage`(key: `nooka:flashcard-progress:${categoryId}`),尚未跟會員帳號綁定
- **進行中**:把進度換成存後端 DB(新增 `WordProgress` 表)、未登入使用者改為純瀏覽單字卡(不能標記熟悉度、不記錄進度)、新增「學習紀錄」頁面顯示會員進度總覽(不曝露內部 Lv1~5 數字,只顯示已精熟/學習中/尚未開始 + 待複習張數)— 詳細分階段步驟見專案根目錄 `plan.md`

**分級規則**(已定案,`useFlashcardProgress.ts` 的實際邏輯):

二、 系統三大操作流程

1. 初學模式（學新卡）

使用者點擊「學習新單字」。

系統撈取 level IS NULL AND is_archived = false 的卡片（每次限額 10 ~ 20 張）。

使用者進行三選一標記，卡片分別進入 Lv 1、Lv 2 或封存。

2. 複習模式（刷舊卡）

使用者點擊「複習已學過的單字」。

系統撈取 next_review_at <= 今天 AND is_archived = false 的卡片。

依序翻卡：

若當前卡片為 Lv 1 ~ Lv 3，點「今天已複習」直接升級並推遲 1 天。

若當前卡片為 Lv 4，點「今天已複習」升為 Lv 5 並推遲 2 天。

若當前卡片為 Lv 5，點「今天已複習」跳出 Confirm 彈窗，讓使用者選擇「不再顯示」或「重新學習」。

3. 空白兜底（當日完成狀態）

若查詢結果為 0 張，畫面顯示「🎉 今日複習已完成！」，將「複習按鈕」反灰或標記打勾，並引導點選「學習新單字」。

三、 系統硬性限制與防呆設計
單日單字唯一性：
同一張單字卡在同一個日曆天內最多只會被練習一次。一旦點選「今天已複習」，next_review_at 必定被推到明天或後天，當天絕對不會再次出現在待複習清單中。

每日複習上限（Review Cap）：
每日複習查詢固定加上 LIMIT 30（或 20）。即使使用者一週沒登入導致大量卡片過期（next_review_at <= 今天），系統當天也只會抽出 30 張，避免債務雪崩。

生疏度優先排序：
待複習清單排序設定為 ORDER BY level ASC, next_review_at ASC，確保最生疏的（Lv 1）以及過期最久的卡片最先被推出來複習。

新字批次節流：
即使整本書有 100 個字，「學新卡」單次發放上限為 20 張，避免一口氣標記過多新字導致隔天複習負擔暴增。

| 動作階段     | 當前狀態      | 點擊選項       | 變更後狀態                     | 下次出現時間 (`next_review_at`)   | 說明                    |
| :----------- | :------------ | :------------- | :----------------------------- | :-------------------------------- | :---------------------- |
| **初學標記** | `level: NULL` | **不認識**     | **Lv 1**                       | **今天 + 1 天（明天）**           | 第 1 次（初學）         |
|              | `level: NULL` | **認識但不熟** | **Lv 2**                       | **今天 + 1 天（明天）**           | 跳過 Lv 1，省時間       |
|              | `level: NULL` | **非常熟悉**   | **封存 (`is_archived: true`)** | `NULL`                            | 永久排除，不複習        |
| **日常複習** | **Lv 1**      | 今天已複習     | **Lv 2**                       | **今天 + 1 天（明天）**           | 第 2 次見面             |
|              | **Lv 2**      | 今天已複習     | **Lv 3**                       | **今天 + 1 天（明天）**           | 第 3 次見面             |
|              | **Lv 3**      | 今天已複習     | **Lv 4**                       | **今天 + 1 天（明天）**           | 第 4 次見面             |
|              | **Lv 4**      | 今天已複習     | **Lv 5**                       | **今天 + 2 天（後天）**           | **強制冷卻 1 天**       |
| **滿級驗收** | **Lv 5**      | 今天已複習     | 彈窗：**【不再顯示】**         | **封存 (`is_archived: true`)**    | 第 5 次（完全掌握畢業） |
|              | **Lv 5**      | 今天已複習     | 彈窗：**【重新學習】**         | **Lv 1**（下次：**今天 + 1 天**） | 重新回到第一級循環      |

MVP 之後可優化方向

1. 登入登出 SSR 閃爍問題
2. refresh token 實踐
