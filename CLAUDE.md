# Nooka

英文單字卡片學習應用(flashcard learning app),需要會員登入機制。

## 現況

專案剛起步,兩邊都還是預設模板,尚未寫任何業務邏輯:
- `backend/Nooka.Api`:ASP.NET Core Web API 預設模板(`WeatherForecastController` 已移除)
- `frontend/`:Nuxt 4 預設模板(`app.vue` 只有 `NuxtWelcome`)

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

## 使用者故事

### 會員(Membership)

- **註冊**:使用者可以用 Email + 密碼建立帳號
  - 註冊後需完成 **Email 驗證**(收驗證信、點連結)才算正式啟用帳號
- **登入**:使用者可以用 Email + 密碼登入
- **登出**:使用者可以登出,清除登入狀態
- **忘記密碼**:使用者可透過 Email 收到重設密碼連結,導向前端重設密碼頁面完成重設(token 帶在網址上)
- *(未來)* **Google 登入**:使用者可綁定 / 用 Google 帳號登入
  - 透過 Google 登入的帳號視為 Email 已驗證,不需再走 Email 驗證流程

### 單字卡學習(Flashcards)

- **卡片內容**:一般使用者不能自建卡片,只能透過**後台管理介面**由 Admin 新增/編輯(不是寫死在程式碼裡),依主題分類,例如「航空用」「工程用」「資安用」「多益用」
  - 後台路由規劃在同一個 Nuxt 專案裡(例如 `/admin`),用 middleware 檢查 `Admin` 角色才能進入
  - 後端需要一組限 `Admin` 權限的卡片 CRUD API
- **學習紀錄**:帳號的核心用途是記錄使用者自己的學習進度,採 SM-2 間隔重複(見上方架構決策),而非單純「學過/沒學過」的二元標記
- **練習模式**:參考 Quizlet,預計有多種練習方式(細節待後續討論):
  - 消消樂(配對)
  - 選擇題
  - 打字拼寫
- *(未來)* 開放一般使用者自行建立卡片 / 卡片組

### 介面(UI)

- **首頁**:以「書架」概念呈現,每本書 = 一個單字分類(旅遊、工作、商務等)
  - 書封暫不顯示學習進度(32/120、進度條等),先簡單呈現就好,是否需要之後再看
- **點進一本書**:先看到該分類**所有單字的內容列表**,再從中選擇練習模式(消消樂/選擇題/打字拼寫)

## 待辦事項

- [x] 移除 backend 預設的 WeatherForecast 範例程式碼
- [ ] 決定並實作會員登入
- [ ] 設計單字卡資料模型
