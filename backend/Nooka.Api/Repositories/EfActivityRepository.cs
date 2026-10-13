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

    public async Task<ActivitySummaryResponse> GetActivitySummaryAsync(int userId, int recentDays = 14, int heatmapDays = 371)
    {
        var rows = await _context.DailyActivities
        .Where(da => da.UserId == userId)
        .ToListAsync();

        var streak = CalculateStreak(rows);
        var recent = CalculateRecent(rows, recentDays);
        var heatmap = CalculateHeatmap(rows, heatmapDays);

        return new ActivitySummaryResponse(streak, recent, heatmap);
    }
    private ActivityStreak CalculateStreak(List<DailyActivity> rows)
    {
        var activityDates = rows.Select(r => r.ActivityDate).Distinct().OrderBy(d => d).ToList();

        if (activityDates.Count == 0) return new ActivityStreak(0, 0);

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
            return new ActivityStreak(0, longestStreak);
        }

        int currentStreak = 0;
        while (dateSet.Contains(cursor))
        {
            currentStreak++;
            cursor = cursor.AddDays(-1);
        }
        return new ActivityStreak(currentStreak, longestStreak);
    }

    private List<ActivityRecentDay> CalculateRecent(List<DailyActivity> rows, int days)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = new List<ActivityRecentDay>();

        for (int i = days - 1; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var choice = rows.FirstOrDefault(r => r.ActivityDate == date && r.Mode == "Choice")?.CompletedRounds ?? 0;
            var typing = rows.FirstOrDefault(r => r.ActivityDate == date && r.Mode == "Typing")?.CompletedRounds ?? 0;
            result.Add(new ActivityRecentDay(date, choice, typing));
        }
        return result;
    }

    private List<ActivityHeatmapDay> CalculateHeatmap(List<DailyActivity> rows, int days)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = new List<ActivityHeatmapDay>();

        var countByDate = rows
            .GroupBy(r => r.ActivityDate)
            .ToDictionary(g => g.Key, g => g.Sum(r => r.CompletedRounds));

        for (int i = days - 1; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var count = countByDate.GetValueOrDefault(date, 0);
            result.Add(new ActivityHeatmapDay(date, count));
        }
        return result;
    }

}