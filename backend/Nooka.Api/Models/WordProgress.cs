namespace Nooka.Api.Models;

public class WordProgress
{
    public int UserId { get; set; }
    public int WordId { get; set; }
    public int? Level { get; set; }
    public bool IsArchived { get; set; }
    public DateOnly? NextReviewAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

}