using System.Security.Claims;
using KanbanApp.Backend.Services;

namespace KanbanApp.Backend.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapGet("/api/users/me", async (ClaimsPrincipal user, IUserService userService) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await userService.GetUserProfileAsync(userId!);
            if (profile == null) return Results.NotFound();
            return Results.Ok(profile);
        }).RequireAuthorization();
    }
}