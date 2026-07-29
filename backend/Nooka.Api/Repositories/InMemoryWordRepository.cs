using System.Text.Json;

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
        // 回傳的東西是 Task<IEnumerable<Word>> —— 也就是一張「號碼牌」,上面寫著「之後會給你一個可迴圈的 Word 集合」 await 之後才會拿到
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