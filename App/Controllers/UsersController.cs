using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TodoLists.App.Entities;
using TodoLists.App.Models;
using TodoLists.App.Utils;

namespace TodoLists.App.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "superuser")]
public class UsersController : Controller
{
    private readonly TodoListsDbContext myListsDbContext;

    public UsersController(TodoListsDbContext listsDbContext)
    {
        myListsDbContext = listsDbContext;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(UserDto request)
    {
        var profile = myListsDbContext.Profiles.Single(x => x.Name == request.Profile);
        var (passwordHash, passwordSalt) = PasswordHashUtils.CreatePasswordHash(request.Password);

        await myListsDbContext.Users.AddAsync(new User
        {
            Profile = profile,
            Username = request.Username,
            UsernameLowerCase = request.Username.ToLower(),
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
        });
        try
        {
            await myListsDbContext.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            if (e.InnerException is PostgresException postgresException)
            {
                // duplicate key value violates unique constraint "ix_users_username_lower_case"
                if (postgresException.SqlState == "23505" &&
                    postgresException.ConstraintName == "ix_users_username_lower_case")
                    return UnprocessableEntity("Имя пользователя занято.");
            }

            throw;
        }

        return Ok();
    }
}