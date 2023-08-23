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
public class TodoItemsController : ControllerBase
{
    private readonly TodoListsDbContext myListsDbContext;
    private readonly IUserService myUserService;

    public TodoItemsController(TodoListsDbContext listsDbContext, IUserService userService)
    {
        myListsDbContext = listsDbContext;
        myUserService = userService;
    }

    // GET: api/TodoItems
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItemDto>>> GetTodoItems()
    {
        return await myListsDbContext.TodoItems
            .Where(x => x.ProfileId == myUserService.GetCurrentUserProfileId())
            .Select(x => ItemToDto(x))
            .ToListAsync();
    }

    // GET: api/TodoItems/5
    // <snippet_GetByID>
    [HttpGet("{id}")]
    public async Task<ActionResult<TodoItemDto>> GetTodoItem(long id)
    {
        var todoItem = await myListsDbContext.TodoItems.FindAsync(id);

        if (todoItem == null)
        {
            return NotFound();
        }

        return ItemToDto(todoItem);
    }
    // </snippet_GetByID>

    // PUT: api/TodoItems/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    // <snippet_Update>
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTodoItem(long id, TodoItemDto todoDto)
    {
        if (id != todoDto.Id)
        {
            return BadRequest();
        }

        var todoItem = await myListsDbContext.TodoItems.FindAsync(id);
        if (todoItem == null)
        {
            return NotFound();
        }

        todoItem.Name = todoDto.Name;
        todoItem.IsComplete = todoDto.IsComplete;

        try
        {
            await myListsDbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException) when (!TodoItemExists(id))
        {
            return NotFound();
        }

        return NoContent();
    }
    // </snippet_Update>

    // POST: api/TodoItems
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    // <snippet_Create>
    [HttpPost]
    public async Task<IActionResult> PostTodoItem(TodoItemDto todoDto)
    {
        var todoItem = new TodoItem
        {
            Profile = await myListsDbContext.Profiles.SingleAsync(x => x.Id == myUserService.GetCurrentUserProfileId()),
            IsComplete = todoDto.IsComplete,
            Name = todoDto.Name,
        };

        myListsDbContext.TodoItems.Add(todoItem);
        await myListsDbContext.SaveChangesAsync();

        return Ok();
    }
    // </snippet_Create>

    // DELETE: api/TodoItems/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTodoItem(long id)
    {
        var todoItem = await myListsDbContext.TodoItems.FindAsync(id);
        if (todoItem == null)
        {
            return NotFound();
        }

        myListsDbContext.TodoItems.Remove(todoItem);
        await myListsDbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpPatch]
    public async Task<IActionResult> PatchTodoItem([FromBody] dynamic body)
    {
        foreach (dynamic change in body.EnumerateArray())
        {
            long id = change.GetProperty("key").GetProperty("id").GetInt64();
            foreach (dynamic property in change.GetProperty("data").EnumerateObject())
            {
                string propertyName = property.Name;
                if (propertyName == "isComplete")
                {
                    bool propertyValue = property.Value.GetBoolean();
                    await myListsDbContext.TodoItems.Where(x => x.Id == id).
                        ExecuteUpdateAsync(x => x.SetProperty(t => t.IsComplete, propertyValue));
                }
                if (propertyName == "name")
                {
                    string propertyValue = property.Value.GetString();
                    await myListsDbContext.TodoItems.Where(x => x.Id == id).
                        ExecuteUpdateAsync(x => x.SetProperty(t => t.Name, propertyValue));
                }
            }
        }
        return Ok();
    }

    private bool TodoItemExists(long id)
    {
        return myListsDbContext.TodoItems.Any(e => e.Id == id);
    }

    private static TodoItemDto ItemToDto(TodoItem todoItem) =>
       new TodoItemDto
       {
           Id = todoItem.Id,
           Name = todoItem.Name,
           IsComplete = todoItem.IsComplete
       };
}