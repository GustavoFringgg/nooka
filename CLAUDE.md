# Nooka

英文單字卡片學習應用(flashcard learning app),需要會員登入機制。

## 問答溝通偏好

- 使用者(Derek)在學習後端概念,問題會一個一個接續著問。**只回答被問到的問題本身,不要延伸額外概念、不要主動補充「你可能也會想知道」的內容**,避免增加認知負荷。
- 使用者會自己把他的理解回饋過來確認對不對,由他主導要往哪個方向延伸,不用預先幫他鋪陳。

## 技術棧

- **Backend**: ASP.NET Core (.NET 10), `backend/Nooka.Api`
  - **DTO 命名慣例(2026/09/30 定案)**:`Models/Dtos/` 底下的 DTO,規範「前端傳進來的格式」結尾用 `Request`(例如 `WordUpsertRequest`),規範「回傳給前端的格式」結尾用 `Response`(例如 `CategoryProgressSummaryResponse`);一個檔案一個型別,檔名跟型別名一致,不特別拆 `Interfaces`/`Implementations` 子資料夾(跟 `Repositories/` 一樣,數量少就平放,靠 `I` 前綴/`Request`/`Response` 字尾區分即可)
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
- **學習演算法**:選擇題/打字拼寫採用 **SM-2 啟發的 EF 加權抽選**(2026/10/14 定案,細節見下方「E. 學習紀錄」與 `plan.md`),單字卡維持既有 Leitner 分級(見 A-1),兩套系統並存、互不取代
  - 使用者輸入簡化為「答對 / 答錯」二選一(不做 0~5 分難易度自評 UI)
  - 答對 / 答錯各對應一組固定的 quality 分數(答對=5、答錯=2),餵進 SM-2 的 ease factor(EF)更新公式:答對 EF +0.1、答錯 EF -0.32,初始值 2.5、下限 1.3
  - **不採用 SM-2 原本「依日期排程複習」的機制**(不維護 interval/下次複習日期):討論後認為天數排程的體驗跟單字卡(Leitner,天數制)重複,且會導致「今天到期單字複習完就沒題目可練」,跟產品想鼓勵一天多輪練習(呼應 `/progress` 熱力圖設計)衝突
  - 改成:每個單字只維護一個 `EF` 值,每輪出題時依 `weight = 1 / EF` 做**加權隨機抽選**(EF 越低/越常答錯,被抽中機率越高;EF 越高/越常答對,機率越低但仍有機會,不會變成 0)。刻意**不做保證抽中**(例如「答錯的字下一輪一定出現」),純機率版本保留隨機性與樂趣
