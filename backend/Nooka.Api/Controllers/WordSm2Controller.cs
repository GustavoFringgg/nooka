using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nooka.Api.Models.Dtos.Request;

[ApiController]
[Route("api/[controller]")]
public class WordSm2Controller : ControllerBase
{
    private readonly IWordSm2Repository _wordSm2Repository;
    public WordSm2Controller(IWordSm2Repository wordSm2Repository)
    {
        _wordSm2Repository = wordSm2Repository;
    }

    [Authorize]
    [HttpPost("answers")]
    public async Task<IActionResult> RecordAnswers(WordSm2AnswersRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _wordSm2Repository.RecordAnswersAsync(userId, request.Mode, request.Answers);
        return Ok(new { message = "record success" });
    }

    [Authorize]
    [HttpGet("words")]
    public async Task<IActionResult> GetWeightedRandomWords(string mode, int categoryId, int count)
    {
        if (mode != "Choice" && mode != "Typing")
        {
            return BadRequest(new { message = "mode 必須是 Choice 或 Typing" });
        }

        if (count <= 0)
        {
            return BadRequest(new { message = "count 必須大於 0" });
        }
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var words = await _wordSm2Repository.GetWeightedRandomWordsAsync(userId, mode, categoryId, count);
        return Ok(words);
    }

    [Authorize]
    [HttpGet("weakest")]
    public async Task<IActionResult> GetWeakestWords(string mode, int count)
    {
        if (mode != "Choice" && mode != "Typing")
        {
            return BadRequest(new { message = "mode 必須是 Choice 或 Typing" });
        }

        if (count <= 0)
        {
            return BadRequest(new { message = "count 必須大於 0" });
        }
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var words = await _wordSm2Repository.GetWeakestWordsAsync(userId, mode, count);
        return Ok(words);
    }

}