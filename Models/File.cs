namespace CodifyProjectsBackend.Models;

public class File
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ParentFolderId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public string Language { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
