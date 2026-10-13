public interface IActivityRepository
{
    Task RecordRoundCompleteAsync(int userId, string mode);
    Task<int> GetStreakAsync(int userId);
}