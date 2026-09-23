using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nooka.Api.Models;
using Nooka.Api.Models.Dtos;

[ApiController]
[Route("api/[controller]")]
public class ProgressController : ControllerBase
{
    private readonly IWordProgressRepository _repository;

    public ProgressController(IWordProgressRepository repository)
    {
        _repository = repository;
    }

    [Authorize]
    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetProgressByCategory(int categoryId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var words = await _repository.GetByCategoryAsync(userId, categoryId);
        return Ok(words);
    }

    [Authorize]
    [HttpPost("batch")]
    public async Task<IActionResult> UpdateWordProgress(List<WordProgressUpdate> updates)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _repository.BatchUpsertAsync(userId, updates);
        return Ok(new { message = "update success" });
    }

    [Authorize]
    [HttpGet("summary")]
    public async Task<IActionResult> GetProgressSummary()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var summary = await _repository.GetSummaryAsync(userId);
        return Ok(summary);
    }

}