namespace Nooka.Api.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // "多益用"
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

}