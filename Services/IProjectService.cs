namespace CodifyProjectsBackend.Services;

public interface IProjectService
{
    ProjectData? SaveProject(string projectName, IFormFileCollection files);
    ProjectData? GetProjectData(string projectName);
    void DeleteProject(string projectName);
}

public class ProjectData
{
    public string WebPath { get; set; } = string.Empty;
    public string PhysicalPath { get; set; } = string.Empty;
}