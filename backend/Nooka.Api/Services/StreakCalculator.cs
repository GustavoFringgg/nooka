using Nooka.Api.Models;
using Nooka.Api.Models.Dtos.Response;

namespace Nooka.Api.Services;

public static class StreakCalculator
{
    public static ActivityStreak Calculate(List<DailyActivity> rows, DateOnly today)
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
}