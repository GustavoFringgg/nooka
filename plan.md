# 學習紀錄真資料串接(D 大項)— /progress 統計列 + 長條圖 + 熱力圖

> 目標:把 `/progress` 頁面(目前是純前端假資料版,見 `learningRecord.md`)換成真資料。這是開發順序 **C → D → E → B → A → H**(見 `CLAUDE.md`)的第二步,排在 E(學習紀錄 SM-2)前面,因為統計列不需要等 SM-2 定案就能做完。
>
> **狀態(2026/10/13):規劃完成,尚未開始寫程式。**

## 為什麼另開這個任務(取代原本 plan.md 內容)

`plan.md` 原本記錄的是「內容管理 Admin 後台」(C 大項)那個任務,已經全部完成並 commit(細節留在 git 歷史裡;無限捲動分頁後來改成延後到 MVP 後做,見 `CLAUDE.md`)。現在開始的是全新的一輪任務(D 大項:完成 `/progress` 真資料串接),plan.md 內容整個換掉。

## 討論定案的內容

### 範圍(依使用者角度,從上到下依序完成)

`/progress` 頁面分三塊(版面定案見 `learningRecord.md`):

- **上(統計列)**:依序完成 ① 連續練習天數 ② 已熟悉 ③ 學習中 ④ 今天待複習
- **中**:近 14 天練習量長條圖(選擇題/打字拼寫兩色柱狀)
- **下**:全年練習熱力圖(GitHub 貢獻格樣式)

長條圖/熱力圖/連續天數**只算選擇題 + 打字拼寫,不含單字卡**(單字卡沒有「一輪」的概念,`flashcard.vue` 不會呼叫任何活動紀錄 API)。

### 新資料表:`DailyActivity`

- 欄位:`UserId`(int)、`ActivityDate`(DateOnly)、`Mode`(string:`"Choice"` / `"Typing"`)、`CompletedRounds`(int)
- 複合主鍵:`UserId` + `ActivityDate` + `Mode`
- 「完整跑完一輪」的定義(沿用 `learningRecord.md`):題目全部作答完才算 1 次,中途離開不算
- 索引規劃:`UserId` + `ActivityDate`(方便查最近 N 天/算連續天數)

### 已熟悉 / 學習中 / 今天待複習:不用新 API

直接用現有 `GET /api/progress/summary`(回傳每個分類的 `Familiar`/`Learning`/`NewWords`/`DueToday`),前端把所有分類加總即可,不用動後端。

### API 新增

- `POST /api/activity/round-complete`:body `{ mode: "Choice" | "Typing" }`,登入後依 JWT 的 UserId,把當天那筆 `DailyActivity` 的 `CompletedRounds` +1(沒有就新建)
- `GET /api/activity/streak`:回傳目前連續練習天數(從今天往前逐日檢查 `DailyActivity` 有沒有任一 Mode 的紀錄,中斷就停)
- `GET /api/activity/recent?days=14`:回傳最近 N 天,每天 choice/typing 各自完成輪數
- `GET /api/activity/heatmap`:回傳全年每天 choice+typing 合計完成輪數

## 分階段步驟(依「上 → 中 → 下」UI 區塊順序)

### Stage 0 — 後端:`DailyActivity` 資料表

- 新增 `Models/DailyActivity.cs`,`AppDbContext` 設定複合鍵 + 索引
- migration:`dotnet ef migrations add AddDailyActivity`

### Stage 1 — 後端 + 前端:連續練習天數

- `IActivityRepository`/`EfActivityRepository`:`RecordRoundCompleteAsync(userId, mode)`(upsert +1)、`GetStreakAsync(userId)`
- 新增 `ActivityController`:`POST /api/activity/round-complete`、`GET /api/activity/streak`,都要登入(從 JWT 拿 UserId)
- 前端:`choice.vue`/`typing.vue` 在題目全部作答完的那一刻,打 `POST /api/activity/round-complete`(對應模式 `Choice`/`Typing`)
- 前端:`progress.vue` 連續天數卡改打 `GET /api/activity/streak`,拔掉假資料
- 驗證:選擇題/打字拼寫各跑完一輪,確認連續天數 +1;中途離開測驗不會誤記;隔天沒練習,確認連續天數斷掉歸零

