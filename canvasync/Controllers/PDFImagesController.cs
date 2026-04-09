using System.Runtime.InteropServices.Marshalling;
using System.Text.Json;
using canvasync.Containers;
using canvasync.Library.Dtos;
using canvasync.Library.Models;
using canvasync.Library.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.JSInterop;

namespace canvasync.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PDFImagesController : ControllerBase
{
    private readonly StateContainer _stateContainer;
    public PDFImagesController(StateContainer stateContainer)
    {
        _stateContainer = stateContainer;
    }

    [HttpGet("{lectureId}")]
    public IActionResult Get(string lectureId)
        => Ok(_stateContainer.Lectures.Where(lec => lec.Id == lectureId).Select(lec => PageDto.PagesToPageDtos(lec.Pages)).FirstOrDefault());
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PagesController : ControllerBase
{
    private readonly StateContainer _stateContainer;
    public PagesController(StateContainer stateContainer)
    {
        _stateContainer = stateContainer;
    }

    [HttpGet("{lectureId}")]
    public IActionResult Get(string lectureId)
        => Ok(_stateContainer.Lectures.Where(lec => lec.Id == lectureId).Select(lec => PageDto.PagesToPageDtos(lec.Pages)).FirstOrDefault());
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PDFDownloadController : ControllerBase
{
    private readonly ICanvasService _canvasService;
    private readonly StateContainer _stateContainer;
    public PDFDownloadController(StateContainer stateContainer, ICanvasService canvasService)
    {
        _stateContainer = stateContainer;
        _canvasService = canvasService;
    }

    [HttpPost("make-pdf/{lectureId}/{memberId}")]
    public async Task<IActionResult> MakePDF(string lectureId, string memberId)
    {
        var maxFileSize = 500 * 1024 * 1024;
        var form = await Request.ReadFormAsync();
        var lecture = await _canvasService.GetLectureAsync(lectureId);
        var member = await _canvasService.GetMemberAsync(memberId);

        var pdfFile = lecture.PdfFileBytes;
        if (pdfFile == null || pdfFile.Count() == 0)
        {
            return BadRequest("PDF file not found.");
        }

        string? drawingDatas = form["drawings"];

        if (string.IsNullOrEmpty(drawingDatas))
        {
            return BadRequest("Drawing data not found.");
        }

        var drawingsDto = JsonSerializer.Deserialize<DrawingsDto>(drawingDatas);
        if (drawingsDto == null)
        {
            return BadRequest("Invalid drawing data");
        }

        // 오버레이할 PDF 생성
        byte[] overlayPdf = _stateContainer.CreateOverlayPdf(drawingsDto);

        if (lecture.PdfFileBytes is not null)
        {
            var result = _stateContainer.MergePdfs(lecture.PdfFileBytes, overlayPdf);
            var fileName = $"{System.IO.Path.GetFileNameWithoutExtension(lecture.FileName)}_{member.Name}.pdf";

            return File(result, "application/pdf", fileName);
        }
        else
        {
            return BadRequest("Invalid drawing data");
        }
    }
}
