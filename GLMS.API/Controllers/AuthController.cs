using GLMS.API.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GLMS.API.Controllers
{
    // Code Attribution
    // Title: JWT authentication in ASP.NET Core
    // Author: Microsoft
    // Date: 2026
    // Availability: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        // POST api/auth/login — returns JWT token
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            // Hardcoded demo credentials — in production use Identity/database
            if (dto.Username != "admin" || dto.Password != "password123")
                return Unauthorized(new { message = "Invalid credentials" });

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, dto.Username),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiry = token.ValidTo
            });
        }
    }
}