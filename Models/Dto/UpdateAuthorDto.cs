using System.ComponentModel.DataAnnotations;

namespace CodifyProjectsBackend.Models.Dto;

public class UpdateAuthorDto
{
    public Guid AuthorId { get; set; }
    [Required]
    public string FullName { get; set; } = string.Empty;
    public bool FullNameChanged { get; set; } = false;
    [Range(0, 150)]
    public int Age { get; set; }
    public bool AgeChanged { get; set; } = false;
    public IFormFile? Photo { get; set; }
    public bool PhotoChanged { get; set; } = false;

    public string Testimonial { get; set; } = string.Empty;
    public bool TestimonialChanged { get; set; } = false;

    public ICollection<string> PreviousSkills { get; set; } = [];
    public bool PreviousSkillsChanged { get; set; } = false;
    public ICollection<string> ObtainedSkills { get; set; } = [];
    public bool ObtainedSkillsChanged { get; set; } = false;
}
