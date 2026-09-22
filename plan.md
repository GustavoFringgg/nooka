# 單字卡分級系統(A-1)接後端資料庫 + 會員進度總覽

> 目標:把 `useFlashcardProgress.ts` 現有的 Lv1~5 分級邏輯(初學三選一、複習升級、Lv5 滿級彈窗)從 localStorage 換成存 DB,跟會員帳號綁定;未登入使用者改成純瀏覽單字卡(不能標記熟悉度);新增「學習紀錄」頁面給會員看自己的進度總覽。
>
> **狀態(2026/09/21):Stage 0~3 完成並 commit(WordProgress model/migration/repository/controller)。Stage 4(彙總查詢)進行中,`CategoryProgressSummary` DTO 已定義,repository/controller 尚未寫。明天(9/22)接續。**

## 為什麼

A-1 的 Lv1~5 分級規則(不認識→Lv1、認識但不熟→Lv2、非常熟悉→封存;複習「今天已練習」逐級升,Lv4→Lv5 強制冷卻一天,Lv5 滿級彈窗選畢業/重來)其實已經**做完了**,實作在 `useFlashcardProgress.ts` + `flashcard.vue`,但進度存在瀏覽器 `localStorage`(key: `nooka:flashcard-progress:${categoryId}`)——`useFlashcardProgress.ts` 檔頭註解本來就寫明「之後接後端 API,呼叫端不用改介面」。

現在 Google 登入 + JWT + Identity 的會員系統已經做完(見上一輪 git 歷史),`UserId` 這個硬依賴已經有了,可以把進度真的存進 DB。同時使用者確認:未登入的人只能純瀏覽單字卡(翻牌看意思),不能標記熟悉度、不記錄進度;會員要能在「學習紀錄」看到自己的進度總覽,但不曝露內部 Lv1~5 數字,只顯示分類後的統計(未學過/學習中/已精熟、待複習張數)。

## 分階段步驟(照順序,一步做完驗證完再下一步)

### Stage 0 — `WordProgress` model + `AppDbContext` 設定

- 新增 `backend/Nooka.Api/Models/WordProgress.cs`,比照 `WordCategory.cs` 的複合鍵 pattern:
  ```csharp
  public class WordProgress
  {
      public int UserId { get; set; }
      public int WordId { get; set; }
      public int? Level { get; set; }        // null = 還沒標記過(新字),1~5
      public bool IsArchived { get; set; }
      public DateOnly? NextReviewAt { get; set; }
      public DateTime CreatedAt { get; set; }
      public DateTime UpdatedAt { get; set; }
  }
  ```
- `AppDbContext.cs`:新增 `DbSet<WordProgress> WordProgresses`,`OnModelCreating` 補上複合主鍵 `(UserId, WordId)`、FK → `AppUser`(cascade)、FK → `Word`(cascade)、索引 `(UserId, NextReviewAt)` 和 `(UserId, IsArchived)`,`CreatedAt`/`UpdatedAt` 比照 `Category` 用 `HasDefaultValueSql("now()")`(手動 SQL insert 不用帶這兩欄,沿用既有慣例)。

驗證:`dotnet build` 過,先不跑 migration。

### Stage 1 — Migration

```
dotnet ef migrations add AddWordProgress
dotnet ef database update
```

套用前先看一眼產生的 migration,確認有複合鍵、兩個 FK、對應索引,沒動到 `Words`/`Categories`/`WordCategories`。套用後去 Supabase table editor 肉眼確認新表結構。

### Stage 2 — Repository

`Repositories/IWordProgressRepository.cs` + `EfWordProgressRepository.cs`,比照 `IWordRepository`/`EfWordRepository` pattern:

```csharp
Task<List<WordProgress>> GetByCategoryAsync(int userId, int categoryId);
// join WordProgresses -> WordCategories(WordId) where CategoryId = X and UserId = userId

Task BatchUpsertAsync(int userId, List<WordProgressUpdate> updates);
// updates = 前端這一輪算好的最終狀態(每張卡的 Level/IsArchived/NextReviewAt),逐筆 upsert(存在就 update,不存在就 insert),包在同一個 transaction
```

