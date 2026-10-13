using Microsoft.EntityFrameworkCore;
using Nooka.Api.Data;
using Nooka.Api.Models;

public class EfActivityRepository : IActivityRepository
{
    private readonly AppDbContext _context;

    public EfActivityRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<int> GetStreakAsync(int userId)
    {
        var activityDates = await _context.DailyActivities
        .Where(da => da.UserId == userId)
        .Select(da => da.ActivityDate)
        .Distinct()  // 去重
        .ToListAsync();

        var dateSet = activityDates.ToHashSet();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var yesterday = today.AddDays(-1);

        DateOnly cursor;
        if (dateSet.Contains(today))
        {
            cursor = today;
        }
        else if (dateSet.Contains(yesterday))
        {
            cursor = yesterday;
        }
        else
        {
            return 0;
        }

        int streak = 0;
        while (dateSet.Contains(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }
        return streak;
    }


    public async Task RecordRoundCompleteAsync(int userId, string mode)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var existing = await _context.DailyActivities.FirstOrDefaultAsync(da => da.UserId == userId && da.ActivityDate == today && da.Mode == mode);

        if (existing != null)
        {
            existing.CompletedRounds++;
        }
        else
        {
            _context.DailyActivities.Add(new DailyActivity
            {
                UserId = userId,
                ActivityDate = today,
                Mode = mode,
                CompletedRounds = 1
            });
        }
        await _context.SaveChangesAsync();
    }
}

