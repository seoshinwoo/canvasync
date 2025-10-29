
namespace canvasync.Library.Models;

public class Lecture
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FileName { get; set; } = string.Empty;
    public byte[]? PdfFileBytes { get; set; }
    public List<Page> Pages { get; set; } = new();
}