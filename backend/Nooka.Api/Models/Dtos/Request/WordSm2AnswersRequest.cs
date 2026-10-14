namespace Nooka.Api.Models.Dtos.Request;

public record WordSm2AnswersRequest(string Mode, List<WordAnswerRequest> Answers);