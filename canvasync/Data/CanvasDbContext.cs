using Microsoft.EntityFrameworkCore;
using canvasync.Library.Models;
using System.Text.Json;
using canvasync.Library.Dtos;

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
            .Property(d => d.Drawings)
            .HasColumnType("jsonb");
    }
}