using System.ComponentModel.DataAnnotations;

namespace CodifyProjectsBackend.Models.Dto;

public class UpdateProjectDto
{
    public Guid Id { get; set; }
    [Required]
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    [Required]
    public string Url { get; set; } = string.Empty;
    public bool IsUrlChanged { get; set; }
    [Required]
    public string Course { get; set; } = string.Empty;

    [Required]
    public Guid AuthorId { get; set; }
    public bool IsAuthorChanged { get; set; }

    public List<IFormFile?>? MediaFiles { get; set; }
    public List<int>? MediaFileIndexes { get; set; }
    public List<string>? MediaUrls { get; set; }
    public List<string>? MediaIds { get; set; }
    public bool IsMediaChanged { get; set; }

    public IFormFileCollection? ProjectFiles { get; set; }
    public bool IsProjectChanged { get; set; }

    public string? CodeStructure { get; set; }
}
