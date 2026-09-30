namespace Nooka.Api.Models.Dtos.Request;

public class WordUpsertRequest
{
    public required Word Word { get; set; }
    public List<int> CategoryIds { get; set; } = new();
}