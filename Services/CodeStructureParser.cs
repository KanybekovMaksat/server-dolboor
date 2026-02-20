using CodifyProjectsBackend.Models;
using CodifyProjectsBackend.Models.Dto;
using System.Text.Json;
using File = CodifyProjectsBackend.Models.File;

public class CodeStructureParser
{
    JsonSerializerOptions options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Folder ParseCodeStructure(string jsonStructure)
    {
        if (string.IsNullOrWhiteSpace(jsonStructure))
        {
            // Создаем пустую корневую папку
            return new Folder
            {
                Id = Guid.NewGuid(),
                Name = "root",
                ParentFolderId = null,
                ContentFiles = new List<File>(),
                ContentFolders = new List<Folder>()
            };
        }

        var dto = JsonSerializer.Deserialize<CodeStructureDto>(jsonStructure, options);

        if (dto == null)
        {
            throw new ArgumentException("Invalid code structure JSON");
        }

        return ParseCodeStructure(dto);
    }

    public Folder ParseCodeStructure(CodeStructureDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentException("Invalid code structure JSON");
        }

        // Создаем корневую папку
        var rootFolder = new Folder
        {
            Id = Guid.NewGuid(),
            Name = "root",
            ParentFolderId = null,
            ContentFiles = new List<File>(),
            ContentFolders = new List<Folder>()
        };

        // Словарь для отслеживания соответствия временных ID и реальных Guid
        var folderIdMap = new Dictionary<string, Guid>();

        // Рекурсивно парсим структуру
        ParseFolders(dto.Folders, rootFolder, folderIdMap);

        return rootFolder;
    }

    private void ParseFolders(List<FolderDto> folderDtos, Folder parentFolder, Dictionary<string, Guid> folderIdMap)
    {
        foreach (var folderDto in folderDtos)
        {
            var folder = new Folder
            {
                Id = Guid.NewGuid(),
                Name = folderDto.Name,
                ParentFolderId = parentFolder.Id,
                ContentFiles = new List<File>(),
                ContentFolders = new List<Folder>()
            };

            // Сохраняем соответствие временного ID и реального Guid
            folderIdMap[folderDto.Id] = folder.Id;

            // Добавляем папку к родителю
            parentFolder.ContentFolders.Add(folder);

            // Парсим файлы в этой папке
            ParseFiles(folderDto.Files, folder, folderIdMap);

            // Рекурсивно парсим вложенные папки
            if (folderDto.Folders.Any())
            {
                ParseFolders(folderDto.Folders, folder, folderIdMap);
            }
        }
    }

    private void ParseFiles(List<FileDto> fileDtos, Folder parentFolder, Dictionary<string, Guid> folderIdMap)
    {
        foreach (var fileDto in fileDtos)
        {
            var file = new File
            {
                Id = Guid.NewGuid(),
                ParentFolderId = parentFolder.Id,
                Name = fileDto.Name,
                Language = fileDto.Language,
                Explanation = fileDto.Explanation,
                Content = fileDto.Content
            };

            parentFolder.ContentFiles.Add(file);
        }
    }

    public CodeStructureDto ConvertToDto(Folder rootFolder)
    {
        var dto = new CodeStructureDto
        {
            Folders = new List<FolderDto>()
        };

        if (rootFolder?.ContentFolders != null)
        {
            foreach (var folder in rootFolder.ContentFolders)
            {
                dto.Folders.Add(ConvertFolderToDto(folder));
            }
        }

        return dto;
    }

    private FolderDto ConvertFolderToDto(Folder folder)
    {
        var dto = new FolderDto
        {
            Id = folder.Id.ToString(),
            Name = folder.Name,
            ParentId = folder.ParentFolderId?.ToString(),
            Folders = new List<FolderDto>(),
            Files = new List<FileDto>(),
            Expanded = true
        };

        // Конвертируем файлы
        if (folder.ContentFiles != null)
        {
            foreach (var file in folder.ContentFiles)
            {
                dto.Files.Add(new FileDto
                {
                    Id = file.Id.ToString(),
                    Name = file.Name,
                    Language = file.Language,
                    Explanation = file.Explanation,
                    Content = file.Content,
                    ParentId = file.ParentFolderId.ToString()
                });
            }
        }

        // Рекурсивно конвертируем вложенные папки
        if (folder.ContentFolders != null)
        {
            foreach (var childFolder in folder.ContentFolders)
            {
                dto.Folders.Add(ConvertFolderToDto(childFolder));
            }
        }

        return dto;
    }
}