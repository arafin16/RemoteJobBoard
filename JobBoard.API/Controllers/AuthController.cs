using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JobBoard.API.Data;
using JobBoard.API.DTOs;
using JobBoard.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace JobBoard.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    // ১. REGISTER ENDPOINT
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        try
        {
            // ইমেইল অলরেডি আছে কিনা চেক করা
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest(new { message = "Email is already registered." });
            }

            // পাসওয়ার্ড হ্যাশ করা
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = passwordHash,
                Role = dto.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration successful!" });
        }
        catch (Exception ex)
        {
            // 🚨 এটি ক্র্যাশ আটকাতে সাহায্য করবে এবং আসল ডাটাবেজ এররটি CORS হেডারসহ ফ্রন্টএন্ডে পাঠাবে
            return StatusCode(500, new 
            { 
                message = "Database or Internal Server Error occurred.", 
                error = ex.Message, 
                innerError = ex.InnerException?.Message 
            });
        }
    }

    // ২. LOGIN ENDPOINT
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            // JWT Token তৈরি করা
            var token = GenerateJwtToken(user);

            return Ok(new { 
                token = token, 
                user = new { user.Id, user.FullName, user.Email, user.Role } 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new 
            { 
                message = "Login failed due to server error.", 
                error = ex.Message,
                innerError = ex.InnerException?.Message
            });
        }
    }

    // JWT Token জেনারেশন মেথড
    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}