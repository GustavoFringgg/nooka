using System.Text.Json;
using Nooka.Api.Models;
public class InMemoryWordRepository : IWordRepository
{
    private readonly List<Word> _words;

    public InMemoryWordRepository()
    {
        var json = File.ReadAllText("Data/words.json");
        _words = JsonSerializer.Deserialize<List<Word>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<Word>();
    }

    public Task<IEnumerable<Word>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Word>>(_words);
    }

    public Task<IEnumerable<Word>> GetByCategoryIdAsync(int categoryId)
    {
        var result = _words.Where(w => w.CategoryId == categoryId);
        return Task.FromResult(result);
    }

    public Task<Word?> GetByIdAsync(int id)
    {
        var word = _words.FirstOrDefault(w => w.Id == id);
        return Task.FromResult(word);
    }


}