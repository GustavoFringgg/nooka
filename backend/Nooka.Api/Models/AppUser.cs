using Microsoft.AspNetCore.Identity;
namespace Nooka.Api.Models;

public class AppUser : IdentityUser<int>
{ }


// IdentityUser<int>
// - Id(int):主鍵
// - UserName / NormalizedUserName:使用者名稱跟它的大寫正規化版本(Identity 內部查詢用正規化版本比對,避免大小寫造成查不到)
// - Email / NormalizedEmail:Email 跟正規化版本
// - EmailConfirmed(bool):Email 是否已驗證——這邊用 Google 登入,可以直接視為 true
// - PasswordHash:密碼的雜湊值——不會用到,永遠是 null
// - SecurityStamp:一個隨機字串,密碼或安全性設定變更時會更新,用來讓舊 token/舊登入狀態失效
// - ConcurrencyStamp:並行控制用的戳記,防止兩個請求同時更新同一筆資料互相覆蓋
// - PhoneNumber / PhoneNumberConfirmed:手機號碼跟是否驗證
// - TwoFactorEnabled(bool):是否啟用兩步驟驗證
// - LockoutEnd / LockoutEnabled:帳號鎖定機制(例如密碼連續輸錯太多次鎖住),欄位存到什麼時候解鎖
// - AccessFailedCount(int):登入失敗次數計數器,搭配 Lockout 機制用