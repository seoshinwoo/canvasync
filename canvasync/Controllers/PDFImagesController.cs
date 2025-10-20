using System.Text.Json;
using canvasync.Containers;
using canvasync.Library.Dtos;
using canvasync.Library.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.JSInterop;

namespace canvasync.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PDFImagesController : ControllerBase
{
    private readonly StateContainer _stateContainer;
    public PDFImagesController(StateContainer stateContainer)
    {
        _stateContainer = stateContainer;
    }

    [HttpGet]
    public IActionResult Get() => Ok(_stateContainer.imageUrls);
}

[ApiController]
[Route("api/[controller]")]
public class PDFDownloadController : ControllerBase
{
    private readonly StateContainer _stateContainer;
    public PDFDownloadController(StateContainer stateContainer)
    {
        _stateContainer = stateContainer;
    }

    [HttpPost("make-pdf")]
    public async Task<IActionResult> MakePDF()
    {
        Console.WriteLine($"_stateContainer.PdfFileBytes.Length : {_stateContainer.PdfFileBytes.Length}");
        var maxFileSize = 500 * 1024 * 1024;
        var form = await Request.ReadFormAsync();

        var pdfFile = _stateContainer.PdfFileBytes;
        if (pdfFile == null || pdfFile.Count() == 0)
        {
            return BadRequest("PDF file not found.");
        }

        string? drawingDatas = form["drawings"];
        Console.WriteLine($"drawingDatas.Length : {drawingDatas.Length}");
        if (string.IsNullOrEmpty(drawingDatas))
        {
            return BadRequest("Drawing data not found.");
        }

        var drawingsDto = JsonSerializer.Deserialize<DrawingsDto>(drawingDatas);
        if (drawingsDto == null)
        {
            return BadRequest("Invalid drawing data");
        }

        // PDF 처리 서비스 호출
        // byte[] resultPdf = _stateContainer.Create(factorDatas);

        // 오버레이할 PDF 생성
        byte[] overlayPdf = _stateContainer.CreateOverlayPdf(drawingsDto);
        Console.WriteLine($"overlayPdf : {overlayPdf.Length}");

        if (_stateContainer.PdfFileBytes is not null)
        {
            Console.WriteLine($"MergePdfs에 들어갈 PdfFileBytes 의 개수 : {_stateContainer.PdfFileBytes.Length}");
            var result = _stateContainer.MergePdfs(_stateContainer.PdfFileBytes, overlayPdf);
            return File(result, "application/pdf", "downloadedFileName.pdf");
        }
        else
        {
            return BadRequest("Invalid drawing data");
        }


        // // 기존 PDF와 오브레이할 PDF 합치기
        // using (var memoryStream = new MemoryStream())
        // {
        //     try
        //     {
        //         await _stateContainer.PdfFile.OpenReadStream(maxFileSize).CopyToAsync(memoryStream);
        //     }
        //     catch (JSDisconnectedException)
        //     {
        //         Console.WriteLine("File upload was canceld by the user.");
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"{ex}, An unexpected error occured during file upload");
        //     }

        //     byte[] basePdfBytes = memoryStream.ToArray();

        //     var result = _stateContainer.MergePdfs(basePdfBytes, overlayPdf);

        //     return File(result, "application/pdf", "downloadedFileName.pdf");
        //     }
        // }
    }
}
