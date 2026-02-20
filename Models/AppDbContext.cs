using Microsoft.EntityFrameworkCore;

namespace CodifyProjectsBackend.Models;

public class AppDbContext : DbContext
{
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<Author> Authors { get; set; } = null!;
    public DbSet<Media> Medias { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>().ToTable("Accounts");
        modelBuilder.Entity<Project>().ToTable("Projects");
        modelBuilder.Entity<Author>().ToTable("Authors");
        modelBuilder.Entity<Media>().ToTable("Medias");

        modelBuilder.Entity<Project>()
            .HasOne(p => p.Author)
            .WithMany(a => a.Projects)
            .HasForeignKey(p => p.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Media>()
            .HasOne(m => m.Project)
            .WithMany(p => p.Medias)
            .HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
} 
