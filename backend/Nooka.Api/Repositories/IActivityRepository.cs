using Nooka.Api.Models.Dtos.Response;

public interface IActivityRepository
{
    Task RecordRoundCompleteAsync(int userId, string mode);
    Task<ActivityStreakResponse> GetStreakSummaryAsync(int userId);
    Task<List<ActivityRecentDayResponse>> GetRecentAsync(int userId, int days);
}