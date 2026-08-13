public interface IWordRepository
{
    Task<IEnumerable<Word>> GetAllAsync();
    Task<IEnumerable<Word>> GetByCategoryIdAsync(int categoryId);
    // (沒加 ?)是因為「查全部」這個操作保證會回傳一個 list,就算沒資料也是回傳空的 list([]),而不是 null,所以不需要標 ?
    Task<Word?> GetByIdAsync(int id);
    // 加? 是代表有可能會找無 所以告訴系統可能會 null
}
