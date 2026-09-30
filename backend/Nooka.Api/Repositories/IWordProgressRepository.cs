using Nooka.Api.Models;
using Nooka.Api.Models.Dtos.Request;
using Nooka.Api.Models.Dtos.Response;

public interface IWordProgressRepository
{
    Task<List<WordProgress>> GetByCategoryAsync(int userId, int categoryId);
    Task BatchUpsertAsync(int userId, List<WordProgressUpdateRequest> updates);
    Task<List<CategoryProgressSummaryResponse>> GetSummaryAsync(int userId);

}