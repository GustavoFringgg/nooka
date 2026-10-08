using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Nooka.Api.Models;
using Nooka.Api.Models.Dtos.Request;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IWordRepository _wordRepository;
    public AdminController(ICategoryRepository categoryRepository, IWordRepository wordRepository)
    {
        _categoryRepository = categoryRepository;
        _wordRepository = wordRepository;
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _categoryRepository.GetAllWithWordCountAsync();
        return Ok(result);
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
            return BadRequest("名稱不能空白");
        try
        {
            var newCategory = await _categoryRepository.CreateAsync(category);
            return Ok(newCategory);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException { SqlState: "23505" })
            {
                return Conflict("此書籍已存在");
            }
            throw;
        }
    }


    [HttpPut("categories/{id}")]
    public async Task<IActionResult> UpdateCategory(int id, Category category)
    {
        if (id != category.Id)
        {
            return BadRequest("網址與資料的 id 不同");
        }
        if (string.IsNullOrWhiteSpace(category.Name))
            return BadRequest("名稱不能空白");
        try
        {
            var newCategory = await _categoryRepository.UpdateAsync(id, category);
            if (newCategory != null)
            {
                return Ok(newCategory);
            }
            else
            {
                return NotFound();
            }
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException { SqlState: "23505" })
            {
                return Conflict("此書籍已存在");
            }
            throw;
        }
    }


    [HttpDelete("categories/{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var isDelete = await _categoryRepository.DeleteAsync(id);
        if (isDelete)
        {
            return NoContent();
        }
        else
        {
            return NotFound();
        }
    }

    [HttpPost("words")]
    public async Task<IActionResult> CreateWord(WordUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Word.Term))
            return BadRequest("名稱不能空白");
        try
        {
            var newWord = await _wordRepository.CreateAsync(request.Word, request.CategoryIds);
            return Ok(newWord);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException { SqlState: "23505" })
            {
                return Conflict("此單字已存在");
            }
            throw;
        }
    }

    [HttpPut("words/{id}")]
    public async Task<IActionResult> UpdateWord(int id, WordUpsertRequest request)
    {
        if (id != request.Word.Id)
        {
            return BadRequest("網址與資料的 id 不同");
        }
        if (string.IsNullOrWhiteSpace(request.Word.Term))
            return BadRequest("名稱不能空白");
        try
        {
            var newWord = await _wordRepository.UpdateAsync(id, request.Word, request.CategoryIds);
            if (newWord != null)
            {
                return Ok(newWord);
            }
            else
            {
                return NotFound();
            }
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException { SqlState: "23505" })
            {
                return Conflict("此單字已存在");
            }
            throw;
        }
    }

    [HttpDelete("words/{id}")]
    public async Task<IActionResult> DeleteWord(int id)
    {
        var isDelete = await _wordRepository.DeleteAsync(id);
        if (isDelete)
        { return NoContent(); }
        else
        { return NotFound(); }
    }

    [HttpGet("words")]
    public async Task<IActionResult> GetWords(int page = 1, int pageSize = 20, int? categoryId = null, string? keyword = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var result = await _wordRepository.GetPagedAsync(page, pageSize, categoryId, keyword);
        return Ok(result);
    }
}