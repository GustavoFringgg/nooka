namespace Nooka.Api.Models.Dtos;

public record WordProgressUpdate(int WordId, int? Level, bool IsArchived, DateOnly? NextReviewAt);