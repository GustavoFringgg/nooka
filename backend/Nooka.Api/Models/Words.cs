namespace Nooka.Api.Models;

public class Word
{
    public int Id { get; set; }
    public int CategoryId { get; set; }                    // 歸屬分類(書)
    public string Term { get; set; } = string.Empty;       // 英文單字,例:"vicarious"
    public string DefinitionCN { get; set; } = string.Empty; // 中文意思
    public string DefinitionEN { get; set; } = string.Empty; // 英文意思
    public string PartOfSpeech { get; set; } = string.Empty; // 詞性,例:"形容詞"
    public List<string> Examples { get; set; } = new();     // 例句,可多筆
    public string? Ipa { get; set; }
}
