using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace canvasync.Library.Models;

public class Member
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;


    [InverseProperty(nameof(Lecture.HostMember))]
    public List<Lecture>? MyLectures { get; set; } = new();


    [InverseProperty(nameof(Lecture.Members))]
    public List<Lecture>? JoinedLectures { get; set; } = new();
}