using Microsoft.EntityFrameworkCore;
using Nooka.Api.Data;
using Nooka.Api.Models;
using Nooka.Api.Models.Dtos.Response;

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

    public async Task<Word?> UpdateAsync(int id, Word word, List<int> categoryIds)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        var existingWord = await _context.Words.FirstOrDefaultAsync(w => w.Id == id);
        if (existingWord == null)
        {
            return null;
        }

        existingWord.Term = word.Term;
        existingWord.DefinitionCN = word.DefinitionCN;
        existingWord.DefinitionEN = word.DefinitionEN;
        existingWord.PartOfSpeech = word.PartOfSpeech;
        existingWord.Examples = word.Examples;
        existingWord.Ipa = word.Ipa;
        await _context.SaveChangesAsync();

        var existingWordCategories = await _context.WordCategories.Where(wc => wc.WordId == id).ToListAsync();
        var existingCategoryIds = existingWordCategories.Select(wc => wc.CategoryId).ToList();

        var toRemove = existingWordCategories.Where(wc => !categoryIds.Contains(wc.CategoryId)).ToList();
        var toAddIds = categoryIds.Where(cId => !existingCategoryIds.Contains(cId)).ToList();
        var toAdd = toAddIds.Select(cId => new WordCategory { WordId = id, CategoryId = cId }).ToList();

        _context.WordCategories.RemoveRange(toRemove);
        _context.WordCategories.AddRange(toAdd);
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();
        return existingWord;
    }


    public async Task<bool> DeleteAsync(int id)
    {
        var existingWord = await _context.Words.FirstOrDefaultAsync(w => w.Id == id);
        if (existingWord != null)
        {
            _context.Words.Remove(existingWord);
            await _context.SaveChangesAsync();
            return true;
        }
        else
        { return false; }
    }

    public async Task<PagedResponse<AdminWordResponse>> GetPagedAsync(int page, int pageSize, int? categoryId, string? keyword)
    {
        var query = _context.Words.AsQueryable();
        // _context.Words => DbSet<Word> 所以要轉成 IQueryable<Word> 使用 .AsQueryable()
        if (categoryId != null)
        {
            var wordIds = _context.WordCategories.Where(wc => wc.CategoryId == categoryId).Select(wc => wc.WordId);
            query = query.Where(w => wordIds.Contains(w.Id));
        }
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(w => EF.Functions.ILike(w.Term, $"%{keyword}%"));
        }
        var totalCount = await query.CountAsync();
        var words = await query
                .OrderBy(w => w.Term)
                .Skip((page - 1) * pageSize) // 先跳過前 ex 20 筆
                .Take(pageSize) // 再從那裡開始取 ex 20 筆
                .ToListAsync();

        var pageWordIds = words.Select(w => w.Id).ToList();
        var wordCategories = await _context.WordCategories
                .Where(wc => pageWordIds.Contains(wc.WordId))
                .ToListAsync();

        var categoryIdsByWord = wordCategories
                .GroupBy(wc => wc.WordId)
                .ToDictionary(g => g.Key, g => g.Select(wc => wc.CategoryId).ToList());

        var items = words.Select(w => new AdminWordResponse(
                w.Id,
                w.Term,
                w.DefinitionCN,
                w.DefinitionEN,
                w.PartOfSpeech,
                w.Examples,
                w.Ipa,
                categoryIdsByWord.GetValueOrDefault(w.Id) ?? new List<int>()
        )).ToList();

        return new PagedResponse<AdminWordResponse>(items, totalCount);
    }
}