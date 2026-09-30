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
}