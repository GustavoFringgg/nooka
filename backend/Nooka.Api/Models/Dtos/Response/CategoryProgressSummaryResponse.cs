namespace Nooka.Api.Models.Dtos.Response;

public record CategoryProgressSummaryResponse(int CategoryId, string CategoryName, int Familiar, int Learning, int NewWords, int DueToday);

//TODO: C# record 的 primary constructor 參數會自動變成 public 屬性,所以用 PascalCase(CategoryId 而不是 categoryId)