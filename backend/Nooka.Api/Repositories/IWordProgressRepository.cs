using Nooka.Api.Models;
using Nooka.Api.Models.Dtos;

public interface IWordProgressRepository
{
    Task<List<WordProgress>> GetByCategoryAsync(int userId, int categoryId);
    Task BatchUpsertAsync(int userId, List<WordProgressUpdate> updates);
    Task<List<CategoryProgressSummary>> GetSummaryAsync(int userId);

}