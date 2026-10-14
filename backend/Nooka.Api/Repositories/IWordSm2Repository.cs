using Nooka.Api.Models;
using Nooka.Api.Models.Dtos.Request;

public interface IWordSm2Repository
{
    Task RecordAnswersAsync(int userId, string mode, List<WordAnswerRequest> answers);
    Task<List<Word>> GetWeightedRandomWordsAsync(int userId, string mode, int categoryId, int count);
}