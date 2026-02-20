using CodifyProjectsBackend.Models;
using CodifyProjectsBackend.Models.Dto;
using CodifyProjectsBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace CodifyProjectsBackend.Controllers;

public partial class ProjectsController : Controller
{

    JsonSerializerOptions jsonSerializerOptions = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        WriteIndented = true
    };

    [HttpGet]
    [Authorize]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    [Authorize]
    public IActionResult Create()
    {
        return View("Create", null);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit([FromServices] AppDbContext db, [FromServices] CodeStructureParser codeStructureParser, Guid id)
    {
        if (id == Guid.Empty)
        {
            return RedirectToAction("Create");
        }

        var project = await db.Projects.Include(p => p.Author).Include(p => p.Medias).FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
        {
            return RedirectToAction("Create");
        }

        return View("Create", new EditProjectDto()
        {
            Id = project.Id,
            Title = project.Title,
            Description = project.Description,
            Url = project.EnteredPath,
            Course = project.Course,
            AuthorId = project.AuthorId,
            AuthorFullName = project.Author.FullName,
            AuthorAge = project.Author.Age,
            AuthorPhotoUrl = project.Author.PhotoUrl,
            AuthorPreviousSkills = [.. project.Author.PreviousSkills],
            AuthorObtainedSkills = [.. project.Author.ObtainedSkills],
            Medias = [.. project.Medias.Select(m => new MediaDto() { Id = m.Id, Url = m.Url, Type = m.Type })],
            LoadedProjectFilesCount = project.LoadedProjectFilesCount,
            CodeStructure = project.Code == null ? null : codeStructureParser.ConvertToDto(JsonSerializer.Deserialize<Folder>(project.Code) ?? new())
        });
    }


    [HttpGet]
    public async Task<IActionResult> Get([FromServices]AppDbContext db, [FromQuery]string? search)
    {
        var query = db.Projects.Include(p => p.Author).AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p => p.Title.ToLower().Contains(search.ToLower()));
        }

        var projects = await query
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.Description,
                p.Url,
                p.CanvaUrl,
                p.Course,
                Author = new
                {
                    p.Author.FullName,
                    p.Author.Age,
                    p.Author.Testimonial,
                    p.Author.PhotoUrl,
                    p.Author.PreviousSkills,
                    p.Author.ObtainedSkills,
                }
            })
            .Take(1000)
            .ToListAsync();

        return Json(projects);
    }

    [HttpGet]
    public async Task<IActionResult> GetDetailed([FromServices] AppDbContext db, [FromServices] CodeStructureParser codeStructureParser, [FromQuery] Guid id)
    {
        var project = await db.Projects
            .Include(p => p.Author)
            .Include(p => p.Medias)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
        {
            return NotFound();
        }

        var result = new
        {
            project.Id,
            project.Title,
            project.Description,
            project.Url,
            project.CanvaUrl,
            project.Course,
            Medias = project.Medias
                .Select(m => new
                {
                    m.Id,
                    m.Url,
                    Type = m.Type == Models.MediaType.Image ? "image" : "video"
                }),
            Author = new
            {
                project.Author.FullName,
                project.Author.Age,
                project.Author.Testimonial,
                project.Author.PhotoUrl,
                project.Author.PreviousSkills,
                project.Author.ObtainedSkills,
            },
            Code = project.Code == null ? null : JsonSerializer.Deserialize<Folder>(project.Code)
        };

        return Json(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Add([FromServices] AppDbContext db, [FromServices]IMediaService mediaService, [FromServices]IProjectService projectService, [FromServices]CodeStructureParser codeStructureParser, [FromForm] CreateProjectDto dto)
    {
        if (dto == null || !ModelState.IsValid)
        {
            return BadRequest();
        }

        Author? author = await db.Authors.FirstOrDefaultAsync(a => a.Id == dto.AuthorId);

        if (author == null) 
        {
            return NotFound();
        }

        var mainUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

        List<Media> medias = new();

        if (dto.MediaFileIndexes != null && dto.MediaIds != null && dto.MediaUrls != null &&
            dto.MediaFileIndexes.Count == dto.MediaIds.Count && dto.MediaFileIndexes.Count == dto.MediaUrls.Count)
        {
            for (int i = 0; i < dto.MediaIds.Count; i++)
            {
                IFormFile? file = null;
                var fileIndex = dto.MediaFileIndexes[i];

                if (dto.MediaFiles != null && dto.MediaFiles.Count > fileIndex && fileIndex >= 0)
                {
                    file = dto.MediaFiles[fileIndex];
                }

                var url = dto.MediaUrls[i];
                var id = dto.MediaIds[i];
                if (file != null)
                {
                    var mediaData = mediaService.SaveMedia(file);
                    if (mediaData == null)
                    {
                        continue;
                    }

                    switch (mediaData.Type)
                    {
                        case Services.MediaType.Video:
                            medias.Add(new() { Url = mediaData.Path, PhysicalPath = mediaData.PhysicalPath, Type = Models.MediaType.Video });
                            break;
                        case Services.MediaType.ImagePng:
                        case Services.MediaType.ImageJpeg:
                            medias.Add(new() { Url = mediaData.Path, PhysicalPath = mediaData.PhysicalPath, Type = Models.MediaType.Image });
                            break;
                    }
                }
            }
        }

        Guid projectId = Guid.NewGuid();

        var isAbsoluteUrl = IsAbsoluteUrlRegex().IsMatch(dto.Url);

        var finalUrl = dto.Url;
        var enteredPath = dto.Url;
        finalUrl = finalUrl.TrimStart('/');
        string? physicalPath = null;

        var loadedProjectFilesCount = 0;

        if (dto.ProjectFiles != null)
        {
            ProjectData? projectData = projectService.SaveProject(projectId.ToString(), dto.ProjectFiles);
            if (projectData != null)
            {
                finalUrl = isAbsoluteUrl ? finalUrl : Path.Combine(projectData.WebPath, finalUrl);
                physicalPath = projectData.PhysicalPath;
                loadedProjectFilesCount = dto.ProjectFiles.Count;
            }
        }

        Folder? code = null;
        if (!string.IsNullOrEmpty(dto.CodeStructure))
        {
            code = codeStructureParser.ParseCodeStructure(dto.CodeStructure);
        }

        var newProject = new Project()
        {
            Id = projectId,
            Title = dto.Title,
            Description = dto.Description,
            Url = finalUrl,
            EnteredPath = enteredPath,
            PhysicalPath = physicalPath,
            Course = dto.Course,
            Author = author,
            Code = JsonSerializer.Serialize(code, jsonSerializerOptions),
            Medias = medias,
            LoadedProjectFilesCount = loadedProjectFilesCount,
        };

        db.Projects.Add(newProject);
        await db.SaveChangesAsync();

        return Ok();
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update(
    [FromServices] AppDbContext db,
    [FromServices] IMediaService mediaService,
    [FromServices] IProjectService projectService,
    [FromServices] CodeStructureParser codeStructureParser,
    [FromForm] UpdateProjectDto dto)
    {
        if (dto == null || !ModelState.IsValid)
        {
            return BadRequest();
        }

        Project? project = await db.Projects
            .Include(p => p.Medias)
            .FirstOrDefaultAsync(p => p.Id == dto.Id);

        if (project == null)
        {
            return NotFound();
        }

        if (project.AuthorId != dto.AuthorId && dto.IsAuthorChanged)
        {
            Author? author = await db.Authors.FirstOrDefaultAsync(a => a.Id == dto.AuthorId);
            if (author == null)
            {
                return NotFound();
            }
            project.AuthorId = author.Id;
        }

        if (dto.IsMediaChanged)
        {
            List<Media> newMedias = new();

            if (dto.MediaFileIndexes != null && dto.MediaIds != null && dto.MediaUrls != null &&
                dto.MediaFileIndexes.Count == dto.MediaIds.Count && dto.MediaFileIndexes.Count == dto.MediaUrls.Count)
            {
                for (int i = 0; i < dto.MediaIds.Count; i++)
                {
                    IFormFile? file = null;
                    var fileIndex = dto.MediaFileIndexes[i];

                    if (dto.MediaFiles != null && dto.MediaFiles.Count > fileIndex && fileIndex >= 0)
                    {
                        file = dto.MediaFiles[fileIndex];
                    }

                    if (file != null)
                    {
                        var mediaData = mediaService.SaveMedia(file);
                        if (mediaData == null) continue;

                        var mediaType = mediaData.Type switch
                        {
                            Services.MediaType.Video => Models.MediaType.Video,
                            Services.MediaType.ImagePng or Services.MediaType.ImageJpeg => Models.MediaType.Image,
                            _ => (Models.MediaType?)null
                        };

                        if (mediaType.HasValue)
                        {
                            newMedias.Add(new Media
                            {
                                Url = mediaData.Path,
                                PhysicalPath = mediaData.PhysicalPath,
                                Type = mediaType.Value
                            });
                        }
                    }
                    else
                    {
                        var id = dto.MediaIds[i];
                        if (!Guid.TryParse(id, out Guid guid)) continue;

                        var existingMedia = project.Medias.FirstOrDefault(m => m.Id == guid);
                        if (existingMedia != null)
                        {
                            newMedias.Add(existingMedia);
                        }
                    }
                }
            }

            // Удаляем медиа, которых нет в новом списке
            var mediasToRemove = project.Medias
                .Where(m => !newMedias.Any(nm => nm.Id == m.Id))
                .ToList();

            foreach (var media in mediasToRemove)
            {
                if (media.PhysicalPath != null)
                {
                    mediaService.DeleteMedia(media.PhysicalPath);
                }
                db.Medias.Remove(media);
            }

            // Добавляем новые медиа
            var mediasToAdd = newMedias
                .Where(nm => nm.ProjectId == Guid.Empty) // Только новые записи
                .ToList();

            foreach (var media in mediasToAdd)
            {
                media.Project = project;
                db.Medias.Add(media);
            }
        }

        if (dto.IsUrlChanged || dto.IsProjectChanged)
        {
            var isAbsoluteUrl = IsAbsoluteUrlRegex().IsMatch(dto.Url);
            var finalUrl = dto.Url;
            var enteredPath = dto.Url;
            finalUrl = finalUrl.TrimStart('/');
            string? physicalPath = null;

            if (dto.IsProjectChanged)
            {
                if (dto.ProjectFiles != null)
                {
                    ProjectData? projectData = projectService.SaveProject(dto.Id.ToString(), dto.ProjectFiles);
                    if (projectData != null)
                    {
                        finalUrl = isAbsoluteUrl ? finalUrl : Path.Combine(projectData.WebPath, finalUrl);
                        physicalPath = projectData.PhysicalPath;
                        project.LoadedProjectFilesCount = dto.ProjectFiles.Count;
                    }
                }
                else
                {
                    projectService.DeleteProject(dto.Id.ToString());
                }
            }
            else
            {
                ProjectData? projectData = projectService.GetProjectData(dto.Id.ToString());
                if (projectData != null)
                {
                    finalUrl = isAbsoluteUrl ? finalUrl : Path.Combine(projectData.WebPath, finalUrl);
                    physicalPath = projectData.PhysicalPath;
                }
            }

            project.Url = finalUrl;
            project.EnteredPath = enteredPath;
            project.PhysicalPath = physicalPath;
        }

        if (!string.IsNullOrEmpty(dto.CodeStructure))
        {
            project.Code = JsonSerializer.Serialize(codeStructureParser.ParseCodeStructure(dto.CodeStructure));
        }
        else
        {
            project.Code = "";
        }

        project.Title = dto.Title;
        project.Description = dto.Description;
        project.Course = dto.Course;

        await db.SaveChangesAsync();
        return Ok();
    }

    [GeneratedRegex("^https?://", RegexOptions.IgnoreCase, "ru-RU")]
    private static partial Regex IsAbsoluteUrlRegex();

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> Delete([FromServices] AppDbContext db, [FromServices] IMediaService mediaService, [FromServices] IProjectService projectService, Guid id)
    {
        if (id == Guid.Empty)
        {
            return BadRequest();
        }

        var project = await db.Projects.Include(p => p.Medias).FirstOrDefaultAsync(a => a.Id == id);

        if (project == null)
        {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(project.PhysicalPath))
        {
            projectService.DeleteProject(project.Id.ToString());
        }

        foreach (var media in project.Medias)
        {
            if (!string.IsNullOrEmpty(media.PhysicalPath))
            {
                mediaService.DeleteMedia(media.PhysicalPath);
            }
        }

        db.Projects.Remove(project);
        await db.SaveChangesAsync();

        return Ok();
    }
}
