namespace Nooka.Api.Models;

public class DailyActivity
{
    public int UserId { get; set; }
    public DateOnly ActivityDate { get; set; }
    public string Mode { get; set; } = string.Empty;
    public int CompletedRounds { get; set; }
}