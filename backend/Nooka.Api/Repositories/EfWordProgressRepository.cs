using Microsoft.EntityFrameworkCore;
using Nooka.Api.Data;
using Nooka.Api.Models;
using Nooka.Api.Models.Dtos;

public class EfWordProgressRepository : IWordProgressRepository
{
    private readonly AppDbContext _context;
    public EfWordProgressRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<WordProgress>> GetByCategoryAsync(int userId, int categoryId)
    {
        var wordIds = _context.WordCategories.Where(wc => wc.CategoryId == categoryId).Select(wc => wc.WordId);
        return await _context.WordProgresses.Where(wp => wp.UserId == userId && wordIds.Contains(wp.WordId)).ToListAsync();
    }
    public async Task BatchUpsertAsync(int userId, List<WordProgressUpdate> updates)
    {
        foreach (var update in updates)
        {
            var existing = await _context.WordProgresses.FirstOrDefaultAsync(wp => wp.UserId == userId && wp.WordId == update.WordId);

            if (existing is null)
            {
                _context.WordProgresses.Add(new WordProgress
                {
                    UserId = userId,
                    WordId = update.WordId,
                    Level = update.Level,
                    IsArchived = update.IsArchived,
                    NextReviewAt = update.NextReviewAt
                });
            }
            else
            {
                existing.Level = update.Level;
                existing.IsArchived = update.IsArchived;
                existing.NextReviewAt = update.NextReviewAt;
                existing.UpdatedAt = DateTime.UtcNow;
            }

        }
        await _context.SaveChangesAsync();
    }
}