using System.ComponentModel.DataAnnotations;

namespace CodifyProjectsBackend.Models.Dto;

public class CreateProjectDto
{
    [Required]
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    [Required]
    public string Url { get; set; } = string.Empty;
    [Required]
    public string Course { get; set; } = string.Empty;

    [Required]
    public Guid AuthorId { get; set; }

    public IFormFileCollection? MediaFiles { get; set; }
    public List<int>? MediaFileIndexes { get; set; }
    public List<string>? MediaUrls { get; set; }
    public List<string>? MediaIds { get; set; }

    public IFormFileCollection? ProjectFiles { get; set; }

    public string? CodeStructure { get; set; }
}
