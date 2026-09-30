namespace Nooka.Api.Models.Dtos.Request;

public record WordProgressUpdateRequest(int WordId, int? Level, bool IsArchived, DateOnly? NextReviewAt);