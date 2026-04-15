using canvasync.Data;
using canvasync.Library.Models;
using canvasync.Library.Services;
using Microsoft.EntityFrameworkCore;
using canvasync.Containers;

namespace canvasync.Services;

public class CanvasService : ICanvasService
{
    private readonly IDbContextFactory<CanvasDbContext> _factory;
    private readonly IDrawingStorageService _drawingStorage;
    private readonly IPdfBlobStorageService _pdfBlobStorageService;

    public CanvasService(
        IDbContextFactory<CanvasDbContext> factory,
        IDrawingStorageService drawingStorage,
        IPdfBlobStorageService pdfBlobStorageService)
    {
        _factory = factory;
        _drawingStorage = drawingStorage;
        _pdfBlobStorageService = pdfBlobStorageService;
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

        var existingData = await context.DrawingData
            .FirstOrDefaultAsync(d => d.LectureId == drawingData.LectureId && d.MemberId == drawingData.MemberId);

        if (existingData != null)
        {
            existingData.Drawings = drawingData.Drawings;
            context.DrawingData.Update(existingData);
        }
        else
        {
            context.DrawingData.Add(drawingData);
        }

        await context.SaveChangesAsync();
    }

    public async Task DeleteLectureAsync(string lectureId)
    {
        using var context = await _factory.CreateDbContextAsync();
        var lecture = await context.Lectures.FindAsync(lectureId);
        if (lecture != null)
        {
            await _pdfBlobStorageService.DeletePdfAsync(lecture.PdfFileAddress);
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

    public async Task<Lecture?> GetLectureAsync(string lectureId)
    {
        using var context = await _factory.CreateDbContextAsync();
        var lecture = await context.Lectures
            .Include(l => l.HostMember)
            .FirstOrDefaultAsync(l => l.Id == lectureId);

        return lecture;
    }

    public async Task<DrawingData?> GetDrawingDataAsync(string lectureId, string memberId)
    {
        using var context = await _factory.CreateDbContextAsync();
        var lecture = await context.Lectures.Include(lect => lect.HostMember).Where(l => l.Id == lectureId).FirstOrDefaultAsync();
        Console.WriteLine($"lectureId : {lectureId}");
        var answer = lecture.HostMember == null ? "true" : "false";
        Console.WriteLine($"lecture의 HostMember는 null 입니까..? : {answer}");

        if (lecture.HostMember.Id == memberId)
        {
            var cachedDrawings = await _drawingStorage.GetAsync(lectureId);
            if (cachedDrawings is not null)
            {
                Console.WriteLine($"Redis에서 가져옴!!");
                var drawingData = new DrawingData();
                drawingData.LectureId = lectureId;
                drawingData.MemberId = memberId;
                drawingData.Drawings = cachedDrawings;

                return drawingData;
            }
            else
            {
                Console.WriteLine($"DB에서 가져옴!!");
                var drawingData = await context.DrawingData
                    .Where(dd => dd.LectureId == lectureId && dd.MemberId == memberId)
                    .FirstOrDefaultAsync();

                return drawingData;
            }
        }
        else
        {
            Console.WriteLine($"DB에서 가져옴!!");
            var drawingData = await context.DrawingData
                .Where(dd => dd.LectureId == lectureId && dd.MemberId == memberId)
                .FirstOrDefaultAsync();

            return drawingData;
        }
    }

    public async Task<Lecture?> GetLectureByCodeAsync(string code)
    {
        using var context = await _factory.CreateDbContextAsync();
        var lecture = await context.Lectures
                .Where(lecture => lecture.Code == code)
                .FirstOrDefaultAsync();

        return lecture;
    }

    public async Task<Member> GetMemberAsync(string memberId)
    {
        using  var context = await _factory.CreateDbContextAsync();

        return await context.Members.Where(member => member.Id == memberId).FirstOrDefaultAsync();
    }
}