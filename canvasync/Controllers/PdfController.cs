
using canvasync.Data;
using canvasync.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace canvasync.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PdfController : ControllerBase
{
    private readonly IDbContextFactory<CanvasDbContext> _contextFactory;
    private readonly IPdfBlobStorageService _pdfBlobStorageService;

    public PdfController(
        IDbContextFactory<CanvasDbContext> contextFactory,
        IPdfBlobStorageService pdfBlobStorageService)
    {
        _contextFactory = contextFactory;
        _pdfBlobStorageService = pdfBlobStorageService;
    }

    [HttpGet("get-filebytes/{lectureId}")]
    public async Task<IActionResult> GetPdf(string lectureId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var lecture = await context.Lectures
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == lectureId);

        if (lecture == null || string.IsNullOrWhiteSpace(lecture.PdfFileAddress))
        {
            return NotFound();
        }

        var bytes = await _pdfBlobStorageService.DownloadPdfAsync(lecture.PdfFileAddress);
        if (bytes == null || bytes.Length == 0)
        {
            return NotFound();
        }

        return File(bytes, "application/pdf", lecture.FileName ?? "document.pdf");
    }

    [HttpPost("save-drawingdata")]
    public async Task<IActionResult> SaveDrawingData()
    {
        

        return Ok();
    }
}