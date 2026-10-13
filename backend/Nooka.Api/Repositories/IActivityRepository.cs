using Nooka.Api.Models.Dtos.Response;

public interface IActivityRepository
{
    Task RecordRoundCompleteAsync(int userId, string mode);
    Task<ActivitySummaryResponse> GetActivitySummaryAsync(int userId, int recentDays = 14, int heatmapDays = 371);
}