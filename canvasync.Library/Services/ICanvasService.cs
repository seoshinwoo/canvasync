using canvasync.Library.Models;

namespace canvasync.Library.Services;

public interface ICanvasService
{
    Task AddLectureAsync(Lecture lecture, string memberId);
    Task JoinLectureAsync(string lectureId, string memberId);
    Task<Lecture?> GetLectureAsync(string lectureId);
    Task <Lecture?> GetLectureByCodeAsync(string code);
    Task<DrawingData?> GetDrawingDataAsync(string lectureId, string memberId); 
    Task<List<Lecture>> GetMyLecturesAsync(string memberId);
    Task<List<Lecture>> GetJoinedLecturesAsync(string memberId);
    Task SaveDrawingDataAsync(DrawingData drawingData);
    Task DeleteLectureAsync(string lectureId);
    Task LeaveLectureAsync(string lectureId, string memberId);
}