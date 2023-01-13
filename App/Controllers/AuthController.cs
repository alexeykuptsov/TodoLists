﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using TodoLists.App.Models;
using TodoLists.App.Services;
using TodoLists.App.Utils;

namespace TodoLists.App.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : Controller
{
    private readonly TodoContext myContext;
    private readonly IConfiguration myConfiguration;

    public AuthController(TodoContext context, IConfiguration configuration)
    {
        myContext = context;
        myConfiguration = configuration;
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<string>> Login(UserDto request)
    {
        var user = await myContext.Users.Include(x => x.Profile).SingleOrDefaultAsync(
            x => x.Profile.Name == request.Profile && x.UsernameLowerCase == request.Username.ToLower());

        if (user == null ||
            !PasswordHashUtils.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            return Unauthorized();
        var jwt = CreateToken(user);

        return Ok(jwt);
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<string>> LoginSuperUser(SuperUserDto request)
    {
        var superUser = await myContext.SuperUsers.SingleOrDefaultAsync(
            x => x.UsernameLowerCase == request.Username.ToLower());

        if (superUser == null || 
            !PasswordHashUtils.VerifyPasswordHash(request.Password, superUser.PasswordHash, superUser.PasswordSalt))
            return Unauthorized();
        var jwt = CreateToken(superUser);
        
        return Ok(jwt);
    }

    private string CreateToken(SuperUser superUser)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, superUser.Username),
            new(ClaimTypes.Role, "superuser"),
        };
        return CreateToken(claims);
    }

    private string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(UserService.ProfileClaimType, user.Profile.Name),
        };
        return CreateToken(claims);
    }

    private string CreateToken(List<Claim> claims)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(myConfiguration.GetSection("AppSettings:JwtKey").Value!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var token = new JwtSecurityToken(
            claims: claims, expires: DateTime.Now.AddHours(12), signingCredentials: credentials);
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return jwt;
    }
}