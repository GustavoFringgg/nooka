using Nooka.Api.Models;
using Nooka.Api.Models.Dtos.Response;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync(); //取得所有書本
    Task<Category?> GetByIdAsync(int id); //取得單一書本
    Task<Category> CreateAsync(Category category);
    Task<Category?> UpdateAsync(int id, Category category);
    Task<bool> DeleteAsync(int id);
    Task<List<AdminCategoryResponse>> GetAllWithWordCountAsync();
}