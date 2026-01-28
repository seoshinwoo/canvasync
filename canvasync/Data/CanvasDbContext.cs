using Microsoft.EntityFrameworkCore;
using canvasync.Library.Models;

namespace canvasync.Data;
public class CanvasDbContext : DbContext
{
    public CanvasDbContext(DbContextOptions<CanvasDbContext> options) : base(options)
    {
        
    }

    public DbSet<Lecture> Lectures { get; set; }
}