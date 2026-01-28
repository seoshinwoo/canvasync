
using System.ComponentModel.DataAnnotations.Schema;

namespace canvasync.Library.Models;

public class Lecture
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Code { get; set; } = Random.Shared.Next(0, 1000000).ToString("D6");
    public string FileName { get; set; } = string.Empty;
    public byte[]? PdfFileBytes { get; set; }
    [NotMapped]
    public List<Page> Pages { get; set; } = new();
}