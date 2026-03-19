using System.Security.Claims;
using KanbanApp.Backend.Data;
using KanbanApp.Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace KanbanApp.Backend.Authorization;

public class IsBoardOwnerHandler : AuthorizationHandler<IsBoardOwnerRequirement, int>
{
    private readonly ApplicationDbContext _db;

    public IsBoardOwnerHandler(ApplicationDbContext db)
    {
        _db = db;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        IsBoardOwnerRequirement requirement,
        int boardId)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var isOwner = await _db.BoardMembers
            .AnyAsync(m => m.BoardId == boardId 
                           && m.UserId == userId 
                           && m.Role == BoardRole.Owner);

        if (isOwner) context.Succeed(requirement);
    }
}
