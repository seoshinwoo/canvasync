

using canvasync.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace canvasync.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PdfController : ControllerBase
{
    private readonly IDbContextFactory<CanvasDbContext> _contextFactory;

    public PdfController(IDbContextFactory<CanvasDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    [HttpGet("get-filebytes/{lectureId}")]
    public async Task<IActionResult> GetPdf(string lectureId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var lecture = await context.Lectures
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == lectureId);

        if (lecture == null || lecture.PdfFileBytes == null)
        {
            return NotFound();
        }

        return File(lecture.PdfFileBytes, "application/pdf", lecture.FileName ?? "document.pdf");
    }

    [HttpPost("save-drawingdata")]
    public async Task<IActionResult> SaveDrawingData()
    {
        

        return Ok();
    }
}