namespace CodifyProjectsBackend.Services;

public class ProjectService : IProjectService
{
    public string PhysicalPath { get; }
    public string WebPath { get; }
    public ProjectService()
    {
        PhysicalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "storage/projects");
        WebPath = "/storage/projects";
    }

    public void EnsureCreatedDirectory() 
    {
        if (!Directory.Exists(PhysicalPath))
        {
            Directory.CreateDirectory(PhysicalPath);
        }
    }

    public ProjectData? SaveProject(string projectName, IFormFileCollection files)
    {
        if (string.IsNullOrEmpty(projectName)) 
        {
            return null;
        }


        EnsureCreatedDirectory();
        var path = Path.Combine(PhysicalPath, projectName);
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }

        ProjectData data = new();

        var projectPhysicalPath = Path.Combine(PhysicalPath, projectName);
        var projectWebPath = Path.Combine(WebPath, projectName);
        data.PhysicalPath = projectPhysicalPath;
        data.WebPath = projectWebPath;

        foreach (var file in files)
        {
            var filePathSlices = file.FileName.Split('/').ToList();
            filePathSlices.RemoveAt(0);
            if (filePathSlices.Count <= 0)
            {
                continue;
            }

            var fileSingleName = filePathSlices[^1];
            var fileDirectorySlices = new string[filePathSlices.Count - 1];
            filePathSlices.CopyTo(0, fileDirectorySlices, 0, filePathSlices.Count - 1);

            var filePhysicalDirectoryPath = Path.Combine(projectPhysicalPath, Path.Combine(fileDirectorySlices));
            var filePhysicalFullPath = Path.Combine(filePhysicalDirectoryPath, fileSingleName);
            if (!Directory.Exists(filePhysicalDirectoryPath))
            {
                Directory.CreateDirectory(filePhysicalDirectoryPath);
            }

            using FileStream fileStream = new(filePhysicalFullPath, FileMode.OpenOrCreate);
            file.CopyTo(fileStream);

        }

        return data;
    }

    public void DeleteProject(string projectName)
    {
        if (string.IsNullOrEmpty(projectName))
        {
            return;
        }


        EnsureCreatedDirectory();
        var path = Path.Combine(PhysicalPath, projectName);
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }

    public ProjectData? GetProjectData(string projectName)
    {
        EnsureCreatedDirectory();
        var path = Path.Combine(PhysicalPath, projectName);
        if (!Directory.Exists(path))
        {
            return null;
        }

        ProjectData data = new();

        var projectPhysicalPath = Path.Combine(PhysicalPath, projectName);
        var projectWebPath = Path.Combine(WebPath, projectName);
        data.PhysicalPath = projectPhysicalPath;
        data.WebPath = projectWebPath;

        return data;
    }
}
