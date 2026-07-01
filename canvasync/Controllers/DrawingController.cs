using canvasync.Library.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace canvasync.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DrawingDataController : ControllerBase
{
    private readonly ICanvasService _canvasSerivce;

    public DrawingDataController(ICanvasService canvasService)
    {
        _canvasSerivce = canvasService;
    }

    private string GetMemberId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException();

    [HttpGet("get-drawingdata/{lectureId}/{memberId}")]
    public async Task<IActionResult> GetDrawingDataAsync(string lectureId, string memberId)
    {
        if (!await _canvasSerivce.CanReadDrawingDataAsync(lectureId, memberId, GetMemberId()))
        {
            return Forbid();
        }

        var drawingData = await _canvasSerivce.GetDrawingDataAsync(lectureId, memberId);
        if (drawingData is null)
        {
            return NotFound();
        }

        return Ok(drawingData);
    }
}
