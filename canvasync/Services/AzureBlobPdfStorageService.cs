using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace canvasync.Services;

public class AzureBlobPdfStorageService : IPdfBlobStorageService
{
    private const string DefaultContainerName = "lecture-pdfs";

    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public AzureBlobPdfStorageService(BlobServiceClient blobServiceClient, IConfiguration configuration)
    {
        _blobServiceClient = blobServiceClient;
        _containerName = configuration["AzureStorage:PdfContainerName"] ?? DefaultContainerName;
    }

    public async Task<string> UploadLecturePdfAsync(
        string lectureId,
        string fileName,
        Stream fileStream,
        string? contentType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(lectureId))
        {
            throw new ArgumentException("Lecture id is required.", nameof(lectureId));
        }

        if (fileStream is null)
        {
            throw new ArgumentNullException(nameof(fileStream));
        }

        var safeFileName = string.IsNullOrWhiteSpace(fileName)
            ? "document.pdf"
            : Path.GetFileName(fileName);

        var blobName = $"{lectureId}/{Guid.NewGuid():N}_{safeFileName}";
        var container = _blobServiceClient.GetBlobContainerClient(_containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

        var blobClient = container.GetBlobClient(blobName);

        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/pdf" : contentType
            }
        };

        await blobClient.UploadAsync(fileStream, uploadOptions, cancellationToken);

        return blobClient.Uri.ToString();
    }

    public async Task<byte[]?> DownloadPdfAsync(string? pdfFileAddress, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pdfFileAddress))
        {
            return null;
        }

        if (!Uri.TryCreate(pdfFileAddress, UriKind.Absolute, out var pdfUri))
        {
            return null;
        }

        var blobClient = GetBlobClient(pdfUri);
        var exists = await blobClient.ExistsAsync(cancellationToken);
        if (!exists.Value)
        {
            return null;
        }

        var content = await blobClient.DownloadContentAsync(cancellationToken);
        return content.Value.Content.ToArray();
    }

    public async Task DeletePdfAsync(string? pdfFileAddress, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pdfFileAddress))
        {
            return;
        }

        if (!Uri.TryCreate(pdfFileAddress, UriKind.Absolute, out var pdfUri))
        {
            return;
        }

        var blobClient = GetBlobClient(pdfUri);
        await blobClient.DeleteIfExistsAsync(
            DeleteSnapshotsOption.IncludeSnapshots,
            cancellationToken: cancellationToken);
    }

    private BlobClient GetBlobClient(Uri blobAddress)
    {
        var blobUriBuilder = new BlobUriBuilder(blobAddress);

        if (string.IsNullOrWhiteSpace(blobUriBuilder.BlobContainerName) || string.IsNullOrWhiteSpace(blobUriBuilder.BlobName))
        {
            throw new InvalidOperationException("The blob address is not valid.");
        }

        return _blobServiceClient
            .GetBlobContainerClient(blobUriBuilder.BlobContainerName)
            .GetBlobClient(blobUriBuilder.BlobName);
    }
}