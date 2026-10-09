using System.Text.Json;
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
public class ProjectTemplatesController : ControllerBase
{
    private readonly TodoListsDbContext myDbContext;
    private readonly long myProfileId;

    public ProjectTemplatesController(TodoListsDbContext dbContext, IUserService userService)
    {
        myDbContext = dbContext;
        myProfileId = userService.GetCurrentUserProfileId();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectTemplateDto>>> GetTemplates() =>
        await myDbContext.ProjectTemplates
            .Where(t => t.ProfileId == myProfileId)
            .OrderBy(t => t.Id)
            .Select(t => new ProjectTemplateDto { Id = t.Id, Name = t.Name })
            .ToListAsync();

    [HttpPost]
    public async Task<IActionResult> PostTemplate(ProjectTemplateDto dto)
    {
        var template = new ProjectTemplate
        {
            ProfileId = myProfileId,
            Name = dto.Name,
        };
        myDbContext.ProjectTemplates.Add(template);
        await myDbContext.SaveChangesAsync();
        return Ok(template.Id);
    }

    [HttpPatch]
    public async Task<IActionResult> PatchTemplate([FromBody] JsonElement body)
    {
        return await WithExceptionHandling(async () =>
        {
            foreach (var change in body.EnumerateArray())
            {
                var changeType = change.GetProperty("type").GetString();
                switch (changeType)
                {
                    case "update":
                        var id = change.GetProperty("data").GetProperty("id").GetInt64();
                        var name = change.GetProperty("data").GetProperty("name").GetString();
                        await myDbContext.ProjectTemplates
                            .Where(t => t.Id == id && t.ProfileId == myProfileId)
                            .ExecuteUpdateAsync(x => x.SetProperty(t => t.Name, name));
                        break;
                    case "remove":
                        id = change.GetProperty("key").GetInt64();
                        var template = await myDbContext.ProjectTemplates
                            .Where(t => t.Id == id && t.ProfileId == myProfileId)
                            .FirstOrDefaultAsync();
                        if (template != null)
                        {
                            myDbContext.ProjectTemplates.Remove(template);
                            await myDbContext.SaveChangesAsync();
                        }
                        break;
                }
            }
            return (IActionResult)Ok();
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
}