- **開發階段**:先做 MVP(會員 + 卡片學習 + SM-2),AI 功能列為 Phase 2,MVP 完成上線後再疊加
  - **接下來的開發順序(2026/09/28 定案,2026/10/13 調整)**:**C(內容管理 Admin 後台)→ D(完成 /progress 真資料串接)→ E(學習紀錄 SM-2)→ B(會員系統補 refresh token)→ A(學習核心收尾:發音/RWD/loading 畫面等)→ H(部署)**。C 排在 D 前面雖然不算 MVP 範圍,但原因是先有 Admin 後台才能好好管理單字內容,不用手動戳 Supabase,之後開發/測試 SM-2 演算法時比較方便新增調整單字。2026/10/13 在原本的 D(SM-2)前面插入新的 D(完成 /progress 真資料串接),原 D 往後推成 E,原 E/F/G(介面/測試/部署)依序推成 F/G/H——原因是 `/progress` 統計列可以直接用現有 `GET /api/progress/summary` 加總,不需要等 SM-2 定案就能做完,順手先把這塊收掉。2026/10/19:**H(部署)已提前完成並上線**(B 的 refresh token 仍待補,A 的收尾項目也還有剩,見下方對應大項),中間順手插入了 `/overview` 總覽頁真資料串接(不在原排序內,見「已完成的基礎建設」)。
  - **AI 功能方向(2026/08/20 討論)**:鎖定以下兩個,都是「後台/演算法輔助,使用者無感」性質
    1. **AI 輔助產生卡片內容**:AI 讀 DB 現有分類/單字(避免重複、抓上下文)生成新單字的中英釋義、例句、詞性,存 DB 標記「待審核」,審核管道兩個都要保留:(a)內容管理 Admin 後台 UI(見下方 C 大項),(b)LINE Messaging API 推播通知 + 按鈕核准(用 postback 夾帶單字 ID,不 parse 純文字回覆,避免多筆待審時對不上),核准後才正式發布
       - **現成資料源(2026/10/15 調查)**,減少 AI 瞎生、降低人工 curate 成本:釋義用 **Free Dictionary API**(`dictionaryapi.dev`,包裝自 Wiktionary);分級單字表用 **Oxford 3000/5000** 或 **CEFR-J wordlist**(對應 JLPT N5~N1 分級的概念);例句用 **Tatoeba**(本身就有英中例句對);詞頻/常用度用 **COCA** 或 **SUBTLEX** 詞頻清單;音標(`ipa` 欄位)用 **CMU Pronouncing Dictionary** 或開源 `ipa-dict`,不用手動輸入;發音朗讀已用瀏覽器內建 Web Speech API(`typing.vue` 已實作),不用額外找工具
    2. **語意向量(embedding)挑選干擾項**:每個單字的釋義預先算好 embedding 存 DB,出題時用 cosine similarity 挑語意相近的單字當選擇題干擾項,取代目前隨機挑同分類單字的做法;這步是一次性預計算 + 純數學比對,不用每次出題都即時呼叫 AI API
    - **刻意不做**:AI 造句批改(使用者直接跟 AI 互動的功能)— 這種會被大量呼叫、目前沒有收費計畫,先不考慮
- **Word ↔ Category 關聯**:已改為方案 B(多對多,`WordCategories` 中介表,複合主鍵 `WordId`+`CategoryId`,2026/09/17 完成遷移),`Word` 不再有 `CategoryId` 外鍵
- **首頁書架**(2026/08/19 定案):「首頁書架」(登入後瀏覽分類)不另外做頁面,直接沿用 `/practice` 選書頁承擔這個角色,不做兩個平行的選書畫面;暫不加登入保護 middleware,等會員系統做出來再補
  - **流程簡化(2026/08/31 確認)**:原規劃的三段式流程(選書 → 分類單字列表頁 → 選練習模式 → 測驗頁)簡化成兩段式,獨立的分類單字列表頁已刪除,書架頁右側面板直接顯示單字預覽 + 開 `UModal` 選方向/題數,確認後直接跳測驗頁 — 維持這個合併式流程,不救回獨立列表頁
  - **nav 標籤(2026/09/28 調整)**:`/practice` 的 nav 標籤從「練習」改成「書架」(這頁本質是書架瀏覽,練習只是次要動作);「學習紀錄」連結從佔位的 `#` 改成實際的 `/progress`
