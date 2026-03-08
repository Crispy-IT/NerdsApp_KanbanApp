using Microsoft.AspNetCore.Authorization;

namespace KanbanApp.Authorization;

public class IsBoardOwnerRequirement : IAuthorizationRequirement
{
}