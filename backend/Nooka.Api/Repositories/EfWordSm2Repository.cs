using Microsoft.EntityFrameworkCore;
using Nooka.Api.Data;
using Nooka.Api.Models;
using Nooka.Api.Models.Dtos.Request;
using Nooka.Api.Models.Dtos.Response;
using Nooka.Api.Services;


public class EfWordSm2Repository : IWordSm2Repository
{
    private const float InitialEF = 2.5f;
    private readonly AppDbContext _context;
    public EfWordSm2Repository(AppDbContext context)
    {
        _context = context;
    }

    // TODO:10/20 專案複習
    public async Task RecordAnswersAsync(int userId, string mode, List<WordAnswerRequest> answers)
    {
        var wordIdList = answers.Select(a => a.WordId).ToList();

        var existingRecords = await _context.WordSm2Progresses
            .Where(wsp => wsp.UserId == userId && wsp.Mode == mode && wordIdList.Contains(wsp.WordId))
            .ToDictionaryAsync(wsp => wsp.WordId);

        foreach (var answer in answers)
        {
            if (existingRecords.TryGetValue(answer.WordId, out var existing))
            {
                existing.EF = Sm2EfCalculator.ApplyAnswer(existing.EF, answer.IsCorrect);
            }
            else
            {
                _context.WordSm2Progresses.Add(new WordSm2Progress
                {
                    UserId = userId,
                    WordId = answer.WordId,
                    Mode = mode,
                    EF = Sm2EfCalculator.ApplyAnswer(InitialEF, answer.IsCorrect)
                });
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<Word>> GetWeightedRandomWordsAsync(int userId, string mode, int categoryId, int count)
    {
        var wordIds = _context.WordCategories.Where(wc => wc.CategoryId == categoryId).Select(wc => wc.WordId);
        var words = await _context.Words.Where(w => wordIds.Contains(w.Id)).ToListAsync();

        var wordIdList = words.Select(w => w.Id).ToList();

        var existingRecords = await _context.WordSm2Progresses
            .Where(wsp => wsp.UserId == userId && wsp.Mode == mode && wordIdList.Contains(wsp.WordId))
            .ToDictionaryAsync(wsp => wsp.WordId);

        var weightedWords = words.Select(w =>
        {
            var ef = existingRecords.TryGetValue(w.Id, out var record) ? record.EF : InitialEF;
            return (Word: w, Weight: 1f / ef);
        }).ToList();

        var random = new Random();
        var pool = new List<(Word Word, float Weight)>(weightedWords);
        var selected = new List<Word>();

        var actualCount = Math.Min(count, pool.Count);

        for (int i = 0; i < actualCount; i++)
        {
            var totalWeight = pool.Sum(p => p.Weight);
            var roll = random.NextDouble() * totalWeight;

            float cumulative = 0;
            var pickedIndex = 0;
            for (int j = 0; j < pool.Count; j++)
            {
                cumulative += pool[j].Weight;
                if (roll <= cumulative)
                {
                    pickedIndex = j;
                    break;
                }
            }
            selected.Add(pool[pickedIndex].Word);
            pool.RemoveAt(pickedIndex);
        }
        return selected;
    }

    public async Task<List<WeakestWordResponse>> GetWeakestWordsAsync(int userId, string mode, int count)
    {
        return await _context.WordSm2Progresses
            .Where(wsp => wsp.UserId == userId && wsp.Mode == mode && wsp.EF < InitialEF)
            .Join(_context.Words, wsp => wsp.WordId, w => w.Id, (wsp, w) => new { Word = w, EF = wsp.EF })
            .OrderBy(x => x.EF)
            .ThenBy(x => x.Word.Term)
            .Take(count)
            .Select(x => new WeakestWordResponse
            {
                WordId = x.Word.Id,
                Term = x.Word.Term,
                DefinitionCN = x.Word.DefinitionCN,
                EF = x.EF
            }).ToListAsync();
    }
}