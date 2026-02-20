namespace CodifyProjectsBackend.Models.Dto;

public class MediaDto
{
    public Guid? Id { get; set; }
    public string? Url { get; set; }
    public IFormFile? File { get; set; }
    public MediaType Type { get; set; }
}
