namespace Nooka.Api.Models;

public class WordSm2Progress
{
    public int UserId { get; set; }
    public int WordId { get; set; }
    public string Mode { get; set; } = string.Empty;
    public float EF { get; set; }
}