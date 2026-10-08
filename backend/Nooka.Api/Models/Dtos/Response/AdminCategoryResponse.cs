namespace Nooka.Api.Models.Dtos.Response;

public record AdminCategoryResponse(int Id, string Name, string? Description, string? Color, DateTime UpdatedAt, int WordCount);
