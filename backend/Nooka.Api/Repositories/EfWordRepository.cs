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
        var wordIds = _context.WordCategories.Where(wc=>wc.CategoryId == categoryId).Select(wc => wc.WordId);
        return await _context.Words.Where(w => wordIds.Contains(w.Id)).ToListAsync();
    }

    public async Task<Word?> GetByIdAsync(int id)
    {
        // TODO: 這裡要理解為啥用 async
        return await _context.Words.FirstOrDefaultAsync(w => w.Id == id);
    }


}