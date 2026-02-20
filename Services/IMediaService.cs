namespace CodifyProjectsBackend.Services;

public interface IMediaService
{
    public MediaData? SaveMedia(IFormFile file);
    public void DeleteMedia(string path);
}

public class MediaData(string path = "", string physicalPath = "", MediaType type = MediaType.Unknown)
{
    public string Path { get; set; } = path;
    public string PhysicalPath { get; set; } = physicalPath;
    public MediaType Type { get; set; } = type;
}

public enum MediaType
{
    Unknown,
    Video,
    ImagePng,
    ImageJpeg
}