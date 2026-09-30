# 內容管理 Admin 後台(C 大項)— 分類 / 單字 CRUD

> 目標:做出 `/admin/categories`、`/admin/words` 兩頁後台管理介面 + 對應後端 CRUD API,讓使用者不用手動戳 Supabase 就能管理分類跟單字。這是接下來開發順序 **C → D → B → A → G**(見 `CLAUDE.md`)的第一步,做完才回頭做 D(SM-2 學習紀錄),因為要先能好好管理單字內容,測試 SM-2 時新增/調整單字才方便。
>
> **狀態(2026/09/28):UI 設計討論完成,尚未開始寫程式。** UI mockup(分類列表、單字列表、單字編輯抽屜、分類編輯抽屜共 4 個畫面)見 Design 畫布:https://claude.ai/artifact/SF2bV9dReQ6HcFS5sqhyrM

## 為什麼另開這個任務(取代原本 plan.md 內容)

`plan.md` 原本記錄的是「單字卡分級系統接後端 + 會員進度總覽」那個任務(Stage 0~8),那個任務**已經全部完成並 commit**(細節留在 git 歷史裡)。現在開始的是全新的一輪任務(C 大項:Admin 後台),plan.md 內容整個換掉,不用保留舊任務的分階段記錄。

## 討論定案的內容

### UI

- **視覺風格**:沿用前台 paper 色票(跟書架/nav 同一套色票字型),但排版改成後台常見的密集表格/列表,不套用書架那套視覺隱喻
- **版面結構**:左側固定 nav(分類管理/單字管理兩個入口)+ 右側主內容區(表格 + 右上角「+ 新增」按鈕)
- **新增/編輯共用同一張表單**:差別只在標題文字(「新增分類」vs「編輯分類」),欄位、儲存邏輯完全共用
- **表單呈現方式**:右側抽屜(從畫面右邊滑出的長型面板),不是置中 Modal——欄位多時可以往下捲動,不會擁擠,且抽屜開著時左邊列表還看得到
- **分類表單欄位**:名稱、描述(多行)、顏色(8 個預設色票 + `<input type="color">` color picker + 色碼文字輸入,三種方式都能選)
- **單字表單欄位**:單字(term)、KK 音標(ipa)、詞性(下拉選單,對應現有 8 種詞性)、英文釋義、中文釋義、例句(可新增多筆,一筆一個輸入框 + 刪除按鈕)、所屬分類(打勾多選,對應多對多的 `WordCategories`)
- **刪除**:一律要二次確認彈窗,不能點了就直接刪
- **單字列表**:用無限捲動,不做分頁 UI(但後端查詢還是分頁式)

### 後端 API

- **路由分離**:新增 `/api/admin/categories`、`/api/admin/words`(`POST`/`PUT`/`DELETE`),跟現有唯讀的 `GET /api/categories`、`GET /api/words` 分開,不混在同一個 controller,方便統一在 Admin controller 上加權限
- **分頁**:`GET /api/admin/words?page=&pageSize=20&categoryId=&search=`,依 `Term` 字母排序,支援分類篩選 + 關鍵字搜尋(前端無限捲動時遞增 `page` 呼叫)
- **單字 ↔ 分類關聯寫入**:前端**只打一支** `PUT /api/admin/words/{id}`,body 直接帶這個單字現在應該屬於哪些分類(`categoryIds: number[]`)。後端在**同一個 transaction** 裡:① 更新單字本身欄位、② 把現有 `WordCategories` 關聯跟新的 `categoryIds` 做差異比對,多的刪掉、少的補上。這個 transaction 寫法沿用 `EfWordProgressRepository.BatchUpsertAsync` 已經驗證過的模式,不是全新概念
- **單字名稱重複**:DB 端在 `Word.Term` 建 unique index 當最終防線,違反時 catch `DbUpdateException` 轉成 `409 Conflict` + 明確錯誤訊息;前端**不**額外打一支「即時查重複」的 API,直接在儲存失敗時把後端回傳的錯誤訊息顯示在「單字」欄位下面
- **分類刪除不 cascade 刪單字**:單字是獨立字典實體,一個單字可以同時屬於多本書(多對多),分類被刪除時只刪對應的 `WordCategories` 關聯列,單字本身保留(失去這個分類歸屬,變成沒有分類的自由單字),`WordProgress` 學習紀錄完全不受影響。正常情況下單字很少被整筆刪除(字典性質,只會修改/新增),所以這不是常見操作但邏輯上要處理對
- **權限**:`/api/admin/*` 全部要 `[Authorize(Roles = "Admin")]`

### 前端路由與導覽

- **nav 顯示條件**:「資料管理」連結只有 `useAuthUser().roles` 包含 `"Admin"` 時才顯示在「學習紀錄」右邊,一般 `User` 角色看不到、不會意識到後台存在
- **連結指向**:`/admin/categories`(分類管理跟單字管理是兩個獨立路由:`/admin/categories`、`/admin/words`,不是同一頁切 tab)
- **路由保護**:非 Admin(不管有沒有登入)直接輸入網址硬進 `/admin/*`,一律導回首頁 `/`,不做 404 頁面(簡單,也不用糾結要不要暴露路由存在)

### 明確排除、不做的事

- **不做多路由 SEO**(例如 `/practice/toeic` 這種每本書獨立網址):現在 `/practice` 是單一網址 + 前端 state 切換書,`Category.description` 這個欄位先當純內容說明用,不接 meta description。理由:現階段使用者靠帳號登入使用,不是靠 Google 搜尋導流,SEO 效益低,之後真的要做流量成長再回頭處理

## 分階段步驟(建議順序,後端 API 先於前端頁面)

### Stage 0 — Admin Controllers 骨架

