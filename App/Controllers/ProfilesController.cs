using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using TodoLists.App.Entities;

namespace TodoLists.App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "superuser")]
    public class ProfilesController : ControllerBase
    {
        private readonly TodoListsDbContext myListsDbContext;

        public ProfilesController(TodoListsDbContext listsDbContext)
        {
            myListsDbContext = listsDbContext;
        }

        // GET: api/Profiles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Profile>>> GetProfiles()
        {
            return await myListsDbContext.Profiles.ToListAsync();
        }

        [HttpGet("MaxId")]
        public async Task<ActionResult> GetMaxId()
        {
            return Ok(await myListsDbContext.Profiles.MaxAsync(x => x.Id));
        }

        // GET: api/Profiles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Profile>> GetProfile(long id)
        {
            var profile = await myListsDbContext.Profiles.FindAsync(id);

            if (profile == null)
            {
                return UnprocessableEntity("Профиль с указанным Id не найден.");
            }

            return profile;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProfile(long id, Profile profile)
        {
            if (id != profile.Id)
            {
                return UnprocessableEntity("Не указан Id.");
            }

            var entry = new Profile
            {
                Id = profile.Id,
                Name = profile.Name,
            };
            myListsDbContext.Entry(entry).State = EntityState.Modified;

            try
            {
                await myListsDbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfileExists(id))
                {
                    return UnprocessableEntity("Профиль с указанным Id не найден.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Profile>> PostProfile(Profile profile)
        {
            var profileEntity = new Profile
            {
                Name = profile.Name,
                CreatedAt = SystemClock.Instance.GetCurrentInstant().ToUnixTimeMilliseconds(),
            };
            myListsDbContext.Profiles.Add(profileEntity);

            var project = new Project
            {
                Name = "Inbox",
                Profile = profileEntity,
            };
            myListsDbContext.Projects.Add(project);

            await myListsDbContext.SaveChangesAsync();

            return CreatedAtAction("GetProfile", new { id = profile.Id }, profile);
        }

        private bool ProfileExists(long id)
        {
            return myListsDbContext.Profiles.Any(e => e.Id == id);
        }
    }
}