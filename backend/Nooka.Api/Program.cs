
using Nooka.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddScoped<IWordRepository, EfWordRepository>();
builder.Services.AddScoped<ICategoryRepository, EfCategoryRepository>();
// 已 DI 處理，:「註冊以後有人要求 IWordRepository,就給他 EfWordRepository 的實例」
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>  // 註冊一定要在 builder 之前
{
    options.AddPolicy("NuxtDev", policy =>
    {
        policy.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod();
    });
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
