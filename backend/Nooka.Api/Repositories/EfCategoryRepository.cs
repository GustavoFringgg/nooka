using Microsoft.EntityFrameworkCore;
using Nooka.Api.Data;
using Nooka.Api.Models;
using Nooka.Api.Models.Dtos.Response;

public class EfCategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public EfCategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories.ToListAsync();
    }
    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Category> CreateAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<Category?> UpdateAsync(int id, Category category)
    {
        var existingCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (existingCategory != null)
        {
            existingCategory.Name = category.Name;
            existingCategory.Color = category.Color;
            existingCategory.Description = category.Description;
            await _context.SaveChangesAsync();
        }
        else
        {
            return null;
        }
        return existingCategory;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existingCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (existingCategory != null)
        {
            _context.Categories.Remove(existingCategory);
            await _context.SaveChangesAsync();
            return true;
        }
        else
        { return false; }
    }

    public async Task<List<AdminCategoryResponse>> GetAllWithWordCountAsync()
    {
        var counts = await _context.WordCategories
            .GroupBy(wc => wc.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToListAsync();

        var categories = await _context.Categories.OrderBy(c => c.Id).ToListAsync();

        var countByCategory = counts.ToDictionary(c => c.CategoryId, c => c.Count);

        return categories.Select(c => new AdminCategoryResponse(
            c.Id,
            c.Name,
            c.Description,
            c.Color,
            c.UpdatedAt,
            countByCategory.GetValueOrDefault(c.Id, 0))).ToList();
    }
}