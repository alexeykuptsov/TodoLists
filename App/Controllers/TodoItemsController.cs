using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoLists.App.Models;
using TodoLists.App.Services;

namespace TodoLists.App.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TodoItemsController : ControllerBase
{
    private readonly TodoContext myContext;
    private readonly IUserService myUserService;

    public TodoItemsController(TodoContext context, IUserService userService)
    {
        myContext = context;
        myUserService = userService;
    }

    // GET: api/TodoItems
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItemDto>>> GetTodoItems()
    {
        return await myContext.TodoItems
            .Where(x => x.ProfileId == myUserService.GetCurrentUserProfileId())
            .Select(x => ItemToDto(x))
            .ToListAsync();
    }

    // GET: api/TodoItems/5
    // <snippet_GetByID>
    [HttpGet("{id}")]
    public async Task<ActionResult<TodoItemDto>> GetTodoItem(long id)
    {
        var todoItem = await myContext.TodoItems.FindAsync(id);

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

        var todoItem = await myContext.TodoItems.FindAsync(id);
        if (todoItem == null)
        {
            return NotFound();
        }

        todoItem.Name = todoDto.Name;
        todoItem.IsComplete = todoDto.IsComplete;

        try
        {
            await myContext.SaveChangesAsync();
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
    public async Task<ActionResult<TodoItemDto>> PostTodoItem(TodoItemDto todoDto)
    {
        var todoItem = new TodoItem
        {
            Profile = await myContext.Profiles.SingleAsync(x => x.Id == myUserService.GetCurrentUserProfileId()),
            IsComplete = todoDto.IsComplete,
            Name = todoDto.Name,
        };

        myContext.TodoItems.Add(todoItem);
        await myContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, ItemToDto(todoItem));
    }
    // </snippet_Create>

    // DELETE: api/TodoItems/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTodoItem(long id)
    {
        var todoItem = await myContext.TodoItems.FindAsync(id);
        if (todoItem == null)
        {
            return NotFound();
        }

        myContext.TodoItems.Remove(todoItem);
        await myContext.SaveChangesAsync();

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
                    await myContext.TodoItems.Where(x => x.Id == id).
                        ExecuteUpdateAsync(x => x.SetProperty(t => t.IsComplete, propertyValue));
                }
                if (propertyName == "title")
                {
                    string propertyValue = property.Value.GetString();
                    await myContext.TodoItems.Where(x => x.Id == id).
                        ExecuteUpdateAsync(x => x.SetProperty(t => t.Name, propertyValue));
                }
            }
        }
        return Ok();
    }

    private bool TodoItemExists(long id)
    {
        return myContext.TodoItems.Any(e => e.Id == id);
    }

    private static TodoItemDto ItemToDto(TodoItem todoItem) =>
       new TodoItemDto
       {
           Id = todoItem.Id,
           Name = todoItem.Name,
           IsComplete = todoItem.IsComplete
       };
}