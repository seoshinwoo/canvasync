
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace canvasync.Library.Models;

public class Lecture
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Code { get; set; } = Random.Shared.Next(0, 1000000).ToString("D6");
    public string FileName { get; set; } = string.Empty;
    public string? PdfFileAddress { get; set; }


    [InverseProperty(nameof(Member.MyLectures))]
    public Member HostMember { get; set; } = null!;


    [InverseProperty(nameof(Member.JoinedLectures))]
    public List<Member>? Members { get; set; }

    [JsonIgnore]
    [NotMapped]
    public List<Page> Pages { get; set; } = new();
}