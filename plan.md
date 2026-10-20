# 測試 + CI/CD pipeline

> 目標:不是追求測試覆蓋率,而是**證明有能力建置一套可運作的 CI/CD pipeline**。只挑最重要、最有邏輯故事性的幾個案例來測,兩邊(前後端)都不用全面覆蓋。
>
> **狀態(2026/10/20):討論定案,尚未開始實作。**

## 為什麼另開這個任務(取代原本 plan.md 內容)

`plan.md` 原本記錄的是 `/overview` 總覽頁真資料串接,已經全部完成並 commit(細節留在 git 歷史裡,摘要已收進 `CLAUDE.md` 的「已完成的基礎建設」)。現在開始的是全新的一輪任務(補測試 + 接 CI/CD),plan.md 內容整個換掉。

## 討論定案的內容

- **範圍**:不用前後端全面補測試,只挑**少數、但測到真實商業邏輯**的案例,避免「湊數字測沒意義的程式碼」或「花費不成比例的時間」
- **CI gate**:只要求測試**跑得過**(pass/fail),不設覆蓋率門檻
- **執行順序**:後端 xUnit → 前端 Vitest → GitHub Actions CI(接在補完測試之後,一次把 pipeline 跑起來)
- **挑選的測試案例**(各邊 2 個,都是已經有明確規則、討論過的邏輯):
  - **後端 xUnit**:
    1. SM-2 EF 更新公式(答對 `EF×1.05`、答錯 `EF×0.85`,下限 1.3)— 目前邏輯內嵌在 `EfWordSm2Repository.RecordAnswersAsync` 裡面,跟 `DbContext` 查詢混在一起,不好單獨測;需要先抽出一個**不依賴 DB 的 pure 方法**(例如 `Sm2EfCalculator.ApplyAnswer(currentEf, isCorrect)`),抽出後 `RecordAnswersAsync` 改呼叫這個方法,邏輯不變,但可以不開 DB 直接測
    2. 連續天數(streak)計算 — `EfActivityRepository.CalculateStreak` 本身已經是純邏輯(只吃 `List<DailyActivity>`,沒有碰 `DbContext`),只是目前是 `private`,需要改成 `internal` 或抽成獨立的 static class,並在測試專案的 csproj 加 `InternalsVisibleTo`(或直接抽出)才能從測試專案呼叫到
  - **前端 Vitest**:
    1. `buildQuizQuestions`(`frontend/app/utils/quiz.ts`)— 已經是 pure function,不用改動,直接測
    2. `buildTypingQuestions`(同檔案)— 同上,直接測

## 分階段步驟

### Stage 0 — 決策定案 → 已完成

本次討論出的所有設計決定,見上方「討論定案的內容」。

### Stage 1 — 後端 xUnit 專案建立 + SM-2 EF 更新公式測試 → 已完成

- ~~新增測試專案 `backend/Nooka.Api.Tests`(xUnit),加進 `Nooka.slnx`~~ → 已完成
- ~~從 `EfWordSm2Repository.RecordAnswersAsync` 抽出 pure 方法~~ → 已完成:`Services/Sm2EfCalculator.cs` 的 `Sm2EfCalculator.ApplyAnswer(float currentEF, bool isCorrect)`,`RecordAnswersAsync` 改呼叫它,行為不變,原本內嵌的 `MinEF`/`CorrectMultiplier`/`IncorrectMultiplier` 常數已從 repository 刪除
- ~~測試案例~~ → 已完成:`Sm2EfCalculatorTests.cs` 3 個 `[Fact]`(答對 ×1.05、答錯 ×0.85、已在下限時答錯仍鎖在 1.3),`dotnet test` 全過

### Stage 2 — 後端 xUnit streak 計算測試 → 已完成

- ~~把 `EfActivityRepository.CalculateStreak` 改成可被測試專案呼叫~~ → 已完成:選擇「抽成獨立 static class」這個方案,新增 `Services/StreakCalculator.cs`(`StreakCalculator.Calculate(List<DailyActivity> rows, DateOnly today)`),額外把原本內部呼叫 `DateTime.UtcNow` 取得「今天」的部分改成外部傳入的 `today` 參數,讓 pure 邏輯不碰系統時間,測試才能自己指定日期;`EfActivityRepository.GetActivitySummaryAsync` 改呼叫 `StreakCalculator.Calculate(rows, DateOnly.FromDateTime(DateTime.UtcNow))`,原本的 `private CalculateStreak` 方法已刪除
- ~~測試案例~~ → 已完成:`StreakCalculatorTests.cs` 3 個 `[Fact]`(連續累加到今天、中間斷一天後 current 重算但 longest 保留歷史最長、今明兩天都沒紀錄時 current 歸零但 longest 不洗掉),`dotnet test` 全過(累計 6 個測試)
- 順手把 Stage 1 遺留的 `Microsoft.EntityFrameworkCore` 版本衝突警告修掉:`Nooka.Api.csproj` 的 `Microsoft.EntityFrameworkCore.Design` 從 `10.0.11` 改成跟其他 EF 相關套件一致的 `10.0.9`

### Stage 3 — 前端 Vitest 設定 + quiz.ts 測試

- 加裝 `vitest`(`frontend/package.json` 新增 devDependency + `test` script),不需要 `@vue/test-utils`(測的是純函式,不是元件)
- 測試案例:
  - `buildQuizQuestions`:選項數量固定是 4(1 正確 + 3 干擾)、正確答案一定在選項裡、`count` 有帶時回傳數量符合
  - `buildTypingQuestions`:`prompt` 對應到 `word.definitionCN`、`count` 有帶時回傳數量符合

### Stage 4 — GitHub Actions CI

- 新增 `.github/workflows/ci.yml`,PR 觸發(`pull_request` 對 `main`/`develop`)
- 兩個 job(或兩個 step):`dotnet test`(後端)+ `npm run test`(前端),皆為 CI gate,不設覆蓋率門檻
- 跑過才能合併(GitHub repo 的 branch protection 設定,需要 GitHub 網站操作,不是 workflow 檔案本身能設定的部分,到時候另外說明怎麼點)

## 跟 CLAUDE.md「G. 測試與 CI/CD」的關係

這次只做 G 大項裡的「單元測試(Unit)」+ CI 串接部分,**不做**「整合測試(Integration,`WebApplicationFactory` 打真實/測試用 DB)」跟「E2E 測試(Playwright)」—— 這兩塊成本較高,且不是這次「證明會接 pipeline」的目的,先不排入。
