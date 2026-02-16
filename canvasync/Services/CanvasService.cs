using canvasync.Data;
using canvasync.Library.Models;
using canvasync.Library.Services;
using Microsoft.EntityFrameworkCore;

namespace canvasync.Services;

public class CanvasDataService : ICanvasService
{
    private readonly IDbContextFactory<CanvasDbContext> _factory;
    public CanvasDataService(IDbContextFactory<CanvasDbContext> factory)
    {
        _factory = factory;
    }

    // Lecture 추가
    public async Task AddLectureAsync(Lecture lecture, string memberId)
    {
        using var context = await _factory.CreateDbContextAsync();
        var member = await context.Members
            .Include(m => m.MyLectures)
            .FirstOrDefaultAsync(m => m.Id == memberId);

        if (member == null) return;

        if (lecture == null) return;
        
        member.MyLectures?.Add(lecture);

        await context.SaveChangesAsync();
    }

    public async Task<List<Lecture>> GetMyLecturesAsync(string memberId)
    {
        using var context = await _factory.CreateDbContextAsync();
        var member = await context.Members
            .Include(m => m.MyLectures)
            .FirstOrDefaultAsync(m => m.Id == memberId);

        if (member == null)
        {
            return new List<Lecture>();
        }

        return member.MyLectures;
    }

    public async Task<List<Lecture>> GetJoinedLecturesAsync(string memberId)
    {
        using var context = await _factory.CreateDbContextAsync();
        var member = await context.Members
            .Include(m => m.JoinedLectures)
            .FirstOrDefaultAsync(m => m.Id == memberId);

        if (member == null)
        {
            return new List<Lecture>();
        }

        return member.JoinedLectures;
    }

    public async Task JoinLectureAsync(string lectureId, string memberId)
    {
        using var context = await _factory.CreateDbContextAsync();
        var member = await context.Members
            .Include(m => m.JoinedLectures)
            .FirstOrDefaultAsync(m => m.Id == memberId);

        if (member == null) return;

        var targetLecture = await context.Lectures.FindAsync(lectureId);
        if (targetLecture == null)
        {
             return;
        }

        if (!member.JoinedLectures.Any(l => l.Id == targetLecture.Id))
        {
            member.JoinedLectures.Add(targetLecture);
            }

        await context.SaveChangesAsync();
    }

    public async Task SaveDrawingDataAsync(DrawingData drawingData)
    {
        using var context = await _factory.CreateDbContextAsync();
        context.DrawingData.Add(drawingData);

        await context.SaveChangesAsync();
    }

    public async Task DeleteLectureAsync(string lectureId)
    {
        using var context = await _factory.CreateDbContextAsync();
        var lecture = await context.Lectures.FindAsync(lectureId);
        if (lecture != null)
        {
            context.Lectures.Remove(lecture);
            await context.SaveChangesAsync();
        }
    }

    public async Task LeaveLectureAsync(string lectureId, string memberId)
    {
        using var context = await _factory.CreateDbContextAsync();
        var member = await context.Members
            .Include(m => m.JoinedLectures)
            .FirstOrDefaultAsync(m => m.Id == memberId);

        if (member != null)
        {
            var lecture = member.JoinedLectures.FirstOrDefault(l => l.Id == lectureId);
            if (lecture != null)
            {
                member.JoinedLectures.Remove(lecture);
                await context.SaveChangesAsync();
            }
        }
    }

    public Task<Lecture?> GetLectureAsync(string lectureId)
    {
        throw new NotImplementedException();
    }
}