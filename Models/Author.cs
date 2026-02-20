namespace CodifyProjectsBackend.Models;

public class Author
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string? PhotoUrl { get; set; }
    public string? PhysicalPhotoPath { get; set; }
    public ICollection<string> PreviousSkills { get; set; } = [];
    public ICollection<string> ObtainedSkills { get; set; } = [];
    public string? Testimonial { get; set; }

    public ICollection<Project> Projects { get; set; } = [];
}
