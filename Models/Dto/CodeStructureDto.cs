namespace CodifyProjectsBackend.Models.Dto;

public class CodeStructureDto
{
    public List<FolderDto> Folders { get; set; } = new();
    public List<FileDto> Files { get; set; } = new();
}

public class FolderDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ParentId { get; set; }
    public List<FolderDto> Folders { get; set; } = new();
    public List<FileDto> Files { get; set; } = new();
    public bool Expanded { get; set; } = true;
}

public class FileDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public string Content { get; set; } = string.Empty;
    public string ParentId { get; set; } = string.Empty;
}