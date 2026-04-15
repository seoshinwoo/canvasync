using System.Security.Claims;
using System.Text.Json;
using canvasync.Library.Models;
using canvasync.Library.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace canvasync.Controllers;

// [Authorize]: 인증된 사용자만 접근 가능. 쿠키가 없거나 만료되면 401 반환.
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class LectureController : ControllerBase
{
    private readonly ICanvasService _canvasService;
    public LectureController(ICanvasService canvasService)
    {
        _canvasService = canvasService;
    }

    // 쿠키의 Claim에서 memberId를 꺼냄. 클라이언트가 위변조 불가능.
    private string GetMemberId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException();

    [HttpPost("add-lecture")]
    public async Task<IActionResult> AddLectureAsync([FromBody] Lecture lecture)
    {
        if (lecture == null || string.IsNullOrWhiteSpace(lecture.PdfFileAddress))
        {
            return BadRequest("PdfFileAddress is required.");
        }

        await _canvasService.AddLectureAsync(lecture, GetMemberId());
        return Ok();
    }

    [HttpPost("join-lecture/{lectureId}")]
    public async Task<IActionResult> JoinLectureAsync(string lectureId)
    {
        await _canvasService.JoinLectureAsync(lectureId, GetMemberId());
        return Ok();
    }

    [HttpGet("my-lectures")]
    public async Task<IActionResult> GetMyLecturesAsync()
    {
        var lectures = await _canvasService.GetMyLecturesAsync(GetMemberId());
        return Ok(lectures);
    }

    [HttpGet("joined-lectures")]
    public async Task<IActionResult> GetJoinedLectures()
    {
        var lectures = await _canvasService.GetJoinedLecturesAsync(GetMemberId());
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
    public async Task<IActionResult> LeaveLecture(string lectureId)
    {
        await _canvasService.LeaveLectureAsync(lectureId, GetMemberId());
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