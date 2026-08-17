using Nooka.Api.Models;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync(); //取得所有書本
    Task<Category?> GetByIdAsync(int id); //取得單一書本
}