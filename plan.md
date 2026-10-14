# 選擇題/打字拼寫 SM-2 學習紀錄(E 大項)

> 目標:幫選擇題(`choice.vue`)、打字拼寫(`typing.vue`)加上 SM-2 間隔重複演算法,決定每個單字下次該在什麼時候被複習。這是開發順序 **D → E → B → A → H**(見 `CLAUDE.md`)的第三步。
>
> **狀態(2026/10/14):設計討論中,尚未開始實作。** 以下是目前已經討論定案的內容。

## 為什麼另開這個任務(取代原本 plan.md 內容)

`plan.md` 原本記錄的是 D 大項(`/progress` 真資料串接)的任務,已經全部完成並 commit(細節留在 git 歷史裡)。現在開始的是全新的一輪任務(E 大項:選擇題/打字拼寫 SM-2),plan.md 內容整個換掉。

## 討論定案的內容

### 範圍與適用對象

- SM-2 只套用在**選擇題、打字拼寫**(`choice.vue`/`typing.vue`);**單字卡維持既有的 Leitner 分級系統**(`flashcard.vue`,見 `CLAUDE.md` A-1),兩套系統並存、互不取代
- 選擇題(辨識單字意思)跟打字拼寫(主動拼出單字)是不同的技能,**不能共用同一套 SM-2 狀態**,否則簡單技能的表現會誤導困難技能的複習排程
- 結論:同一個使用者、同一個單字,會有 **3 筆獨立進度紀錄**(跨 2 張表):
  - `WordProgress`(既有表,單字卡 Leitner 用)1 筆
  - 新表(選擇題/打字拼寫 SM-2 用)2 筆,用 `Mode` 欄位區分

### 新資料表(表名待 Stage 1 決定)

- 跟 `WordProgress` **分開**,不共用,因為 Leitner 跟 SM-2 是兩套互相獨立的複習邏輯,共用會難以維護
- 複合主鍵:`UserId` + `WordId` + `Mode`(`Mode`:`"Choice"` / `"Typing"`,同一個單字可以有兩筆,各自獨立複習排程)
- 欄位:
  - `EF`(ease factor,float,預設 2.5,下限 1.3,標準 SM-2 規則)
  - `Repetitions`(int,連續答對次數,答錯歸零)
  - `Interval`(int,目前複習間隔天數)
  - `NextReviewDate`(DateOnly,下次該複習的日期)

### Quality 對照(答對/答錯二元輸入 → SM-2 quality 分數)

- 使用者只有「答對/答錯」二元輸入(架構決策已定案,不做 0~5 自評 UI),所以不用使用者作答秒數去動態判斷 quality(討論過,秒數在選擇題/打字拼寫基準不同、閾值難訂,先不做,之後真的需要再疊加)
- 固定對照:**答對 → quality = 5**(EF +0.1)、**答錯 → quality = 2**(EF -0.32)
  - 原本考慮答對用 quality = 4(EF 持平),但這樣 EF 只會因為答錯下降、永遠無法回升,不符合 EF 本質上該雙向調整(反映「這個字現在對使用者來說多難」,應該能隨表現變好或變差)的設計——改成 5 之後,EF 下降(-0.32)比回升(+0.1)快,是「跌容易、爬難」的不對稱設計,視為合理

### Interval 計算規則

- **答對**(quality ≥ 3):`Repetitions` + 1,`Interval` 依公式成長(第 1 次 1 天、第 2 次 6 天、第 3 次以後「前一次 Interval × EF」),但**設上限 60 天**(考量使用者不會每天都使用系統,避免排到太遙遠的未來)
- **答錯**(quality < 3):`Repetitions` 歸零、`Interval` 打回 1 天 —— 採用**標準 SM-2 的直接歸零行為**,不做「漸進縮短」的客製化版本(討論過漸進縮短的方案,因為比較難維護、且偏離標準演算法語意,決定不採用)

### 索引規劃(待 Stage 2 細化)

- 常用查詢:「某使用者在某 Mode 下,`NextReviewDate` ≤ 今天」的到期單字清單
- 複合主鍵 `UserId + WordId + Mode` 的 leftmost prefix 是 `UserId`,但 `WordId` 夾在中間、`NextReviewDate` 不在鍵裡,這個查詢效率有限;單字量變大時可能要額外加 `(UserId, Mode, NextReviewDate)` 索引,實作時再評估是否需要

## 分階段步驟

### Stage 0 — 決策定案 → 已完成

本次討論出的所有設計決定,見上方「討論定案的內容」。

### Stage 1 — 後端:新增 SM-2 複習狀態表

- 新增 Model + migration,`AppDbContext` 設定複合鍵

### Stage 2 — 後端:SM-2 計算邏輯 + API

- Repository:記錄答案(upsert,算 quality → 更新 EF/Repetitions/Interval/NextReviewDate)、查詢到期單字清單
- Controller:對應的 API endpoint

### Stage 3 — 前端:出題邏輯改成依複習排程選字

- `choice.vue`/`typing.vue` 原本的選字邏輯(目前隨機/全部撈)改成呼叫新 API 取得到期單字來出題
- 作答完(答對/答錯)呼叫記錄 API

### Stage 4 — 前端:複習排程呈現

- 對應 `CLAUDE.md` E-2「複習排程呈現(非單純學過/沒學過二元標記)」,細節待討論

## 涉及檔案(預期,待細化)

## 驗證方式(待細化)

TODO: 待學習資訊

ASP.NET Identity + JWT 登入/登出

1. 為什麼是在 Nooka.Api.http 測試 為什麼是在這裡測試 Nooka.Api.http 在這個專案扮演什麼樣的角色
2. http第二個 應該只是測試api 還沒有帶token進去 去驗證
3. 登出 要怎麼判斷這個 token 過期
4. 那這樣我 httpOnly 我實際是把 token 存在哪裡? 機制是什麼 檢查token的機制是怎麼看得
