
using Nooka.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Nooka.Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


// 註冊 Identity 服務
builder.Services.AddIdentityCore<AppUser>()
    .AddRoles<AppRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();


builder.Services.AddControllers();
builder.Services.AddScoped<IWordRepository, EfWordRepository>();
builder.Services.AddScoped<ICategoryRepository, EfCategoryRepository>();
builder.Services.AddScoped<IWordProgressRepository, EfWordProgressRepository>();
// 已 DI 處理，:「註冊以後有人要求 IWordRepository,就給他 EfWordRepository 的實例」
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddAuthorization(); // 註冊另一個獨立的服務:「授權」系統本身的基礎設施
builder.Services.AddCors(options =>  // 註冊一定要在 builder 之前
{
    options.AddPolicy("NuxtDev", policy =>
    {
        policy.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        // 前端呼叫 /api/auth/me、/api/auth/logout 時要帶 credentials: 'include',讓瀏覽器把 cookie 一起送過去
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
//  AddAuthentication() 註冊的驗證機制,指定用「JWT Bearer」這一種驗證方式,options 是這個驗證方式的設定物件
{
    options.TokenValidationParameters = new TokenValidationParameters
    // TokenValidationParameters 是「收到一個 token 之後,要用什麼規則檢查它是不是有效」的設定包
    {
        ValidateIssuer = true, // 要檢查 token 的發行者(issuer)欄位
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        // ValidIssuer: 告訴它「正確的發行者應該是什麼」,值從 appsettings.Development.json 的 Jwt:Issuer("NookaApi")讀進來。如果 token 裡的 issuer 跟這個不一樣,判定無效
        ValidateAudience = true, // 要檢查 token 的 Audience 欄位
        ValidAudience = builder.Configuration["Jwt:Audience"],
        // 同樣邏輯,但檢查的是「受眾」(audience,這個 token 是發給誰用的),對應 Jwt:Audience("NookaClient")
        ValidateIssuerSigningKey = true, // 要驗證這個 token 的簽章是不是用「我方」的密鑰簽的(防止有人偽造 token)
        IssuerSigningKey = new SymmetricSecurityKey(
        // 把 Jwt:Key 這個字串轉成位元組陣列,包成 SymmetricSecurityKey(對稱金鑰),當作驗證簽章用的密鑰——這把密鑰之後產 token 的時候也會用同一把來簽,所以兩邊要一致
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        ValidateLifetime = true,
        //要檢查 token 有沒有過期(看 token 裡的 exp 欄位跟現在時間比較)。
    };
    //TokenValidationParameters 設定結束



    options.Events = new JwtBearerEvents
    // Events 是 JWT Bearer 中介軟體在處理請求過程中,可以插入自訂邏輯的「掛勾點」集合,這裡要覆寫其中一個
    {
        OnMessageReceived = context =>
        // OnMessageReceived 是其中一個掛勾點,時機是「一收到請求,還沒開始找 token 之前」就會觸發。預設行為是去 Authorization: Bearer xxx 這個 header 找 token,這裡要換掉這個預設行為
        {
            context.Token = context.Request.Cookies["access_token"];
            // 這次請求的 cookie 裡,把名字叫 access_token 的那個值挖出來,直接指定給 context.Token——等於告訴中介軟體「不用去 header 找了,token 在這裡」
            return Task.CompletedTask;
            // OnMessageReceived 這個事件的簽名要求回傳一個 Task,因為框架設計成可以是非同步操作(例如去外部服務查東西);這裡沒有任何非同步動作,所以直接回傳一個已完成的 Task,滿足型別要求就好
        }
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("NuxtDev");
// UseCors 是中介軟體(middleware),ASP.NET Core 的中介軟體是照寫的順序一個一個執行,CORS 檢查一定要放在 UseAuthorization/MapControllers 之前,不然請求都被後面的檢查擋掉了,CORS 規則根本沒機會生效
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
    // AppRole 這個 C# 類別,對應到 AspNetRoles 這張表(class ↔ table)
    String[] roles = { "Admin", "User" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new AppRole { Name = role });
        }
    }
}
app.Run();
