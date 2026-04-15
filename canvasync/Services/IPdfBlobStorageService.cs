namespace canvasync.Services;

public interface IPdfBlobStorageService
{
    Task<string> UploadLecturePdfAsync(
        string lectureId,
        string fileName,
        Stream fileStream,
        string? contentType,
        CancellationToken cancellationToken = default);

    Task<byte[]?> DownloadPdfAsync(string? pdfFileAddress, CancellationToken cancellationToken = default);

    Task DeletePdfAsync(string? pdfFileAddress, CancellationToken cancellationToken = default);
}