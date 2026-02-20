namespace CodifyProjectsBackend.Services;

public class MediaService : IMediaService
{
    public string MediaPath { get; }
    public string WebMediaPath { get; }
    public MediaService()
    {
        MediaPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "storage/media");
        WebMediaPath = "/storage/media";
    }

    public void EnsureCreated()
    {
        if (!Directory.Exists(MediaPath))
        {
            Directory.CreateDirectory(MediaPath);
        }
    }

    public MediaData? SaveMedia(IFormFile file)
    {
        EnsureCreated();

        if (file == null)
        {
            return null;
        }

        Guid guid = Guid.NewGuid();
        var contentType = file.ContentType;
        string? filePath;
        string? fileWebPath;
        MediaType type = MediaType.Unknown;

        switch (contentType)
        {
            case "video/mp4":
                filePath = Path.Combine(MediaPath, $"{guid}.mp4");
                fileWebPath = Path.Combine(WebMediaPath, $"{guid}.mp4");
                type = MediaType.Video;
                break;
            case "image/png":
                filePath = Path.Combine(MediaPath, $"{guid}.png");
                fileWebPath = Path.Combine(WebMediaPath, $"{guid}.png");
                type = MediaType.ImagePng;
                break;
            case "image/jpeg":
                filePath = Path.Combine(MediaPath, $"{guid}.jpg");
                fileWebPath = Path.Combine(WebMediaPath, $"{guid}.jpg");
                type = MediaType.ImageJpeg;
                break;
            default:
                filePath = Path.Combine(MediaPath, guid.ToString());
                fileWebPath = Path.Combine(WebMediaPath, guid.ToString());
                break;
        }

        if (File.Exists(filePath))
        {
            return new(filePath);
        }

        using FileStream fileStream = new(filePath, FileMode.Create);
        file.CopyTo(fileStream);

        return new(fileWebPath, filePath, type);
    }
    public void DeleteMedia(string path)
    {
        File.Delete(path);
    }
}
