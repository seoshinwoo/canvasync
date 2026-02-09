using System.Net.Http.Json;
using canvasync.Library.Models;
using canvasync.Library.Services;
using SkiaSharp;

namespace canvasync.Client.Services;

public class PdfClientService : IPdfService
{
    private readonly HttpClient _httpClient;

    public PdfClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public Task DownloadPdf(string lectureId, string memberId)
    {
        throw new NotImplementedException();
    }

    public async Task<(byte[] Bytes, string FileName)?> GetLecture(string lectureId)
    {
        var response = await _httpClient.GetAsync($"api/pdf/get-filebytes/{lectureId}");
        if (response.IsSuccessStatusCode)
        {
            // Body에서 바이트 데이터 가져오기
            var bytes = await response.Content.ReadAsByteArrayAsync();

            // Header에서 파일 이름 꺼내기
            var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
                            ?? response.Content.Headers.ContentDisposition?.FileName
                            ?? "unknown.pdf";

            return (bytes, fileName);
        }
        return null;
    }

    public async Task SaveDrawingData(DrawingData drawingData)
    {
        var response = await _httpClient.PostAsJsonAsync("api/pdf/save-drawingdata", drawingData);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<string>> PdfToImages(byte[] pdfFile)
    {
        var imageUrls = new List<string>();

        using var stream = new MemoryStream(pdfFile);
        var images = PDFtoImage.Conversion.ToImagesAsync(stream);

        await foreach (var image in images)
        {
            using var data = image.Encode(SKEncodedImageFormat.Png, 80);
            var base64 = Convert.ToBase64String(data.ToArray());
            imageUrls.Add($"data:image/png;base64,{base64}");
        }

        return imageUrls;
    }
}