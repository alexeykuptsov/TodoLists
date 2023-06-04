using EasyExceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TodoLists.App.Entities;
using TodoLists.App.Models;
using TodoLists.App.Services;

namespace TodoLists.App.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly TodoListsDbContext myListsDbContext;
    private readonly IUserService myUserService;

    public ProjectsController(TodoListsDbContext listsDbContext, IUserService userService)
    {
        myListsDbContext = listsDbContext;
        myUserService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects()
    {
        return await myListsDbContext.Projects
            .Where(x => x.ProfileId == myUserService.GetCurrentUserProfileId())
            .Select(x => ItemToDto(x))
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDto>> GetProject(long id)
    {
        var project = await myListsDbContext.Projects.FindAsync(id);

        if (project == null)
        {
            return NotFound();
        }

        return ItemToDto(project);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutProject(long id, ProjectDto projectDto)
    {
        if (id != projectDto.Id)
        {
            return BadRequest();
        }

        var project = await myListsDbContext.Projects.FindAsync(id);
        if (project == null)
        {
            return NotFound();
        }

        project.Name = projectDto.Name;

        try
        {
            await myListsDbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException) when (!ProjectExists(id))
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> PostProject(ProjectDto projectDto)
    {
        var project = new Project
        {
            Profile = await myListsDbContext.Profiles.SingleAsync(x => x.Id == myUserService.GetCurrentUserProfileId()),
            Name = projectDto.Name,
        };

        myListsDbContext.Projects.Add(project);
        await myListsDbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProject(long id)
    {
        var project = await myListsDbContext.Projects.FindAsync(id);
        if (project == null)
        {
            return NotFound();
        }

        myListsDbContext.Projects.Remove(project);
        await myListsDbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpPatch]
    public async Task<IActionResult> PatchProject([FromBody] dynamic body)
    {
        return await WithExceptionHandling(async () =>
        {
            foreach (dynamic change in body.EnumerateArray())
            {
                long id = change.GetProperty("key").GetProperty("id").GetInt64();
                foreach (dynamic property in change.GetProperty("data").EnumerateObject())
                {
                    string propertyName = property.Name;
                    if (propertyName == "name")
                    {
                        string propertyValue = property.Value.GetString();
                        await myListsDbContext.Projects.Where(x => x.Id == id).
                            ExecuteUpdateAsync(x => x.SetProperty(t => t.Name, propertyValue));
                    }
                }
            }
            return Ok();
        });
    }

    private static async Task<T> WithExceptionHandling<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception e)
        {
            Log.Error("Unhandled exception: {Exception}", ExceptionDumpUtil.Dump(e));
            throw;
        }
    }

    private bool ProjectExists(long id)
    {
        return myListsDbContext.Projects.Any(e => e.Id == id);
    }

    private static ProjectDto ItemToDto(Project project) => new()
    {
        Id = project.Id,
        Name = project.Name,
    };
}