狀態轉換規則(不認識→Lv1+明天複習、認識但不熟→Lv2+明天複習、非常熟悉→封存;Lv1~3 複習後升一級+明天複習;Lv4 複習後→Lv5+後天複習;Lv5 畢業/重來)**不在後端算**,改成前端沿用 `useFlashcardProgress.ts` 現有的純函式在瀏覽器記憶體裡算完一整輪,後端只負責把算好的最終結果寫進去,不重新驗證這輪的中間過程。

在 `Program.cs` 註冊 `AddScoped<IWordProgressRepository, EfWordProgressRepository>()`。

驗證:先寫一個最小的單元測試或直接在 controller 完成後用 `.http` 測。

### Stage 3 — Controller(mutate 三支 + 查詢一支)

新增 `Controllers/WordProgressController.cs`,`[Authorize]`(全部要登入),`[Route("api/progress")]`,User Id 從 `User.FindFirstValue(ClaimTypes.NameIdentifier)` 取得(比照 `AuthController.Me()`,不從前端傳 userId)。

| Method | Route                                 | 用途                                                                                                                                   |
| ------ | -------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------- |
| GET    | `/api/progress/category/{categoryId}` | 回傳該使用者在這本書的完整進度清單,前端載入這一輪要練習的卡片時用,也給現有的 `getNewWords`/`getDueWords`/`getCounts` 純函式篩選 |
| POST   | `/api/progress/batch`                 | body 是純陣列 `[{ wordId, level, isArchived, nextReviewAt }]`(不包 `{ updates: [...] }`),一輪練習結束(或翻到最後一張)才打一次,把整輪算好的最終狀態一次寫進去 |

中途關頁籤/離開頁面不送出 batch 就當這輪沒發生,不用額外處理「部分送出」或恢復機制。

驗證:用 `Nooka.Api.http` 或 Swagger,帶登入後拿到的 `access_token` cookie 測兩支;不帶 cookie 應該回 401。

**實作備註(跟原規劃的小差異)**:

- 兩支 DTO(`GoogleLoginRequest`、`WordProgressUpdate`)搬到獨立的 `Models/Dtos/` 子資料夾(namespace `Nooka.Api.Models.Dtos`),跟 `Models/` 底下真正對應 DB 表的 entity(`Word`/`Category`/`WordProgress` 等)分開。
- Controller class 實際命名 `ProgressController`(檔名維持 `WordProgressController.cs`),用 `[Route("api/[controller]")]` 慣例自動產生 `api/Progress` 前綴(routing 不分大小寫,不影響前端打 `/api/progress/...`)。
- `POST /api/progress/batch` 的 body 已確認是**純陣列** `[{ wordId, level, isArchived, nextReviewAt }]`,不包一層 `{ updates: [...] }`(上面表格已同步更新)。

### Stage 4 — 彙總查詢 `/api/progress/summary`(進行中)

在 `IWordProgressRepository`/`EfWordProgressRepository` 加 `GetSummaryAsync(int userId)`,跨所有 category 彙總每本書「已精熟(`IsArchived`)/學習中(`Level != null && !IsArchived`)/尚未開始」的數量 + 今天到期(`NextReviewAt <= today`)張數。Controller 加 `GET /api/progress/summary`——**回傳時只回分類後的計數,不回傳原始 `Level` 數字**(前端「學習紀錄」頁不顯示 Lv1~5 這種內部分級)。

**回傳形狀已定案並建好 DTO**(`Models/Dtos/CategoryProgressSummary.cs`):
```csharp
public record CategoryProgressSummary(int CategoryId, string CategoryName, int Familiar, int Learning, int NewWords, int DueToday);
```
一個單字只會落在 `Familiar`/`Learning`/`NewWords` 三者之一(互斥);`DueToday` 不是第四種狀態,是 `Learning` 這群裡再篩 `NextReviewAt <= 今天` 的子集合計數,疊加在 `Learning` 之上,不是獨立一批單字。

**尚未做**(明天接續):`GetSummaryAsync` 的查詢邏輯(`WordProgresses` join `WordCategories` 按 `CategoryId` 分組;`NewWords` 要另外拿 `WordCategories` 算每本書總字數,扣掉已有進度紀錄的數量)+ Controller 的 `GET /api/progress/summary` action。

