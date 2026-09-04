
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
// 已 DI 處理，:「註冊以後有人要求 IWordRepository,就給他 EfWordRepository 的實例」
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddAuthorization(); // 註冊另一個獨立的服務:「授權」系統本身的基礎設施
builder.Services.AddCors(options =>  // 註冊一定要在 builder 之前
{
    options.AddPolicy("NuxtDev", policy =>
    {
        policy.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        ValidateLifetime = true,
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
app.UseAuthorization();
app.MapControllers();

app.Run();