- **學習紀錄頁面統計列粒度(2026/09/28 定案)**:`/progress`(個人總覽)的統計列(連續天數/已精熟/學習中/今天待複習)代表**跨全部書的總計**;`/practice` 書架的每本書封面**另外**顯示這本書自己的三個數字(單字數/已學數/待複習數),兩者粒度不同、並存不互斥,細節見 `learningRecord.md`

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
- 未登入 = 單字卡純瀏覽模式已完成(不能標記熟悉度、不記錄進度)
- `/progress` 學習紀錄頁面**全部完成串真實 API(2026/10/13)**:統計列(連續天數+最長紀錄/已熟悉/學習中/今天待複習,跨全部書總計,數字有從 0 滾動的 count-up 動畫)+ 近 14 天長條圖(選擇題/打字拼寫兩色柱狀)+ 全年練習熱力圖(GitHub 貢獻格樣式),已熟悉/學習中/今天待複習打 `GET /api/progress/summary` 前端加總,連續天數/長條圖/熱力圖打合併後的單一端點 `GET /api/activity/summary`(細節見下方 D 大項);未登入時**不渲染學習紀錄區塊**,只顯示登入引導卡(`v-if="isLoggedIn"`/`v-else`),原本「blur + 降透明度背景疊登入卡片」的呈現方式已拿掉,因為 `/overview` 已經有登入引導的職責,不用在這頁重複;細節見 `learningRecord.md`
- `/practice` 書架書封進度標示已完成:左上角橘色圓形角標顯示今日待複習張數(>0 才顯示,跟右上角選中書籤角標分開避免重疊)、書名下方進度條(白底 + 螢光綠填色,已學/總字數比例)+ 文字數字;直接沿用既有 `GET /api/progress/summary`(回傳每本書的 Familiar/Learning/NewWords/DueToday),不用等後端補新 API
- Admin 後台 API 全部完成,合併在單一 `AdminController`(跟 `plan.md` 原規劃的兩個獨立 controller 不同,功能等價):分類 CRUD(`GET/POST/PUT/DELETE /api/admin/categories`)、單字 CRUD + 分頁查詢(`GET/POST/PUT/DELETE /api/admin/words`,`PUT` 在同一個 transaction 裡 diff 增減 `WordCategories` 關聯)、`Word.Term`/`Category.Name` 都有 DB unique index,違反時回傳 409;全部 `[Authorize(Roles = "Admin")]`
- Admin 後台前端:`/admin` middleware + `AppNav.vue`「資料管理」條件式連結已完成;`/admin/categories` 分類管理頁已完成串 API(列表/新增/編輯/刪除);`/admin/words` 單字管理頁已完成新增/編輯/刪除串 API,列表可展開看例句,分類篩選(只顯示有單字的分類)、關鍵字搜尋(400ms debounce)都已接上 `GET /api/admin/words` 查詢,**分頁 UI 還沒做**(2026/10/12 定案:管理後台用傳統分頁 component,不做無限捲動——無限捲動對瀏覽內容才有意義,管理後台需要看到總筆數;分頁 component 排到 MVP 結束後再做,目前先把前端 `pageSize` 跟後端上限都調大,一次撈全部頂著)
- 選擇題/打字拼寫 SM-2 啟發的 EF 加權抽選**全部完成(2026/10/15)**:`WordSm2Progress` 表(`UserId`+`WordId`+`Mode` 複合鍵,只存 `EF`)、`POST /api/WordSm2/answers`(批次更新 EF)、`GET /api/WordSm2/words`(依 `weight = 1/EF` 加權隨機抽選出題);`choice.vue`/`typing.vue` 登入時改打這兩支 API,未登入維持原本公開 API 跟隨機出題;`typing.vue` 額外加了「同一字錯滿 3 次(含跳過)才算不熟悉」的容錯規則;細節見 `plan.md`
- `/overview` 總覽頁**全部完成串真實資料(2026/10/16)**:登入狀態改用真的 `useIsLoggedIn()`(取代假的 `useDemoLoggedIn()`,該 composable 已刪除);單字庫總數/主題書架數直接打現成的 `GET /api/words`/`GET /api/categories` 算 `.length`;「已學字數」格改方向顯示**連續學習天數**(沿用 `/progress` 同一支 `GET /api/activity/summary` 的 `streak.currentStreak`,視覺做成數字疊在放大火焰 emoji 中間);「每日一字/每日一句」改成純前端靜態 JSON(`frontend/app/data/dailyWords.json`,30 筆)、`onMounted` 用 `Math.random()` 隨機選一筆,跟 `Words` 資料表無關,各使用者看到的可能不同;「最容易忘記的 5 個字」新增後端 `GET /api/WordSm2/weakest?mode=Choice&count=5`(`EfWordSm2Repository.GetWeakestWordsAsync`,篩 `EF < 2.5` 初始值、`OrderBy(EF).ThenBy(Term)`,回傳帶 `EF` 的 `WeakestWordResponse` DTO),前端用 `(2.5-EF)/(2.5-1.3)*100` 算進度條百分比;部署、分階段細節見 git 歷史上的 `plan.md`(2026/10/16 commit)

