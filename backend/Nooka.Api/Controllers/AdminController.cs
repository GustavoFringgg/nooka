using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nooka.Api.Models;


[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly ICategoryRepository _repository;
    public AdminController(ICategoryRepository repository)
    {
        _repository = repository;
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
            return BadRequest("名稱不能空白");
        var newCategory = await _repository.CreateAsync(category);
        return Ok(newCategory);
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
        var newCategory = await _repository.UpdateAsync(id, category);
        if (newCategory != null)
        {
            return Ok(newCategory);
        }
        else
        {
            return NotFound();
        }
    }


    [HttpDelete("categories/{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var isDelete = await _repository.DeleteAsync(id);
        if (isDelete)
        {
            return NoContent();
        }
        else
        {
            return NotFound();
        }
    }
}