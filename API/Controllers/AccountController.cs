using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Interfaces;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(AppDbContext context, ITokenService tokenService) : BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDto)
    {
        if (await UserExists(registerDto.Email))
            return BadRequest("Email is already taken");

        using var hmac = new HMACSHA512();

        var user = new AppUser
        {
            DisplayName = registerDto.DisplayName,
            Email = registerDto.Email,
            PasswordHash = hmac.ComputeHash(
                Encoding.UTF8.GetBytes(registerDto.Password)
            ),
            PasswordSalt = hmac.Key
        };

        context.Users.Add(user);

        await context.SaveChangesAsync();

        return user.ToDto(tokenService);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDto)
    {
        var user = await context.Users
            .SingleOrDefaultAsync(x => x.Email == loginDto.Email);

        if (user == null)
            return Unauthorized("Invalid email");

        using var hmac = new HMACSHA512(user.PasswordSalt);

        var computedHash = hmac.ComputeHash(
            Encoding.UTF8.GetBytes(loginDto.Password)
        );

        for (var i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i])
                return Unauthorized("Invalid password");
        }

        return user.ToDto(tokenService);
    }

    private async Task<bool> UserExists(string email)
    {
        return await context.Users.AnyAsync(x => x.Email == email);
    }
}

public class LoginDTO
{
    public string Email { get; internal set; }
    public char[] Password { get; internal set; }
}