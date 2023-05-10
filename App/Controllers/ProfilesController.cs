using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using TodoLists.App.Models;

namespace TodoLists.App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "superuser")]
    public class ProfilesController : ControllerBase
    {
        private readonly TodoContext myContext;

        public ProfilesController(TodoContext context)
        {
            myContext = context;
        }

        // GET: api/Profiles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Profile>>> GetProfiles()
        {
            return await myContext.Profiles.ToListAsync();
        }

        [HttpGet("Count")]
        public async Task<ActionResult> GetProfilesCount()
        {
            return Ok(await myContext.Profiles.CountAsync());
        }

        // GET: api/Profiles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Profile>> GetProfile(long id)
        {
            var profile = await myContext.Profiles.FindAsync(id);

            if (profile == null)
            {
                return UnprocessableEntity("Профиль с указанным Id не найден.");
            }

            return profile;
        }

        // PUT: api/Profiles/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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
            myContext.Entry(entry).State = EntityState.Modified;

            try
            {
                await myContext.SaveChangesAsync();
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

        // POST: api/Profiles
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Profile>> PostProfile(Profile profile)
        {
            myContext.Profiles.Add(new Profile
            {
                Name = profile.Name,
                CreatedAt = SystemClock.Instance.GetCurrentInstant().ToUnixTimeMilliseconds(),
            });
            await myContext.SaveChangesAsync();

            return CreatedAtAction("GetProfile", new { id = profile.Id }, profile);
        }

        private bool ProfileExists(long id)
        {
            return (myContext.Profiles?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}