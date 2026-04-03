using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KanbanApp.Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace KanbanApp.Backend.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/register", async ([FromBody] RegisterRequest request, UserManager<ApplicationUser> userManager) =>
        {
            if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return Results.BadRequest(new { message = "All fields are required." });

            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email
            };

            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return Results.BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });

            return Results.Ok(new { message = "User created successfully." });
        });

        app.MapPost("/login", async ([FromBody] LoginRequest request,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Results.Unauthorized();

            var result = await signInManager.PasswordSignInAsync(user.UserName!, request.Password, false, false);
            if (!result.Succeeded)
                return Results.Unauthorized();

            var key = configuration["Jwt:Key"] ?? "super-secret-key-that-is-at-least-32-chars-long";
            var token = GenerateJwtToken(user, key);

            return Results.Ok(new { accessToken = token });
        });
    }

    private static string GenerateJwtToken(ApplicationUser user, string key)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenKey = Encoding.UTF8.GetBytes(key);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? "")
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(tokenKey),
                SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

public record RegisterRequest(string UserName, string Email, string Password);
public record LoginRequest(string Email, string Password);