---

## A. Nooka 學習核心(練習模式)

狀態:書架、選擇題已完成串真實後端;打字拼寫前端第一版完成;單字卡分級(Lv1~5)前端邏輯+ UI 已完成,進度串 DB 正在進行(見 `plan.md`)

1. 單字卡 — 書架瀏覽 + 單字卡資訊(`practice/index.vue`,已完成)
2. 選擇題 — 出題/作答/計分/答錯詳解(`choice.vue`,已完成)
3. 打字拼寫 — 逐字母輸入/發音/複習清單(`typing.vue`,前端邏輯完成)
4. 單字卡分級練習 — 翻牌 + 熟悉度三選一 + Leitner Lv1~5 複習(`flashcard.vue`+`useFlashcardProgress.ts`,前端邏輯完成,規則見下方「A-1」)

0903 mvp 完成 尚缺

1. ~~單字卡進度目前存在 local storage,DB 化 + 未登入純瀏覽模式 + 會員進度總覽頁面進行中~~ → 已完成(DB 化、未登入純瀏覽、`/progress` 假資料版學習紀錄頁面皆已完成,見 `plan.md`、`learningRecord.md`);`/progress` 統計列/長條圖/熱力圖改真資料,見下方「D. 完成 /progress 真資料串接」
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

狀態:後端 CRUD API 全部完成;前端分類管理頁已完成;單字管理頁新增/編輯/刪除、分類篩選、關鍵字搜尋都已完成,**分頁 component 還沒做**

1. ~~分類(書)CRUD API + 後台 UI~~ → 已完成,見下方「已完成的基礎建設」
2. 卡片(單字)CRUD API → 已完成;後台 UI 的新增/編輯/刪除、分類篩選(只顯示有單字的分類)、關鍵字搜尋(debounce 400ms)都已完成,`/admin/words` 列表分頁 UI(頁碼按鈕 + 總筆數)排到 MVP 結束後再做,目前先一次撈全部資料(前端 `pageSize` 調大、後端上限也調大)頂著,不做無限捲動
3. ~~`/admin` middleware(限 `Admin` 角色)~~ → 已完成

(未來)開放一般使用者自行建立卡片/卡片組;卡片加入「常用片語/慣用搭配詞」(collocation)欄位 — MVP 先用多筆例句(`Examples`)頂著,片語需人工 curate 內容成本高,暫不排入 MVP。

## D. 完成 /progress 真資料串接

狀態:**已完成(2026/10/13)**,細節見 `learningRecord.md`

1. ~~統計列(連續天數/已精熟/學習中/今天待複習)串真資料~~ → 已完成:已精熟/學習中/今天待複習打 `GET /api/progress/summary` 前端加總;連續天數(含最長紀錄)打新增的 `GET /api/activity/summary`
2. ~~新增 `DailyActivity` 表~~ → 已完成:`UserId`+`ActivityDate`+`Mode`(`"Choice"`/`"Typing"`字串)複合主鍵 + `CompletedRounds`,選擇題/打字拼寫「完整跑完一輪」時由 `POST /api/activity/round-complete` +1
3. ~~連續練習天數(streak)計算邏輯~~ → 已完成,含「最長紀錄」(歷史上最長連續區間,不限於目前這段)
4. 近 14 天長條圖、全年熱力圖也一併完成,三者(連續天數/長條圖/熱力圖)原本規劃三支獨立 API,後來合併成一支 `GET /api/activity/summary`(一次回傳 `{ streak, recent, heatmap }`),理由是這三個都只有 `/progress` 這頁在用、資料源都是 `DailyActivity`,合併可以把這頁的網路來回從 4 次砍到 2 次(另一次是 `GET /api/progress/summary`),避免依序 `await` 4 支 API 的 waterfall;熱力圖「幾輪對應幾級」的門檻(`heatmapLevel`)先用參考值(0/1-2/3-4/5-6/7+),之後可依實際使用情況調整
5. 跟 E 大項(SM-2)的差異:這裡只是「有沒有完成一輪練習」的活動日誌,不涉及 SM-2 的 ease factor/interval 計算,兩者互相獨立,不互相卡進度

