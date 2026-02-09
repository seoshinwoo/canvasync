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
            .OwnsOne(d => d.Drawings, builder => // Drawings 속성을 소유(OwnsOne)하는 관계임을 명시
            {
                builder.ToJson(); // 핵심: 이 객체(Drawings)를 별도 테이블이 아닌 JSON 문자열로 저장하라고 지시

                // Drawings 안에 있는 PageDtos 리스트도 JSON 구조 안에 포함됨을 명시
                builder.OwnsMany(x => x.PageDtos, pageBuilder =>
                {
                    // PageDtos 안에 있는 FactorDtos 리스트도 깊은 구조로 포함됨을 명시
                    pageBuilder.OwnsMany(p => p.FactorDtos, factorBuilder =>
                    {
                        // 더 깊은 계층 구조 정의...
                        factorBuilder.OwnsMany(f => f.TextBlockDtos);
                        factorBuilder.OwnsOne(f => f.PenPathDto);
                    });
                });
            });
    }
}