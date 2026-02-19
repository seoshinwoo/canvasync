using System.Text.Json;
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
    public async Task<IActionResult> GetMyLecturesAsync(
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

    [HttpGet("get-lecture/{lectureId}")]
    public async Task<IActionResult> GetLectureAsync(string lectureId)
    {
        var lecture = await _canvasService.GetLectureAsync(lectureId);

        return Ok(lecture);
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

    [HttpPost("set-status")]
    public async Task<IActionResult> SetLectureStatus([FromBody] JsonElement data)
    {
        try
        {
            string lectureId = data.GetProperty("LectureId").GetString();
            bool inProgress = data.GetProperty("InProgress").GetBoolean();

            return Ok(new { success = true });
        }
        catch (KeyNotFoundException)
        {
            return BadRequest("필수 데이터(LectureId 또는 InProgress)가 누락되었습니다.");
        }
    }
}