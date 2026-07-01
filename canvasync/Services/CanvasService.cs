using canvasync.Data;
using canvasync.Library.Models;
using canvasync.Library.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Security.Cryptography;

namespace canvasync.Services;

public class CanvasService : ICanvasService
{
    private const int LectureCodeRetryLimit = 10;

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

    private static string CreateLectureCode()
        => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

    private static async Task<string> CreateAvailableLectureCodeAsync(CanvasDbContext context)
    {
        for (var attempt = 0; attempt < LectureCodeRetryLimit; attempt++)
        {
            var code = CreateLectureCode();
            var exists = await context.Lectures
                .AsNoTracking()
                .AnyAsync(l => l.Code == code);

            if (!exists)
            {
                return code;
            }
        }

        throw new InvalidOperationException("강의 입장 코드를 생성하지 못했습니다.");
    }

    private static bool IsUniqueViolation(DbUpdateException exception, string constraintName)
    {
        if (exception.InnerException is not PostgresException postgresException)
        {
            return false;
        }

        return postgresException.SqlState == PostgresErrorCodes.UniqueViolation
            && postgresException.ConstraintName == constraintName;
    }

    // Lecture 추가
    public async Task AddLectureAsync(Lecture lecture, string memberId)
    {
        if (lecture == null || string.IsNullOrWhiteSpace(memberId))
        {
            return;
        }

        for (var attempt = 1; attempt <= LectureCodeRetryLimit; attempt++)
        {
            using var context = await _factory.CreateDbContextAsync();
            var member = await context.Members
                .FirstOrDefaultAsync(m => m.Id == memberId);

            if (member == null)
            {
                return;
            }

            lecture.Code = await CreateAvailableLectureCodeAsync(context);
            lecture.HostMember = member;

            context.Lectures.Add(lecture);

            try
            {
                await context.SaveChangesAsync();
                return;
            }
            catch (DbUpdateException ex)
                when (attempt < LectureCodeRetryLimit && IsUniqueViolation(ex, "IX_Lectures_Code"))
            {
                // 다른 요청이 같은 6자리 코드를 먼저 저장한 경우 새 코드로 재시도합니다.
            }
        }

        throw new InvalidOperationException("중복되지 않는 강의 입장 코드를 저장하지 못했습니다.");
    }

    public async Task<List<Lecture>> GetMyLecturesAsync(string memberId)
    {
        using var context = await _factory.CreateDbContextAsync();
        return await context.Lectures
            .AsNoTracking()
            .Where(l => l.HostMember.Id == memberId)
            .OrderBy(l => l.FileName)
            .ToListAsync();
    }

    public async Task<List<Lecture>> GetJoinedLecturesAsync(string memberId)
    {
        using var context = await _factory.CreateDbContextAsync();
        return await context.Lectures
            .AsNoTracking()
            .Where(l => l.Members!.Any(m => m.Id == memberId))
            .OrderBy(l => l.FileName)
            .ToListAsync();
    }

    public async Task JoinLectureAsync(string lectureId, string memberId)
    {
        using var context = await _factory.CreateDbContextAsync();
        var member = await context.Members
            .Include(m => m.JoinedLectures)
            .FirstOrDefaultAsync(m => m.Id == memberId);

        if (member == null) return;

        var targetLecture = await context.Lectures
            .Include(l => l.HostMember)
            .FirstOrDefaultAsync(l => l.Id == lectureId);
        if (targetLecture == null)
        {
             return;
        }

        if (targetLecture.HostMember.Id == memberId)
        {
            return;
        }

        member.JoinedLectures ??= new List<Lecture>();
        if (!member.JoinedLectures.Any(l => l.Id == targetLecture.Id))
        {
            member.JoinedLectures.Add(targetLecture);
        }

        await context.SaveChangesAsync();
    }

