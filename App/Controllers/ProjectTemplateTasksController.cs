using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoLists.App.Entities;
using TodoLists.App.Models;
using TodoLists.App.Services;

namespace TodoLists.App.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProjectTemplateTasksController : ControllerBase
{
    private readonly TodoListsDbContext myDbContext;
    private readonly long myProfileId;

    public ProjectTemplateTasksController(TodoListsDbContext dbContext, IUserService userService)
    {
        myDbContext = dbContext;
        myProfileId = userService.GetCurrentUserProfileId();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectTemplateTaskDto>>> GetItems(
        [FromQuery] long projectTemplateId) =>
        await myDbContext.ProjectTemplateTasks
            .Where(i => i.ProfileId == myProfileId && i.ProjectTemplateId == projectTemplateId)
            .OrderBy(i => i.Id)
            .Select(i => new ProjectTemplateTaskDto { Id = i.Id, ProjectTemplateId = i.ProjectTemplateId, Name = i.Name })
            .ToListAsync();

    [HttpPost]
    public async Task<IActionResult> PostItem(ProjectTemplateTaskDto dto)
    {
        var templateExists = await myDbContext.ProjectTemplates
            .AnyAsync(t => t.Id == dto.ProjectTemplateId && t.ProfileId == myProfileId);
        if (!templateExists)
            return BadRequest("Template not found.");

        var item = new ProjectTemplateTask
        {
            ProfileId = myProfileId,
            ProjectTemplateId = dto.ProjectTemplateId,
            Name = dto.Name,
        };
        myDbContext.ProjectTemplateTasks.Add(item);
        await myDbContext.SaveChangesAsync();
        return Ok(item.Id);
    }

    [HttpPatch]
    public async Task<IActionResult> PatchItem([FromBody] dynamic body)
    {
        foreach (dynamic change in body.EnumerateArray())
        {
            string? type = null;
            try { type = change.GetProperty("type").GetString(); } catch { /* no type field */ }

            long id = change.GetProperty("key").GetInt64();

            if (type == "remove")
            {
                var item = await myDbContext.ProjectTemplateTasks
                    .Where(i => i.Id == id && i.ProfileId == myProfileId)
                    .FirstOrDefaultAsync();
                if (item != null)
                {
                    myDbContext.ProjectTemplateTasks.Remove(item);
                    await myDbContext.SaveChangesAsync();
                }
            }
            else
            {
                foreach (dynamic property in change.GetProperty("data").EnumerateObject())
                {
                    string propertyName = property.Name;
                    if (propertyName == "name")
                    {
                        string propertyValue = property.Value.GetString();
                        await myDbContext.ProjectTemplateTasks
                            .Where(i => i.Id == id && i.ProfileId == myProfileId)
                            .ExecuteUpdateAsync(x => x.SetProperty(t => t.Name, propertyValue));
                    }
                }
            }
        }
        return Ok();
    }
}
