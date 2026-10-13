using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nooka.Api.Models.Dtos.Request;

[ApiController]
[Route("api/[controller]")]
public class ActivityController : ControllerBase
{
    private readonly IActivityRepository _activityRepository;
    public ActivityController(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }


    [Authorize]
    [HttpPost("round-complete")]
    public async Task<IActionResult> RecordRoundComplete(ActivityRoundCompleteRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _activityRepository.RecordRoundCompleteAsync(userId, request.Mode);
        return Ok(new { message = "record success" });
    }

    [Authorize]
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var summary = await _activityRepository.GetActivitySummaryAsync(userId);
        return Ok(summary);
    }

}