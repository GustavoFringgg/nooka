# /overview 總覽頁開發

> 目標:把 `frontend/app/pages/overview.vue` 從純假資料的設計稿,改成真的串接後端資料、反映使用者實際狀態的總覽頁。
>
> **狀態(2026/10/16):Stage 0-5 全部完成。** /overview 五個 bento 格都已串真實資料。

## 為什麼另開這個任務(取代原本 plan.md 內容)

`plan.md` 原本記錄的是 E 大項(選擇題/打字拼寫 SM-2 加權抽選)的任務,已經全部完成並 commit(細節留在 git 歷史裡)。現在開始的是全新的一輪任務(`/overview` 總覽頁),plan.md 內容整個換掉。

## 現況

`overview.vue` 六個 bento 格全部是寫死的假資料,登入狀態也是用假的 `useDemoLoggedIn()`(`frontend/app/composables/useDemoAuth.ts`),跟真實登入系統(`useAuthUser()`)沒串接。

## 討論定案的內容

- **登入狀態**:`useDemoLoggedIn()` 換成真的 `useAuthUser()`
- **單字庫總數 / 主題書架數**:直接打現成的 `GET /api/words`、`GET /api/categories` 算,不需要新 API
- **已學字數** → 討論後改成**連續學習天數**(見 Stage 3):跟「已精熟/學習中」統計跟 `/progress` 頁重複,改打 `GET /api/activity/summary` 的 `streak.currentStreak`,視覺上做成「數字包在火焰 emoji 裡」
- **每日一字 / 每日一句**:純前端靜態 JSON(放 `frontend/app/data/`),跟 `Words` 資料表完全無關,`onMounted` 用 `Math.random()` 亂數選一筆 —— 同一時間 A 使用者跟 B 使用者看到的可能不一樣(各自獨立亂數,不是全站同步的「今日單字」)。完全不碰後端。內容先隨便帶幾筆意思到就好,之後再補。
- **最容易忘記的 5 個字**:抓**選擇題(Choice 模式)** 的 `WordSm2Progress`,依 `EF` 升冪排序取前 5,EF 相同時按字母排序。目前 `GET /api/WordSm2/words`(`WordSm2Controller.cs`)是依 `categoryId` 加權隨機抽題用的,沒有「跨全部書、依 EF 排序取最低 N 個」這支,**這塊需要後端新增一支 API**。

## 分階段步驟

### Stage 0 — 決策定案 → 已完成

本次討論出的所有設計決定,見上方「討論定案的內容」。

### Stage 1 — 登入狀態串接真實 auth → 已完成

- `overview.vue` 改用 `useIsLoggedIn()`,template 3 處 `loggedIn` 改名 `isLoggedIn`
- 登入按鈕改成 `router.push("/login")`,對齊 `AppNav.vue` 的 `handleCtaClick` 行為
- `useDemoAuth.ts`(`useDemoLoggedIn()`)已刪除

### Stage 2 — 單字庫總數 / 主題書架數 → 已完成

- `overview.vue` 加 `useFetch<Category[]>`/`useFetch<Word[]>` 打 `/api/categories`、`/api/words`,取 `.length`,取代寫死的 `1240`/`6`
- count-up 動畫 target 改成 `words.value?.length ?? 0`

### Stage 3 — 已學字數 → 改方向,已完成

- **改方向**:討論後發現「已學字數(familiar+learning)」跟 `/progress` 頁的統計列重複,改成顯示**連續學習天數(streak)**,跟右邊「最容易忘記的字」形成「鼓勵 + 提醒」互補
- 串現成的 `GET /api/activity/summary`(`/progress` 頁同一支 API),取 `streak.currentStreak`,取代寫死的 `182`
- 視覺上做了「數字包在火焰裡」的效果:🔥 emoji 放大當背景,數字疊在火焰中間偏下(用 CSS,不額外引入圖片/icon 套件),加 `drop-shadow` 光暈 + `keyframes` 閃爍動畫
- 卡片背景色跟著火焰主題調整(淺綠 → 深紅棕 `#3a1310` + 淺色文字),「每日一字」卡的背景也順手從純黑(`#2e2b25`)換成亮橘金(`#ffcb8a` + 深色文字),都是這次討論中順手調的視覺微調

### Stage 4 — 每日一字 / 每日一句 → 已完成

- 新增 `frontend/app/data/dailyWords.json`(30 筆,單字/中文意思/詞性/英文例句/中文例句)
- `overview.vue` `onMounted` 用 `Math.random()` 從 JSON 挑一筆存進 `dailyWord` ref,template 改用 `dailyWord?.word` 等 optional chaining,取代原本寫死的 5 個 const

### Stage 5 — 最容易忘記的 5 個字 → 已完成

- **篩選邏輯確認(2026/10/15)**:只從有 `WordSm2Progress` 紀錄、且 `EF < 2.5`(初始值)的字裡面取,不到 5 個就顯示實際數量;從沒練過、或 EF 已經回升到 ≥2.5 的字都不列入
- 後端:`IWordSm2Repository`/`EfWordSm2Repository` 新增 `GetWeakestWordsAsync`,用 `WordSm2Progresses` join `Words`,篩 `EF < InitialEF`,`OrderBy(EF).ThenBy(Term)`,`Take(count)`;回傳型別改成新增的 `WeakestWordResponse` DTO(帶 `WordId`/`Term`/`DefinitionCN`/`EF`),因為前端要用 `EF` 換算進度條百分比,單純回傳 `Word` 會把 `EF` 丟掉
- `WordSm2Controller` 新增 `GET /api/WordSm2/weakest?mode=Choice&count=5`(`[Authorize]`,`mode`/`count` 驗證跟 `GetWeightedRandomWords` 同款,**沒有** `categoryId` 參數,因為是跨全部書統計)
- 前端:`overview.vue` 新增 `weakestWords`/`weakestWordBars`,打這支 API,用 `(2.5 - EF) / (2.5 - 1.3) * 100` 算進度條 `pct`(EF 範圍已經被篩選邏輯鎖在 1.3~2.5 之間,直接拿這個區間反推最合理);「錯 N 次」文字拿掉(系統沒存這個數字);未登入模糊預覽改用獨立假資料 `weakWordsMockDataPreview`,不跟真實資料混用;沒有作答紀錄時顯示「還沒有足夠的作答紀錄」

/overview 開發到此全部完成。

## 這次 session 順手做的額外工作(跟 /overview 本身無直接關係)

- `/practice` 書架:書封改直立比例(`aspect-[2/3]`)、一行排 5 本(`grid-cols-5`)而不是 2 本;拿掉左上角「今天待複習」橘色角標跟螢光綠進度條(進度數字呈現方式還在想,先註解掉);書名改貼齊卡片底部
- Admin 後台:新增 `POST /api/admin/words/batch` 批次新增單字 API(整批失敗/整批回滾,不是部分成功),方便手動塞測試單字資料時不用一筆一筆打 Postman
