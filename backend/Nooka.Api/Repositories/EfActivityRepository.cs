using Microsoft.EntityFrameworkCore;
using Nooka.Api.Data;
using Nooka.Api.Models;
using Nooka.Api.Models.Dtos.Response;

public class EfActivityRepository : IActivityRepository
{
    private readonly AppDbContext _context;

    public EfActivityRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<ActivityStreakResponse> GetStreakSummaryAsync(int userId)
    {
        var activityDates = await _context.DailyActivities
        .Where(da => da.UserId == userId)
        .Select(da => da.ActivityDate)
        .Distinct()  // 去重
        .OrderBy(d => d)
        .ToListAsync();

        if (activityDates.Count == 0) return new ActivityStreakResponse(0, 0);

        // 最長紀錄:逐一比對「這天」是不是「前一天 + 1」,是就延續這段連續區間,不是就重新起算
        int longestStreak = 1;
        int runLength = 1;
        for (int i = 1; i < activityDates.Count; i++)
        {
            if (activityDates[i] == activityDates[i - 1].AddDays(1))
            {
                runLength++;
            }
            else
            {
                runLength = 1;
            }
            longestStreak = Math.Max(longestStreak, runLength);
        }

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
            return new ActivityStreakResponse(0, longestStreak);
        }

        int currentStreak = 0;
        while (dateSet.Contains(cursor))
        {
            currentStreak++;
            cursor = cursor.AddDays(-1);
        }
        return new ActivityStreakResponse(currentStreak, longestStreak);
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

