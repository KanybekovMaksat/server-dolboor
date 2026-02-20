using CodifyProjectsBackend.Models;
using CodifyProjectsBackend.Models.Dto;
using CodifyProjectsBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodifyProjectsBackend.Controllers;

public class AuthorsController : Controller
{
    [HttpGet]
    [Authorize]
    public IActionResult Index() => View();

    [HttpGet]
    [Authorize]
    public IActionResult Create() => View("Create", null);

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit([FromServices]AppDbContext db, Guid id) 
    {
        if (id == Guid.Empty)
        {
            return RedirectToAction("Create");
        }

        var author = await db.Authors.FirstOrDefaultAsync(a => a.Id == id);

        if (author == null)
        {
            return RedirectToAction("Create");
        }

        return View("Create", author); 
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Get([FromServices]AppDbContext db, [FromQuery]string? search)
    {
        var query = db.Authors.AsQueryable();

        if (search != null)
        {
            query = query.Where(a => a.FullName.ToLower().Contains(search.ToLower()));
        }

        var result = await query
            .Select(a => new
            {
                a.Id,
                a.FullName,
                a.Age,
                a.PhotoUrl,
                a.PreviousSkills,
                a.ObtainedSkills,
                a.Testimonial,
            })
            .Take(1000)
            .ToListAsync();

        return Json(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Add([FromServices]AppDbContext db, [FromServices]IMediaService mediaService, [FromForm]CreateAuthorDto dto)
    {
        if (dto == null || !ModelState.IsValid)
        {
            return BadRequest();
        }

        string? photoUrl = null;
        string? physicalPhotoPath = null;
        if (dto.Photo != null && (dto.Photo.ContentType == "image/png" || dto.Photo.ContentType == "image/jpeg"))
        {
            var mediaData = mediaService.SaveMedia(dto.Photo);
            if (mediaData != null)
            {
                photoUrl = mediaData.Path;
                physicalPhotoPath = mediaData.PhysicalPath;
            }
        }

        Author newAuthor = new()
        {
            FullName = dto.FullName,
            Age = dto.Age,
            PhotoUrl = photoUrl,
            PhysicalPhotoPath = physicalPhotoPath,
            Testimonial = dto.Testimonial,
            PreviousSkills = dto.PreviousSkills,
            ObtainedSkills = dto.ObtainedSkills,
        };

        db.Authors.Add(newAuthor);
        await db.SaveChangesAsync();

        return Ok();
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update([FromServices] AppDbContext db, [FromServices] IMediaService mediaService, [FromForm] UpdateAuthorDto dto)
    {
        if (dto == null || !ModelState.IsValid)
        {
            return BadRequest();
        }

        if (dto.AuthorId == Guid.Empty)
        {
            return BadRequest();
        }

        var author = await db.Authors.FirstOrDefaultAsync(a => a.Id == dto.AuthorId);

        if (author == null) 
        {
            return NotFound();
        }

        if (dto.PhotoChanged)
        {
            string? photoUrl = null;
            string? physicalPhotoPath = null;
            if (dto.Photo != null && (dto.Photo.ContentType == "image/png" || dto.Photo.ContentType == "image/jpeg"))
            {
                var mediaData = mediaService.SaveMedia(dto.Photo);
                if (mediaData != null)
                {
                    photoUrl = mediaData.Path;
                    physicalPhotoPath = mediaData.PhysicalPath;
                }
            }

            if (photoUrl != null && physicalPhotoPath != null)
            {
                if (!string.IsNullOrEmpty(author.PhysicalPhotoPath))
                {
                    mediaService.DeleteMedia(author.PhysicalPhotoPath);
                }
                author.PhotoUrl = photoUrl;
                author.PhysicalPhotoPath = physicalPhotoPath;
            }
        }

        if (dto.FullNameChanged)
        {
            author.FullName = dto.FullName;
        }

        if (dto.AgeChanged)
        {
            author.Age = dto.Age;
        }

        if (dto.TestimonialChanged)
        {
            author.Testimonial = dto.Testimonial;
        }

        if (dto.PreviousSkillsChanged)
        {
            author.PreviousSkills = dto.PreviousSkills;
        }

        if (dto.ObtainedSkillsChanged)
        {
            author.ObtainedSkills = dto.ObtainedSkills;
        }

        await db.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> Delete([FromServices] AppDbContext db, [FromServices] IMediaService mediaService, Guid id)
    {
        if (id == Guid.Empty)
        {
            return BadRequest();
        }

        var author = await db.Authors.FirstOrDefaultAsync(a => a.Id == id);

        if (author == null)
        {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(author.PhysicalPhotoPath))
        {
            mediaService.DeleteMedia(author.PhysicalPhotoPath);
        }

        db.Authors.Remove(author);
        await db.SaveChangesAsync();

        return Ok();
    }
}
