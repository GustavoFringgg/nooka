using Nooka.Api.Models;
using Nooka.Api.Models.Dtos.Request;
using Nooka.Api.Models.Dtos.Response;

public interface IWordSm2Repository
{
    Task RecordAnswersAsync(int userId, string mode, List<WordAnswerRequest> answers);
    Task<List<Word>> GetWeightedRandomWordsAsync(int userId, string mode, int categoryId, int count);
    Task<List<WeakestWordResponse>> GetWeakestWordsAsync(int userId, string mode, int count);
}