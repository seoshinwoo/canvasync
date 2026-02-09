using canvasync.Library.Models;

namespace canvasync.Library.Services;

public interface IPdfService
{
    Task<(byte[] Bytes, string FileName)?> GetLecture(string lectureId);
    Task SaveDrawingData(DrawingData drawingData);
    Task DownloadPdf(string lectureId, string memberId);
}