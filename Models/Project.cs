namespace CodifyProjectsBackend.Models;

public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? EnteredPath { get; set; }
    public string? Url { get; set; }
    public string? PhysicalPath { get; set; }
    public string Course { get; set; } = string.Empty;
    public string CanvaUrl { get; set; } = string.Empty;

    public Guid AuthorId { get; set; }
    public Author Author { get; set; } = null!;

    public ICollection<Media> Medias { get; set; } = null!;

    public int LoadedProjectFilesCount { get; set; }

    public string? Code { get; set; }
}
