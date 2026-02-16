using canvasync.Library.Models;
using canvasync.Library.Services;
using Microsoft.AspNetCore.Mvc;

namespace canvasync.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LectureController : ControllerBase
{
    private readonly ICanvasService _canvasService;
    public LectureController(ICanvasService canvasService)
    {
        _canvasService = canvasService;
    }

    [HttpPost("add-lecture")]
    public async Task<IActionResult> AddLectureAsync(
        [FromBody] Lecture lecture, [FromHeader(Name = "X-Member-Id")] string memberId)
    {
        await _canvasService.AddLectureAsync(lecture, memberId);
        return Ok();
    }

    [HttpPost("join-lecture/{lectureId}")]
    public async Task<IActionResult> JoinLectureAsync(
        string lectureId, [FromHeader(Name = "X-Member-Id")] string memberId)
    {
        await _canvasService.JoinLectureAsync(lectureId, memberId);
        return Ok();
    }

    [HttpGet("my-lectures")]
    public async Task<IActionResult> GetMyLectureAsync(
        [FromHeader(Name = "X-Member-Id")] string memberId
    )
    {
        var lectures = await _canvasService.GetMyLecturesAsync(memberId);
        return Ok(lectures);
    }

    [HttpGet("joined-lectures")]
    public async Task<IActionResult> GetJoinedLectures(
        [FromHeader(Name = "X-Member-Id")] string memberId)
    {
        var lectures = await _canvasService.GetJoinedLecturesAsync(memberId);
        return Ok(lectures);
    }

    [HttpPost("save-drawingdata")]
    public async Task<IActionResult> SaveDrawingData([FromBody] DrawingData drawingData)
    {
        await _canvasService.SaveDrawingDataAsync(drawingData);
        return Ok();
    }

    [HttpDelete("{lectureId}")]
    public async Task<IActionResult> DeleteLecture(string lectureId)
    {
        await _canvasService.DeleteLectureAsync(lectureId);
        return Ok();
    }

    [HttpPost("leave-lecture/{lectureId}")]
    public async Task<IActionResult> LeaveLecture(string lectureId, [FromHeader(Name = "X-Member-Id")] string memberId)
    {
        await _canvasService.LeaveLectureAsync(lectureId, memberId);
        return Ok();
    }
}