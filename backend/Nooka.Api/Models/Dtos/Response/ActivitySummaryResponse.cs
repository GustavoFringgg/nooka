namespace Nooka.Api.Models.Dtos.Response;

public record ActivityStreak(int CurrentStreak, int LongestStreak);

public record ActivityRecentDay(DateOnly Date, int Choice, int Typing);

public record ActivityHeatmapDay(DateOnly Date, int Count);

public record ActivitySummaryResponse(
    ActivityStreak Streak,
    List<ActivityRecentDay> Recent,
    List<ActivityHeatmapDay> Heatmap
);
