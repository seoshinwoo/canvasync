using canvasync.Library.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [HttpGet("get-drawingdata/{lectureId}/{memberId}")]
    public async Task<IActionResult> GetDrawingDataAsync(string lectureId, string memberId)
    {
        var drawingData = await _canvasSerivce.GetDrawingDataAsync(lectureId, memberId);
        return Ok(drawingData);
    }
}