## E. 學習紀錄(SM-2 啟發的 EF 加權抽選)

狀態:**已完成**

1. ~~新增 `WordSm2Progress` 表~~ → 已完成:`UserId`+`WordId`+`Mode` 複合鍵,只存 `EF`(天數排程版的 `Interval`/`NextReviewDate`/`Repetitions` 已拿掉)
2. ~~答對/答錯 → quality 分數(5/2)→ EF 更新 API~~ → 已完成:實作時發現不需要保留 quality 這個中間值,直接寫死答對 `EF×1.05`、答錯 `EF×0.85`(比例式,下限 1.3);`POST /api/WordSm2/answers` 批次更新
3. ~~出題邏輯~~ → 已完成:`choice.vue`/`typing.vue` 登入時改打 `GET /api/WordSm2/words`,依 `weight = 1/EF` 加權隨機抽選,取代原本隨機/全部撈;未登入維持原本公開 API;`restartQuiz` 登入時會重新打 API 反映最新 EF
4. ~~複習排程呈現~~、~~NextReviewDate 索引規劃~~ → 已不適用(改成 EF 加權抽選後不再有日期排程概念);討論後決定**不額外呈現 EF/熟悉度資訊**,因為 `/progress` 學習紀錄頁(D 大項)已足夠覆蓋
5. `typing.vue` 額外加了容錯規則:同一字錯滿 3 次(含跳過)才算 `isCorrect: false`,避免打字手誤被過度懲罰,細節見 `plan.md`

## F. 介面(Landing + 書架殼)

狀態:landing page、書架頁改版已完成

1. Landing page(單頁式:影片 Hero + Night Palette 內容區,已完成)
2. 書架頁殼(米白配色、書架排版,已完成;loading 畫面、答對/答錯音效待補)
3. `design_handoff_velorah_hero/` 設計稿其餘頁面套用(待處理)
4. 吉祥物狗狗(Nooka):全域浮動元件掛在 `app.vue` 根層,跨頁面持續存在;平常在畫面上亂跑(GSAP 隨機移動),進測驗頁(choice/typing)時切成待機不動(用全域狀態如 Pinia store 控制模式),待實作規劃

## G. 測試與 CI/CD

1. 單元測試(Unit):後端用 xUnit 測商業邏輯(例如之後的 SM-2 計算),前端用 Vitest 測 utils/quiz.ts 這類純函式(buildQuizQuestions/buildTypingQuestions)
2. 整合測試(Integration):後端 API 層,用 WebApplicationFactory 打真實/測試用 DB,驗證 Controller + EF Core + Repository 串起來對不對
3. E2E 測試:前端用 Playwright 之類的工具,模擬使用者走完一個流程(登入 → 選書 → 練習 → 看結果)

CI/CD 把關方式:GitHub Actions 在 PR 階段跑上述測試,擋合併進 main;Vercel(前端)、Cloud Run(後端,需另寫 build+deploy workflow)都只監聽 main 的 push 自動部署,等於用「擋合併」間接把關部署,不是讓 Vercel 自己跑測試。

## H. 部署與基礎建設

狀態:**已完成(2026/10/19)**,前後端皆已上線(不同網域)

