namespace Nooka.Api.Models.Dtos.Response;

public class WeakestWordResponse
{
    public int WordId { get; set; }
    public string Term { get; set; } = string.Empty;
    public string DefinitionCN { get; set; } = string.Empty;
    public float EF { get; set; }
}