- 新增 `Controllers/AdminCategoriesController.cs`、`Controllers/AdminWordsController.cs`,`[Authorize(Roles = "Admin")]`,`[Route("api/admin/categories")]`/`[Route("api/admin/words")]`
- 先寫空的 action 簽名(`POST`/`PUT`/`DELETE`),確認路由跟權限擋得住(用非 Admin 帳號打應該 403,不帶 token 應該 401)

### Stage 1 — 分類 CRUD

- `ICategoryRepository`/`EfCategoryRepository` 加 `CreateAsync`/`UpdateAsync`/`DeleteAsync`(欄位:`Name`/`Description`/`Color`,`CreatedAt`/`UpdatedAt` 沿用 DB `DEFAULT now()` 慣例不用手動帶)
- 刪除分類時只刪 `WordCategories` 裡對應的關聯列,不動 `Words` 表
- 驗證:用 `.http`/Swagger 測三支,確認刪除分類後底下的單字還在(只是少了這個分類標籤)

### Stage 2 — 單字分頁查詢

- `IWordRepository`/`EfWordRepository` 加分頁版查詢方法,支援 `page`/`pageSize`/`categoryId`/`search`,依 `Term` 排序
- Controller 加 `GET /api/admin/words`(這支可以跟唯讀 API 共用邏輯,只是多了 Admin 權限跟分頁參數;或直接複用現有 `GET /api/words` 的查詢再疊加分頁——實作時再決定要不要重複造輪子)
- 驗證:造超過一頁的測試資料,確認 `page=2` 能拿到下一批,`search`/`categoryId` 篩選正確

### Stage 3 — 單字 CRUD + 分類關聯 transaction

- `IWordRepository`/`EfWordRepository` 加 `CreateAsync`/`UpdateAsync`/`DeleteAsync`
- `UpdateAsync` 內:讀現有 `WordCategories`、跟傳入的 `categoryIds` diff、`AddRange`/`RemoveRange`,整包包在 `_context.Database.BeginTransactionAsync()`(或用 EF Core 7+ 的 execution strategy)
- `Word.Term` 加 unique index(要新增一支 migration),`UpdateAsync`/`CreateAsync` catch `DbUpdateException` 轉 409
- 驗證:新增/編輯單字帶不同 `categoryIds` 組合,確認 `WordCategories` 正確增減;故意新增重複單字名稱,確認回 409 而不是 500

### Stage 4 — 前端:`/admin` 路由保護 + nav 顯示

- 新增 `frontend/app/middleware/admin.ts`,檢查 `useAuthUser()?.roles` 是否包含 `"Admin"`,沒有就 `navigateTo("/")`
- `AppNav.vue`:「資料管理」連結只在 `roles` 含 `Admin` 時渲染,`to: "/admin/categories"`

### Stage 5 — 前端:分類管理頁

- 新增 `frontend/app/pages/admin/categories.vue`,套用 mockup 的列表 + 右側抽屜表單(名稱/描述/顏色)
- 刪除加二次確認 `UModal`

### Stage 6 — 前端:單字管理頁

- 新增 `frontend/app/pages/admin/words.vue`,列表(無限捲動,滾到底加載下一頁)+ 篩選列(分類下拉 + 搜尋框)+ 右側抽屜表單(含例句動態新增/刪除、分類多選)
- 刪除加二次確認 `UModal`

## 涉及檔案(還沒動工,先列出預期會碰到的)

**後端**

- `backend/Nooka.Api/Controllers/AdminCategoriesController.cs`(新增)
- `backend/Nooka.Api/Controllers/AdminWordsController.cs`(新增)
- `backend/Nooka.Api/Repositories/ICategoryRepository.cs`、`EfCategoryRepository.cs`(修改,加 CUD 方法)
- `backend/Nooka.Api/Repositories/IWordRepository.cs`、`EfWordRepository.cs`(修改,加分頁查詢 + CUD 方法)
- `backend/Nooka.Api/Migrations/`(新增:`Word.Term` unique index)
- `backend/Nooka.Api/Nooka.Api.http`(新增測試請求)

**前端**

- `frontend/app/middleware/admin.ts`(新增)
- `frontend/app/pages/admin/categories.vue`(新增)
- `frontend/app/pages/admin/words.vue`(新增)
- `frontend/app/components/AppNav.vue`(修改,加「資料管理」條件式連結)
- `frontend/app/types/auth.ts`/`practice.ts`(視情況新增 Admin CRUD 相關型別)

## 驗證方式

- 非 Admin 帳號(或未登入)打 `/api/admin/*` 應該 401/403;直接網址硬進 `/admin/*` 應該被導回首頁
- Admin 帳號能新增/編輯/刪除分類跟單字,列表跟練習頁(`/practice`)看到的資料同步更新
- 刪除分類後,原本屬於這個分類的單字還能在單字管理列表查到(只是分類標籤消失)
- 新增重複單字名稱應該顯示明確錯誤訊息,不是白畫面或 500
- 單字管理頁捲到底能載入下一頁,搜尋/分類篩選正常運作

完成後回頭更新 `CLAUDE.md`:C 大項狀態改成完成,補上 Admin API + 後台頁面到「已完成的基礎建設」,並把開發順序往下推進到 D(SM-2)。

TODO: 待學習資訊

ASP.NET Identity + JWT 登入/登出

1. 為什麼是在 Nooka.Api.http 測試 為什麼是在這裡測試 Nooka.Api.http 在這個專案扮演什麼樣的角色
2. http第二個 應該只是測試api 還沒有帶token進去 去驗證
3. 登出 要怎麼判斷這個 token 過期
4. 那這樣我 httpOnly 我實際是把 token 存在哪裡? 機制是什麼 檢查token的機制是怎麼看得