1. ~~前端 Vercel 設定~~ → 已完成:`https://nooka-learning.vercel.app`,API base 透過 `NUXT_PUBLIC_API_BASE` 環境變數覆寫 `nuxt.config.ts` 預設的 `http://localhost:5016`
2. ~~後端 Google Cloud Run 設定~~ → 已完成:新增 `backend/Nooka.Api/Dockerfile`(多階段建置,`mcr.microsoft.com/dotnet/sdk:10.0` build → `aspnet:10.0` runtime,`ASPNETCORE_HTTP_PORTS=8080`)
3. ~~CORS / Cookie(`SameSite=None; Secure`)設定~~ → 已完成:`Program.cs` CORS policy 明確列出 Vercel 網域(`WithOrigins` + `AllowCredentials`);`AuthController` 的 `access_token` cookie 依 `IWebHostEnvironment.IsDevelopment()` 切換 ——本機開發 `Secure=false`/`SameSite=Lax`,正式環境 `Secure=true`/`SameSite=None`(跨網域 cookie 必須如此瀏覽器才會夾帶),`Logout` 刪除 cookie 時也帶上相同設定,否則本機設的 cookie 刪不掉正式環境的
4. (暫緩)Redis 快取 / refresh token 撤銷名單 — 等 Cloud Run 開多 instance 或需要撤銷機制時再評估,現階段可用內建 `IMemoryCache`

---

### A-1 單字卡練習模式(前端已完成,進度 DB 化進行中)

**核心設計**(已定案並實作):

- 核心互動:翻牌 + 自評,不是單純瀏覽
- 進入方式:比照選擇題/打字拼寫,點「單字卡」跳 UModal;跟選擇題/打字拼寫不同的是**沒有題數 slider**,改成「學習新單字」/「複習已學過的單字」二選一(依 `useFlashcardProgress` 算出的 new/due 數量顯示)
- 切換下一張:按鈕 + 鍵盤方向鍵(跟 `choice.vue` 一致)
- 翻牌動畫:GSAP(`rotateY` + `backface-visibility: hidden`,驗證於 `cardTest.vue` 原型頁,正式實作在 `flashcard.vue`)
- 複習演算法:**不用 SM-2**——SM-2 留給 E 大項(學習紀錄)給選擇題/打字拼寫用,單字卡另外用一套自訂等級(Leitner 分級)系統,兩者並存、互不取代

**實作現況**:

- 前端邏輯 + UI 已完成:`frontend/app/composables/useFlashcardProgress.ts`(分級狀態機)+ `frontend/app/pages/practice/[categoryId]/flashcard.vue`(翻牌互動、三選一/複習按鈕、Lv5 滿級彈窗)+ `frontend/app/types/practice.ts` 的 `FlashcardLevel`/`FlashcardProgress` 型別
- 進度已改存後端 `WordProgress` 表(`useFlashcardProgress.ts` 打 `/api/progress/*` 系列 API),不再用 localStorage
- 未登入使用者已改為純瀏覽單字卡(不能標記熟悉度、不記錄進度)
- 「學習紀錄」頁面(`/progress`)已完成假資料版(不曝露內部 Lv1~5 數字,顯示已精熟/學習中/今天待複習等統計 + 趨勢圖表);**改真資料串接還沒做**,詳細分階段步驟見專案根目錄 `plan.md`、頁面範圍定案見 `learningRecord.md`

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

1. ~~登入登出 SSR 閃爍問題~~ → 已完成(2026/09/24,見 commit `d9e1f1f`):新增 `frontend/app/plugins/auth.ts`,在 app 初始化時(SSR + client 都會跑)就呼叫 `fetchMe()`,不用等個別頁面各自觸發;`useApiFetch` 也改成伺服器端呼叫時用 `useRequestHeaders(["cookie"])` 把瀏覽器的 cookie 轉傳過去,不然 SSR 階段打 API 沒有 cookie 可用,會被當成未登入
2. refresh token 實踐
