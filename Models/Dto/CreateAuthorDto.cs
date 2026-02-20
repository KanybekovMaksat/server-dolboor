using System.ComponentModel.DataAnnotations;

namespace CodifyProjectsBackend.Models.Dto;

public class CreateAuthorDto
{
    [Required]
    public string FullName { get; set; } = string.Empty;
    [Range(0, 150)]
    public int Age { get; set; }
    public IFormFile? Photo { get; set; } 

    public string Testimonial { get; set; } = string.Empty;

    public ICollection<string> PreviousSkills { get; set; } = [];
    public ICollection<string> ObtainedSkills { get; set; } = [];
}
