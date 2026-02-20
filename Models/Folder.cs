namespace CodifyProjectsBackend.Models;

public class Folder
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? ParentFolderId { get; set; }

    public string Name { get; set; } = string.Empty;
    public ICollection<File> ContentFiles { get; set; } = [];
    public ICollection<Folder> ContentFolders { get; set; } = [];
}
