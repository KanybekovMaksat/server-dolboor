using System.ComponentModel.DataAnnotations;

namespace CodifyProjectsBackend.Models.Dto;

public class EditProjectDto
{
    public Guid Id { get; set; }
    [Required]
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    [Required]
    public string? Url { get; set; }
    [Required]
    public string Course { get; set; } = string.Empty;

    [Required]
    public Guid AuthorId { get; set; }
    public string AuthorFullName { get; set; } = string.Empty;
    public int AuthorAge { get; set; }
    public string? AuthorPhotoUrl { get; set; }
    public List<string> AuthorPreviousSkills { get; set; } = [];
    public List<string> AuthorObtainedSkills { get; set; } = [];

    public List<MediaDto>? Medias { get; set; }

    public int LoadedProjectFilesCount { get; set; }

    public CodeStructureDto? CodeStructure { get; set; }
}
