using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using canvasync.Library.Dtos;

namespace canvasync.Library.Models;

public class DrawingData
{
    [Key]
    public int Id { get; set; }
    
    public string LectureId { get; set; } = string.Empty;
    
    public string MemberId { get; set; } = string.Empty;
    
    public List<List<FactorDto>> Drawings { get; set; } = new();

    [JsonIgnore]
    public Lecture? Lecture { get; set; }

    [JsonIgnore]
    public Member? Member { get; set; }
}
