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
        modelBuilder.Entity<Member>(entity =>
        {
            entity.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(m => m.Password)
                .IsRequired();

            entity.HasIndex(m => m.Name)
                .IsUnique();
        });

        modelBuilder.Entity<Lecture>(entity =>
        {
            entity.Property(l => l.Code)
                .IsRequired()
                .HasMaxLength(6);

            entity.Property(l => l.FileName)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasIndex(l => l.Code)
                .IsUnique();
        });

        modelBuilder.Entity<DrawingData>(entity =>
        {
            entity.Property(d => d.LectureId)
                .IsRequired();

            entity.Property(d => d.MemberId)
                .IsRequired();

            entity.Property(d => d.Drawings)
                .HasColumnType("jsonb");

            entity.HasIndex(d => new { d.LectureId, d.MemberId })
                .IsUnique();

            entity.HasOne(d => d.Lecture)
                .WithMany()
                .HasForeignKey(d => d.LectureId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Member)
                .WithMany()
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
