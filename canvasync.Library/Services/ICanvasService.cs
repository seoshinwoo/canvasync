using canvasync.Library.Models;

namespace canvasync.Library.Services;

public interface ICanvasService
{
    Task AddLectureAsync(Lecture lecture, string memberId);
    Task JoinLectureAsync(string lectureId, string memberId);
    Task<List<Lecture>> GetMyLecturesAsync(string memberId);
    Task<List<Lecture>> GetJoinedLecturesAsync(string memberId);

}