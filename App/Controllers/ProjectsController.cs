using System.Text.Json;
using EasyExceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    private readonly long myProfileId;

    public ProjectsController(TodoListsDbContext listsDbContext, IUserService userService)
    {
        myListsDbContext = listsDbContext;
        myUserService = userService;
        myProfileId = myUserService.GetCurrentUserProfileId();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects([FromQuery] string? projectName)
    {
        var allProjects = await myListsDbContext.Projects
            .Where(x => x.ProfileId == myUserService.GetCurrentUserProfileId())
            .Where(x => !x.IsDeleted)
            .Select(x => EntityToDto(x))
            .ToListAsync();
        if (projectName != null)
            return allProjects.Where(x => x.Name == projectName).ToList();
        return allProjects;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDto>> GetProject(long id)
    {
        var project = await myListsDbContext.Projects.FindAsync(id);

        if (project == null)
        {
            return NotFound();
        }

        return EntityToDto(project);
    }

    [HttpPost]
    public async Task<IActionResult> PostProject(ProjectDto projectDto)
    {
        var project = new Project
        {
            Profile = await myListsDbContext.Profiles.SingleAsync(x => x.Id == myUserService.GetCurrentUserProfileId()),
            Name = projectDto.Name,
            IsDeleted = projectDto.IsDeleted,
        };

        myListsDbContext.Projects.Add(project);
        await myListsDbContext.SaveChangesAsync();

        return Ok(project.Id);
    }

    [HttpPost("Clone")]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> CloneProject(CloneProjectDto clonedProjectDto)
    {
        return await WithExceptionHandling(async () =>
        {

            var clonedProject =
                myListsDbContext.Projects.Single(x => x.Id == clonedProjectDto.Id && x.ProfileId == myProfileId);

            var project = new Project
            {
                ProfileId = myProfileId,
                Name = clonedProject.Name,
                IsDeleted = clonedProject.IsDeleted,
            };
            myListsDbContext.Projects.Add(project);

            var clonedTodoItems = myListsDbContext.TodoItems
                .Where(x => x.ProfileId == myProfileId && x.ProjectId == clonedProjectDto.Id)
                .ToList();

            foreach (var clonedTodoItem in clonedTodoItems)
            {
                myListsDbContext.TodoItems.Add(new TodoItem
                {
                    ProfileId = myProfileId,
                    Name = clonedTodoItem.Name,
                    IsComplete = clonedTodoItem.IsComplete,
                    Project = project,
                });
            }

            await myListsDbContext.SaveChangesAsync();

            return await GetProjects(null);
        });
    }

    [HttpPatch]
    public async Task<ActionResult<string>> PatchProject([FromBody] JsonElement body)
    {
        return await WithExceptionHandling(async () =>
        {
            var rows = new JArray();
            foreach (var change in body.EnumerateArray())
            {
                var changeType = change.GetProperty("type").GetString();
                long? id;
                switch (changeType)
                {
                    case "insert":
                        id = await InsertProject(change.GetProperty("data"));
                        break;
                    case "update":
                        id = change.GetProperty("key").GetProperty("id").GetInt64();
                        await UpdateProject(change.GetProperty("data"), id.Value);
                        break;
                    case "remove":
                        id = change.GetProperty("key").GetProperty("id").GetInt64();
                        await DeleteProject(id.Value);
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                rows.Add(new JObject { { "id", id } });
            }

            return Ok();
        });
    }

    private async Task UpdateProject(JsonElement data, long id)
    {
        foreach (dynamic property in data.EnumerateObject())
        {
            string propertyName = property.Name;
            if (propertyName == "name")
            {
                string propertyValue = property.Value.GetString();
                await myListsDbContext.Projects
                    .Where(x => x.Id == id && x.ProfileId == myUserService.GetCurrentUserProfileId())
                    .ExecuteUpdateAsync(x => x.SetProperty(t => t.Name, propertyValue));
            }
        }
    }

    private async Task DeleteProject(long id)
    {
        await myListsDbContext.Projects
            .Where(x => x.Id == id && x.ProfileId == myUserService.GetCurrentUserProfileId())
            .ExecuteUpdateAsync(x => x.SetProperty(t => t.IsDeleted, true));
    }

    private async Task<long> InsertProject(JsonElement data)
    {
        var entity = new Project
        {
            ProfileId = myUserService.GetCurrentUserProfileId(),
            Name = data.GetProperty("name").GetString(),
        };
        await myListsDbContext.Projects.AddAsync(entity);
        await myListsDbContext.SaveChangesAsync();
        return entity.Id;
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

    private static ProjectDto EntityToDto(Project entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        IsDeleted = entity.IsDeleted
    };
}