    public async Task SaveDrawingDataAsync(DrawingData drawingData)
    {
        using var context = await _factory.CreateDbContextAsync();

        var updatedRows = await context.DrawingData
            .Where(d => d.LectureId == drawingData.LectureId && d.MemberId == drawingData.MemberId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(d => d.Drawings, drawingData.Drawings));

        if (updatedRows > 0)
        {
            return;
        }

        context.DrawingData.Add(new DrawingData
        {
            LectureId = drawingData.LectureId,
            MemberId = drawingData.MemberId,
            Drawings = drawingData.Drawings
        });

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
            when (IsUniqueViolation(ex, "IX_DrawingData_LectureId_MemberId"))
        {
            await context.DrawingData
                .Where(d => d.LectureId == drawingData.LectureId && d.MemberId == drawingData.MemberId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(d => d.Drawings, drawingData.Drawings));
        }
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
            var lecture = member.JoinedLectures?.FirstOrDefault(l => l.Id == lectureId);
            if (lecture != null)
            {
                member.JoinedLectures!.Remove(lecture);
                await context.SaveChangesAsync();
            }
        }
    }

    public async Task<Lecture?> GetLectureAsync(string lectureId)
    {
        using var context = await _factory.CreateDbContextAsync();
        var lecture = await context.Lectures
            .AsNoTracking()
            .Include(l => l.HostMember)
            .FirstOrDefaultAsync(l => l.Id == lectureId);

        return lecture;
    }

    public async Task<DrawingData?> GetDrawingDataAsync(string lectureId, string memberId)
    {
        using var context = await _factory.CreateDbContextAsync();
        var hostMemberId = await context.Lectures
            .AsNoTracking()
            .Where(l => l.Id == lectureId)
            .Select(l => l.HostMember.Id)
            .FirstOrDefaultAsync();

        if (hostMemberId is null)
        {
            return null;
        }

        if (hostMemberId == memberId)
        {
            var cachedDrawings = await _drawingStorage.GetAsync(lectureId);
            if (cachedDrawings is not null)
            {
                return new DrawingData
                {
                    LectureId = lectureId,
                    MemberId = memberId,
                    Drawings = cachedDrawings
                };
            }
        }

        return await context.DrawingData
            .AsNoTracking()
            .Where(dd => dd.LectureId == lectureId && dd.MemberId == memberId)
            .FirstOrDefaultAsync();
    }

    public async Task<Lecture?> GetLectureByCodeAsync(string code)
    {
        using var context = await _factory.CreateDbContextAsync();
        var lecture = await context.Lectures
            .AsNoTracking()
            .Where(lecture => lecture.Code == code.Trim())
            .FirstOrDefaultAsync();

        return lecture;
    }

    public async Task<Member?> GetMemberAsync(string memberId)
    {
        using  var context = await _factory.CreateDbContextAsync();

        return await context.Members
            .AsNoTracking()
            .Where(member => member.Id == memberId)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> CanAccessLectureAsync(string lectureId, string memberId)
    {
        if (string.IsNullOrWhiteSpace(lectureId) || string.IsNullOrWhiteSpace(memberId))
        {
            return false;
        }

        using var context = await _factory.CreateDbContextAsync();
        return await context.Lectures
            .AsNoTracking()
            .AnyAsync(l => l.Id == lectureId
                && (l.HostMember.Id == memberId || l.Members!.Any(m => m.Id == memberId)));
    }

    public async Task<bool> CanReadDrawingDataAsync(string lectureId, string requestedMemberId, string authenticatedMemberId)
    {
        if (string.IsNullOrWhiteSpace(lectureId)
            || string.IsNullOrWhiteSpace(requestedMemberId)
            || string.IsNullOrWhiteSpace(authenticatedMemberId))
        {
            return false;
        }

        using var context = await _factory.CreateDbContextAsync();
        var access = await context.Lectures
            .AsNoTracking()
            .Where(l => l.Id == lectureId)
            .Select(l => new
            {
                HostMemberId = l.HostMember.Id,
                IsParticipant = l.HostMember.Id == authenticatedMemberId
                    || l.Members!.Any(m => m.Id == authenticatedMemberId)
            })
            .FirstOrDefaultAsync();

        return access is not null
            && access.IsParticipant
            && (requestedMemberId == authenticatedMemberId || requestedMemberId == access.HostMemberId);
    }

    public async Task<bool> IsLectureHostAsync(string lectureId, string memberId)
    {
        if (string.IsNullOrWhiteSpace(lectureId) || string.IsNullOrWhiteSpace(memberId))
        {
            return false;
        }

        using var context = await _factory.CreateDbContextAsync();
        return await context.Lectures
            .AsNoTracking()
            .AnyAsync(l => l.Id == lectureId && l.HostMember.Id == memberId);
    }
}
