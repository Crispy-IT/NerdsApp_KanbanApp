using KanbanApp.Backend.Models;

namespace KanbanApp.Backend.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapIdentityApi<ApplicationUser>();
    }
}