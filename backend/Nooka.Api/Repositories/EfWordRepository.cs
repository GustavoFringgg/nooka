using Microsoft.EntityFrameworkCore;
using Nooka.Api.Data;
using Nooka.Api.Models;
public class EfWordRepository : IWordRepository
{

    private readonly AppDbContext _context;

    public EfWordRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Word>> GetAllAsync()
    {
        // TODO: ToListAsync 的意思
        return await _context.Words.ToListAsync();
    }

    public async Task<IEnumerable<Word>> GetByCategoryIdAsync(int categoryId)
    {
        var wordIds = _context.WordCategories.Where(wc => wc.CategoryId == categoryId).Select(wc => wc.WordId);
        return await _context.Words.Where(w => wordIds.Contains(w.Id)).ToListAsync();
    }

    public async Task<Word?> GetByIdAsync(int id)
    {
        // TODO: 這裡要理解為啥用 async
        return await _context.Words.FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<Word> CreateAsync(Word word, List<int> categoryIds)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        _context.Words.Add(word);
        await _context.SaveChangesAsync();

        var wordCategories = categoryIds.Select(cId => new WordCategory { WordId = word.Id, CategoryId = cId }).ToList();

        _context.WordCategories.AddRange(wordCategories);
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();
        return word;
    }
}