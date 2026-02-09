using canvasync.Library.Models;
using canvasync.Library.Services;

namespace canvasync.Services;

public class PdfService : IPdfService
{
    public async Task DownloadPdf(string lectureId, string memberId)
    {
        
    }

    public async Task<byte[]?> GetPdf(string lectureId)
    {
        return null;
    }

    public async Task SaveDrawingData(DrawingData drawingData)
    {
        
    }

    public async Task PdfToPages(byte[] pdfFile)
    {
        
    }

    public Task<(byte[] Bytes, string FileName)?> GetLecture(string lectureId)
    {
        throw new NotImplementedException();
    }
}