驗證:標記幾張卡後打這支 API,人工核對計數對不對。

### Stage 5 — `useFlashcardProgress.ts` 換成打 API

保留現有四個純函式(`getNewWords`/`getDueWords`/`getCounts`/`getProgress`)的篩選邏輯不變,`markInitialLearning`/`markReviewed`/`resolveLevel5` 這三個狀態轉換函式的計算邏輯也**不變**,但:

- `loadProgressList` 的 localStorage I/O 換成 `GET /api/progress/category/${categoryId}`(用 `useApiFetch`,`credentials: "include"`),讀進來的資料只存在這個 composable 的記憶體狀態(reactive ref)裡。
- `markInitialLearning`/`markReviewed`/`resolveLevel5` 改成只更新記憶體裡的狀態(邏輯跟原本 localStorage 版本一樣,只是不寫 storage),同時把這筆變動記進一個「待送出」清單(dirty list)。
- 新增 `submitBatch()`,把 dirty list 整理成純陣列,一輪練習的最後一張卡完成時(或使用者主動結束這輪)呼叫一次 `POST /api/progress/batch`;沒呼叫到就等同這輪沒發生。
- 讀取(`loadProgressList`)變非同步要 `await`;三個標記函式本身維持同步(純算記憶體狀態),只有 `submitBatch()` 是非同步。

驗證:先不改 UI,console.log 確認一輪結束時才打出一支帶完整 `updates` 陣列的 batch API,中途點擊三選一/今天已練習不會觸發任何網路請求。

### Stage 6 — 呼叫端調整(`practice/index.vue`、`flashcard.vue`)

- `practice/index.vue`:`flashcardCounts` 從同步 `computed` 改用 `useAsyncData`;`chooseFlashcardMode` 裡的 `isFirstTimeForBook()` 改成 await。
- `flashcard.vue`:`sessionWords` 改用 `useAsyncData` 直接抓,原本為了 localStorage/SSR 不一致包的 `<ClientOnly>` 可以拿掉;在最後一張卡完成、或使用者主動結束這輪(例如按返回書架)時呼叫 `submitBatch()`。

驗證:登入後走一次完整流程(學新字三選一 → 複習「今天已練習」→ Lv5 滿級彈窗),重新整理頁面或換瀏覽器登入同帳號,進度應該還在(證明真的存 DB)。

### Stage 7 — 未登入 = 純瀏覽模式

- `flashcard.vue` 用 `useAuthUser()` 判斷:未登入時不呼叫任何 `/api/progress/*`,單字照表列順序全部顯示,只能翻牌 + 上一張/下一張,不出現三選一按鈕、不出現「今天已練習」按鈕與 Lv 圓點。
- `practice/index.vue` 的單字卡 UModal:未登入時不顯示「學習新單字/複習已學過的單字」這組,改顯示「先看看這本書的單字」瀏覽入口,導去 `flashcard.vue` 的瀏覽模式(例如 `?mode=browse`)。
- 不新增登入保護 middleware(維持書架頁本身不擋登入的既有決定),只在單字卡子功能內用 `useAuthUser()` 做 UI 分支。

驗證:登出後開單字卡,應該只能翻牌瀏覽,看不到任何標記按鈕。

### Stage 8 — 「學習紀錄」頁面

- 新增 `frontend/app/pages/progress.vue`,抓 `GET /api/progress/summary`,顯示每本書「已精熟 X 字 / 學習中 Y 字 / 尚未開始 Z 字」+ 今天待複習張數,不出現 Lv1~5 字眼。
- `AppNav.vue` 把「學習紀錄」的 `to: "#"` 改成 `to: "/progress"`。
- 未登入訪問這頁:比照 `overview.vue` 現有處理登入態的方式,顯示「登入後查看你的學習進度」。

驗證:標記過的書要出現在頁面上且計數正確,未登入訪問要看到登入提示而不是空白/報錯。

## 決策點總表

