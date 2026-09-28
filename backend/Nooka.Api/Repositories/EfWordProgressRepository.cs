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
        var wordIds = updates.Select(u => u.WordId).ToList();

        var result = await _context.WordProgresses.Where(wp => wp.UserId == userId && wordIds.Contains(wp.WordId)).ToListAsync();

        var existingDict = result.ToDictionary(wp => wp.WordId);

        foreach (var update in updates)
        {
            if (existingDict.TryGetValue(update.WordId, out var existing))
            {
                existing.Level = update.Level;
                existing.IsArchived = update.IsArchived;
                existing.NextReviewAt = update.NextReviewAt;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
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

        }
        await _context.SaveChangesAsync();
    }

    public async Task<List<CategoryProgressSummary>> GetSummaryAsync(int userId)
    {
        var totalWordCount = await _context.WordCategories
        .GroupBy(wc => wc.CategoryId)
        .Select(g => new { CategoryId = g.Key, Total = g.Count() })
        .ToListAsync();

        var progressByCategory = await _context.WordProgresses
        .Where(wp => wp.UserId == userId)
        .Join(_context.WordCategories, wp => wp.WordId, wc => wc.WordId, (wp, wc) => new { wp, wc.CategoryId })
        .GroupBy(x => x.CategoryId).Select(g => new
        {
            CategoryId = g.Key,
            Familiar = g.Count(x => x.wp.IsArchived),
            Learning = g.Count(x => x.wp.Level != null && !x.wp.IsArchived),
            DueToday = g.Count(x => x.wp.Level != null && !x.wp.IsArchived && x.wp.NextReviewAt <= DateOnly.FromDateTime(DateTime.UtcNow))
        })
        .ToListAsync();

        var categories = await _context.Categories.ToListAsync();

        var result = totalWordCount.Select(t =>
        {
            var progress = progressByCategory.FirstOrDefault(p => p.CategoryId == t.CategoryId);
            var categoryName = categories.First(c => c.Id == t.CategoryId).Name;

            int familiar = progress?.Familiar ?? 0;
            int learning = progress?.Learning ?? 0;
            int dueToday = progress?.DueToday ?? 0;
            int newWords = t.Total - familiar - learning;

            return new CategoryProgressSummary(t.CategoryId, categoryName, familiar, learning, newWords, dueToday);
        }).ToList();
        return result;
    }
}