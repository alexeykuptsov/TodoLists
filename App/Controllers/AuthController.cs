using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TodoLists.App.Entities;
using TodoLists.App.Models;
using TodoLists.App.Services;
using TodoLists.App.Utils;

namespace TodoLists.App.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : Controller
{
    private readonly TodoListsDbContext myListsDbContext;
    private readonly IConfiguration myConfiguration;

    public AuthController(TodoListsDbContext listsDbContext, IConfiguration configuration)
    {
        myListsDbContext = listsDbContext;
        myConfiguration = configuration;
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<AuthResponse>> Login(UserDto request)
    {
        var user = await myListsDbContext.Users.Include(x => x.Profile).SingleOrDefaultAsync(
            x => x.Profile.Name == request.Profile && x.UsernameLowerCase == request.Username.ToLower());

        if (user == null ||
            !PasswordHashUtils.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            return Unauthorized();

        // Clean up old expired refresh tokens for this user
        await CleanupExpiredRefreshTokensAsync(user.Id, null);

        var accessToken = GenerateAccessToken(user);
        var refreshToken = await GenerateAndStoreRefreshTokenAsync(user.Id, null);

        return Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<AuthResponse>> LoginSuperUser(SuperUserDto request)
    {
        var superUser = await myListsDbContext.SuperUsers.SingleOrDefaultAsync(
            x => x.UsernameLowerCase == request.Username.ToLower());

        if (superUser == null ||
            !PasswordHashUtils.VerifyPasswordHash(request.Password, superUser.PasswordHash, superUser.PasswordSalt))
            return Unauthorized();

        // Clean up old expired refresh tokens for this super user
        await CleanupExpiredRefreshTokensAsync(null, superUser.Id);

        var accessToken = GenerateAccessToken(superUser);
        var refreshToken = await GenerateAndStoreRefreshTokenAsync(null, superUser.Id);
        
        return Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });
    }


    [HttpPost("[action]")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request)
    {
        try
        {
            var refreshToken = await ValidateRefreshTokenAsync(request.RefreshToken);
            if (refreshToken == null)
                return Unauthorized("Invalid or expired refresh token");

            // Generate new tokens
            string newAccessToken;
            if (refreshToken.UserId.HasValue)
            {
                var user = await myListsDbContext.Users.Include(x => x.Profile)
                    .FirstOrDefaultAsync(x => x.Id == refreshToken.UserId.Value);
                if (user == null)
                    return Unauthorized("User not found");
                newAccessToken = GenerateAccessToken(user);
            }
            else if (refreshToken.SuperUserId.HasValue)
            {
                var superUser = await myListsDbContext.SuperUsers
                    .FirstOrDefaultAsync(x => x.Id == refreshToken.SuperUserId.Value);
                if (superUser == null)
                    return Unauthorized("Super user not found");
                newAccessToken = GenerateAccessToken(superUser);
            }
            else
            {
                return Unauthorized("Invalid refresh token association");
            }

            // Revoke old refresh token
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;

            // Generate new refresh token
            var newRefreshToken = await GenerateAndStoreRefreshTokenAsync(refreshToken.UserId, refreshToken.SuperUserId);

            await myListsDbContext.SaveChangesAsync();

            return Ok(new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }
        catch (Exception)
        {
            return Unauthorized("Invalid refresh token");
        }
    }

    [HttpPost("[action]")]
    public async Task<ActionResult> Logout(RefreshRequest request)
    {
        try
        {
            var refreshToken = await myListsDbContext.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == request.RefreshToken && !x.IsRevoked);

            if (refreshToken != null)
            {
                refreshToken.IsRevoked = true;
                refreshToken.RevokedAt = DateTime.UtcNow;
                await myListsDbContext.SaveChangesAsync();
            }

            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception)
        {
            return BadRequest("Error during logout");
        }
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(UserService.ProfileIdClaimType, user.Profile.Id.ToString()),
        };
        return CreateToken(claims);
    }

    private string GenerateAccessToken(SuperUser superUser)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, superUser.Username),
            new(ClaimTypes.Role, "superuser"),
        };
        return CreateToken(claims);
    }

    private async Task<string> GenerateAndStoreRefreshTokenAsync(long? userId, long? superUserId)
    {
        var tokenString = GenerateRefreshToken();
        var refreshToken = new Entities.RefreshToken
        {
            Token = tokenString,
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(30),
            UserId = userId,
            SuperUserId = superUserId
        };

        myListsDbContext.RefreshTokens.Add(refreshToken);
        await myListsDbContext.SaveChangesAsync();

        return tokenString;
    }

    private async Task<Entities.RefreshToken?> ValidateRefreshTokenAsync(string token)
    {
        var refreshToken = await myListsDbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == token);

        if (refreshToken == null || refreshToken.IsRevoked || refreshToken.Expires < DateTime.UtcNow)
            return null;

        // Update last used timestamp
        refreshToken.LastUsedAt = DateTime.UtcNow;
        await myListsDbContext.SaveChangesAsync();

        return refreshToken;
    }

    private async Task CleanupExpiredRefreshTokensAsync(long? userId, long? superUserId)
    {
        var expiredTokens = await myListsDbContext.RefreshTokens
            .Where(x => (userId.HasValue && x.UserId == userId.Value) ||
                       (superUserId.HasValue && x.SuperUserId == superUserId.Value))
            .Where(x => x.Expires < DateTime.UtcNow || x.IsRevoked)
            .ToListAsync();

        if (expiredTokens.Any())
        {
            myListsDbContext.RefreshTokens.RemoveRange(expiredTokens);
            await myListsDbContext.SaveChangesAsync();
        }
    }

    private string CreateToken(List<Claim> claims)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(myConfiguration.GetSection("AppSettings:JwtKey").Value!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var token = new JwtSecurityToken(
            claims: claims, expires: DateTime.Now.AddMinutes(30), signingCredentials: credentials);
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return jwt;
    }

}