| 決策                                | 選擇                                                                | 理由                                                                          |
| ----------------------------------- | ------------------------------------------------------------------- | ----------------------------------------------------------------------------- |
| 未登入使用者能不能玩單字卡          | 能,但純瀏覽(翻牌看意思),不能標記熟悉度、不記錄進度                  | 進度本來就要綁會員,沒有帳號就沒有東西可以存;選擇題/打字拼寫維持不用登入       |
| 進度總覽放哪                        | 新頁面 `/progress`,掛在 AppNav 既有的「學習紀錄」佔位連結           | 比 `overview.vue` 的 mock dashboard 更明確對應「學習紀錄」這個既有 nav 入口   |
| 學習紀錄要不要顯示 Lv1~5            | 不顯示,只顯示「已精熟/學習中/尚未開始」+ 待複習張數                 | Lv1~5 是內部演算法分級,對使用者沒有意義,只會增加認知負荷                      |
| LIMIT/排序邏輯放前端還是後端        | 維持在前端(`getNewWords`/`getDueWords` 純函式不變),後端只回完整清單 | 單一使用者單本書的進度筆數不大,MVP 先不做這層效能優化,之後有需要再搬進 SQL    |
| 要不要做 localStorage → DB 資料搬遷 | 不用                                                                | MVP 尚未上線,現有 localStorage 資料是開發測試產生的,直接讓新版本改吃 API 即可 |
| 進度 API 呼叫時機                   | 一輪練習結束才打一次 batch API,不是每次點擊三選一/今天已練習都打    | 頻繁單次呼叫對前後端流量都是不必要負擔;使用者確認不在意「中途關頁籤導致這輪進度遺失」,所以不需要逐步存檔換取容錯 |

## 涉及檔案

**後端**

- `backend/Nooka.Api/Models/WordProgress.cs`(新增)
- `backend/Nooka.Api/Data/AppDbContext.cs`(修改)
- `backend/Nooka.Api/Migrations/`(新增 `AddWordProgress`)
- `backend/Nooka.Api/Repositories/IWordProgressRepository.cs`、`EfWordProgressRepository.cs`(新增)
- `backend/Nooka.Api/Controllers/WordProgressController.cs`(新增)
- `backend/Nooka.Api/Program.cs`(DI 註冊)
- `backend/Nooka.Api/Nooka.Api.http`(新增測試請求)

**前端**

- `frontend/app/composables/useFlashcardProgress.ts`(改寫成打 API)
- `frontend/app/pages/practice/index.vue`(async 化 + 未登入分支)
- `frontend/app/pages/practice/[categoryId]/flashcard.vue`(async 化 + 未登入純瀏覽模式)
- `frontend/app/pages/progress.vue`(新增)
- `frontend/app/components/AppNav.vue`(「學習紀錄」連結)

## 驗證方式

- 登入後標記幾張 Lv1/Lv2,重新整理頁面、或換瀏覽器再登入同一帳號,進度應該還在(證明真的存 DB 而不是 localStorage)。
- 登出後開同一本書單字卡,應該只能翻牌瀏覽,看不到三選一/今天已練習按鈕。
- 「學習紀錄」頁面能看到剛剛標記過的書出現對應的計數,且畫面上不出現 Lv1~Lv5 這種字眼。
- 後端可用 `.http`/Swagger 直接呼叫 `/api/progress/*` 系列 API,確認 `[Authorize]` 生效(未帶 cookie 應該 401)。

完成後回頭更新 `CLAUDE.md`:A-1 狀態改成完成,補上 `WordProgress` 表 + 相關 API 到「已完成的基礎建設」,記錄「未登入 = 純瀏覽,登入才記錄進度」這條規則。

TODO: 待學習資訊

ASP.NET Identity + JWT 登入/登出

1. 為什麼是在 Nooka.Api.http 測試 為什麼是在這裡測試 Nooka.Api.http 在這個專案扮演什麼樣的角色
2. http第二個 應該只是測試api 還沒有帶token進去 去驗證
3. 登出 要怎麼判斷這個 token 過期
4. 那這樣我 httpOnly 我實際是把 token 存在哪裡? 機制是什麼 檢查token的機制是怎麼看得
