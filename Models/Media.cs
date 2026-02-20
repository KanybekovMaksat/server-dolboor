namespace CodifyProjectsBackend.Models;

public class Media
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;


    public MediaType Type { get; set; }

    public string? Url { get; set; }
    public string? PhysicalPath { get; set; }
}

public enum MediaType
{
    Image,
    Video
}