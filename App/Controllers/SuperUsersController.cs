using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TodoLists.App.Models;
using TodoLists.App.Utils;

namespace TodoLists.App.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "superuser")]
public class SuperUsersController : Controller
{
    private readonly TodoContext myContext;

    public SuperUsersController(TodoContext context)
    {
        myContext = context;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(SuperUserDto request)
    {
        var (passwordHash, passwordSalt) = PasswordHashUtils.CreatePasswordHash(request.Password);

        await myContext.SuperUsers.AddAsync(new SuperUser
        {
            Username = request.Username,
            UsernameLowerCase = request.Username.ToLower(),
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
        });
        try
        {
            await myContext.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            if (e.InnerException is PostgresException postgresException)
            {
                // duplicate key value violates unique constraint "ix_super_users_username_lower_case"
                if (postgresException.SqlState == "23505" &&
                    postgresException.ConstraintName == "ix_super_users_username_lower_case")
                    return UnprocessableEntity("Имя пользователя занято.");
            }

            throw;
        }

        return Ok();
    }
}