namespace Nooka.Api.Models.Dtos.Response;

public record PagedResponse<T>(List<T> Items, int TotalCount);