### Stage 2 — 前端:已熟悉 / 學習中 / 今天待複習

- `progress.vue` 這三張卡改打現有 `GET /api/progress/summary`,前端加總所有分類的 `Familiar`/`Learning`/`DueToday`,拔掉假資料
- 驗證:在 `/admin/words` 或練習頁改變某個字的進度狀態,確認這三個數字會跟著變動

### Stage 3 — 後端 + 前端:近 14 天長條圖

- `EfActivityRepository` 加 `GetRecentAsync(userId, days)`
- `ActivityController` 加 `GET /api/activity/recent?days=14`
- 前端:`progress.vue` 長條圖改打這支 API,拔掉假資料
- 驗證:選擇題跑完一輪,確認今天那根「選擇題」柱子 +1,「打字拼寫」柱子不動

### Stage 4 — 後端 + 前端:全年熱力圖

- `EfActivityRepository` 加 `GetHeatmapAsync(userId)`
- `ActivityController` 加 `GET /api/activity/heatmap`
- 前端:`progress.vue` 熱力圖改打這支 API,拔掉假資料
- 驗證:今天格子的深淺(或是否點亮)隨完成輪數變化

## 涉及檔案(還沒動工,先列出預期會碰到的)

**後端**

- `backend/Nooka.Api/Models/DailyActivity.cs`(新增)
- `backend/Nooka.Api/Controllers/ActivityController.cs`(新增)
- `backend/Nooka.Api/Repositories/IActivityRepository.cs`、`EfActivityRepository.cs`(新增)
- `backend/Nooka.Api/Models/Dtos/Request/ActivityRoundCompleteRequest.cs`(新增)
- `backend/Nooka.Api/Models/Dtos/Response/ActivityStreakResponse.cs`、`ActivityRecentResponse.cs`、`ActivityHeatmapResponse.cs`(新增,視實作時要不要合併再調整)
- `backend/Nooka.Api/Data/AppDbContext.cs`(修改,加 `DailyActivity` 設定)
- `backend/Nooka.Api/Migrations/`(新增:`AddDailyActivity`)

**前端**

- `frontend/app/pages/practice/[categoryId]/choice.vue`(修改,跑完一輪打 round-complete)
- `frontend/app/pages/practice/[categoryId]/typing.vue`(修改,同上)
- `frontend/app/pages/progress.vue`(修改,四個區塊依序拔假資料換真資料)

## 驗證方式

- 選擇題/打字拼寫各完整跑完一輪(中途離開的不算),確認連續天數、長條圖對應柱子、熱力圖今天格子都正確更新
- 已熟悉/學習中/今天待複習三個數字跟 `/practice` 書架書封上的進度數字(同一來源 `GET /api/progress/summary`)加總後對得起來
- 未登入狀態下 `/progress` 維持原本的 blur + 登入卡片呈現,不受這次改動影響

完成後回頭更新 `CLAUDE.md`:D 大項狀態改成完成,補上活動紀錄 API + `/progress` 真資料串接到「已完成的基礎建設」,並把開發順序往下推進到 E(SM-2)。

TODO: 待學習資訊

ASP.NET Identity + JWT 登入/登出

1. 為什麼是在 Nooka.Api.http 測試 為什麼是在這裡測試 Nooka.Api.http 在這個專案扮演什麼樣的角色
2. http第二個 應該只是測試api 還沒有帶token進去 去驗證
3. 登出 要怎麼判斷這個 token 過期
4. 那這樣我 httpOnly 我實際是把 token 存在哪裡? 機制是什麼 檢查token的機制是怎麼看得
