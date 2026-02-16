using Microsoft.EntityFrameworkCore;
using canvasync.Library.Models;

namespace canvasync.Data;
public class CanvasDbContext : DbContext
{
    public CanvasDbContext(DbContextOptions<CanvasDbContext> options) : base(options)
    {
        
    }

    public DbSet<Lecture> Lectures { get; set; }
    public DbSet<Member> Members {get;set;}
    public DbSet<DrawingData> DrawingData { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // DrawingData 엔티티에 대한 설정을 시작합니다.
        modelBuilder.Entity<DrawingData>()
            .OwnsMany(d => d.Drawings, builder => // Drawings 가 이제 List<PageDto> 이므로 OwnsMany 사용
            {
                builder.ToJson(); // 이 리스트(Drawings)를 JSON 배열로 저장하도록 설정

                // PageDto 내부의 FactorDtos 리스트 설정
                builder.OwnsMany(p => p.FactorDtos, factorBuilder =>
                {
                    // FactorDto 내부의 중첩 객체들 설정
                    factorBuilder.OwnsMany(f => f.TextBlockDtos);
                    factorBuilder.OwnsOne(f => f.PenPathDto);
                });
            });
    }
}