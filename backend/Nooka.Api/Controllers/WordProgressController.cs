using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nooka.Api.Models;
using Nooka.Api.Models.Dtos.Request;

[ApiController]
[Route("api/[controller]")]
public class ProgressController : ControllerBase
{
    private readonly IWordProgressRepository _wordProgressRepository;

    public ProgressController(IWordProgressRepository wordProgressRepository)
    {
        _wordProgressRepository = wordProgressRepository;
    }

    [Authorize]
    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetProgressByCategory(int categoryId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var words = await _wordProgressRepository.GetByCategoryAsync(userId, categoryId);
        return Ok(words);
    }

    [Authorize]
    [HttpPost("batch")]
    public async Task<IActionResult> UpdateWordProgress(List<WordProgressUpdateRequest> updates)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _wordProgressRepository.BatchUpsertAsync(userId, updates);
        return Ok(new { message = "update success" });
    }

    [Authorize]
    [HttpGet("summary")]
    public async Task<IActionResult> GetProgressSummary()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var summary = await _wordProgressRepository.GetSummaryAsync(userId);
        return Ok(summary);
    }

}