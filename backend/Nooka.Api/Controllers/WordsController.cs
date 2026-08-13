using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class WordsController : ControllerBase
{
    private readonly IWordRepository _repository;


    public WordsController(IWordRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var words = await _repository.GetAllAsync();
        return Ok(words);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var word = await _repository.GetByIdAsync(id);
        if (word is null)
        {
            return NotFound();
        }
        return Ok(word);
    }

    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetByCategory(int categoryId)
    {
        var words = await _repository.GetByCategoryIdAsync(categoryId);
        return Ok(words);
    }
}