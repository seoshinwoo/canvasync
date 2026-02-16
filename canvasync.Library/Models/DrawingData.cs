using System.ComponentModel.DataAnnotations;
using canvasync.Library.Dtos;

namespace canvasync.Library.Models;

public class DrawingData
{
    [Key]
    public int Id { get; set; }
    
    public string LectureId { get; set; }
    
    public string MemberId { get; set; }
    
    public List<PageDto> Drawings { get; set; } = new();
}
