namespace Nooka.Api.Models.Dtos.Response;

public record ActivityRecentDayResponse(DateOnly Date, int Choice, int Typing);