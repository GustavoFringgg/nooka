namespace Nooka.Api.Models.Dtos.Response;

public record AdminWordResponse(int Id, string Term, string DefinitionCN, string DefinitionEN, string PartOfSpeech, List<string> Examples, string? Ipa, List<int> CategoryIds);