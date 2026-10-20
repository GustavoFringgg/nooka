using Nooka.Api.Models;
using Nooka.Api.Services;

namespace Nooka.Api.Tests;

public class StreakCalculatorTests
{
    private static DailyActivity Row(DateOnly date) => new() { ActivityDate = date };
    [Fact]
    public void Calculate_WhenActiveUpToToday_CountsConsecutiveDays()
    {
        var today = new DateOnly(2026, 10, 20);
        var rows = new List<DailyActivity>
        {
            Row(new DateOnly(2026, 10, 18)),
            Row(new DateOnly(2026, 10, 19)),
            Row(today)
        };

        var result = StreakCalculator.Calculate(rows, today);

        Assert.Equal(3, result.CurrentStreak);
        Assert.Equal(3, result.LongestStreak);
    }

    [Fact]
    public void Calculate_WhenStreakWasBrokenByGap_KeepsLongestButResetsCurrent()
    {
        var today = new DateOnly(2026, 10, 20);
        var rows = new List<DailyActivity>
        {
            Row(new DateOnly(2026, 10, 1)),
            Row(new DateOnly(2026, 10, 2)),
            Row(new DateOnly(2026, 10, 3)),
            Row(new DateOnly(2026, 10, 4)),
            Row(new DateOnly(2026, 10, 19)),
            Row(today)
        };

        var result = StreakCalculator.Calculate(rows, today);

        Assert.Equal(2, result.CurrentStreak);
        Assert.Equal(4, result.LongestStreak);
    }

    [Fact]
    public void Calculate_WhenNoActivityTodayOrYesterday_CurrentStreakIsZero()
    {
        var today = new DateOnly(2026, 10, 20);
        var rows = new List<DailyActivity>
        {
            Row(new DateOnly(2026, 10, 10)),
            Row(new DateOnly(2026, 10, 11)),
            Row(new DateOnly(2026, 10, 12))
        };

        var result = StreakCalculator.Calculate(rows, today);

        Assert.Equal(0, result.CurrentStreak);
        Assert.Equal(3, result.LongestStreak);
    }
}