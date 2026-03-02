using System.ComponentModel.DataAnnotations;

namespace CodifyProjectsBackend.Models.Dto;

public class AuthorDto
{
    [Required]
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string? PhotoUrl { get; set; }
    public List<string> PreviousSkills { get; set; } = [];
    public List<string> ObtainedSkills { get; set; } = [];
}
