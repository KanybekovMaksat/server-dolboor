using System.ComponentModel.DataAnnotations;

namespace CodifyProjectsBackend.Models.Dto;

public class EditProjectDto
{
    public Guid Id { get; set; }
    [Required]
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Url { get; set; } = string.Empty;

    public string? CanvaUrl { get; set; }
    public string? GithubUrl { get; set; }

    [Required]
    public string Course { get; set; } = string.Empty;
    [Required]
    public string Mentor { get; set; } = string.Empty;

    [Required]
    public List<AuthorDto> Authors { get; set; } = [];

    public List<MediaDto>? Medias { get; set; }

    public int LoadedProjectFilesCount { get; set; }

    public CodeStructureDto? CodeStructure { get; set; }
}