using Microsoft.EntityFrameworkCore;
using KanbanApp.Data;
using KanbanApp.Models;
using KanbanApp.Services;
using System.Security.Claims;
using KanbanApp.DTOs;
using KanbanApp.Authorization;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IBoardService, BoardService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("IsBoardOwner", policy =>
        policy.Requirements.Add(new IsBoardOwnerRequirement()));
});

builder.Services.AddScoped<IAuthorizationHandler, IsBoardOwnerHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapIdentityApi<ApplicationUser>();

app.MapGet("/api/boards", async (IBoardService service) =>
{
    var boards = await service.GetAllByUserAsync("test-user-id");
    return Results.Ok(boards);
});

app.MapGet("/api/users/me", async (ClaimsPrincipal user, IUserService userService) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    var profile = await userService.GetUserProfileAsync(userId!);
    if (profile == null) return Results.NotFound();
    return Results.Ok(profile);
}).RequireAuthorization();

app.MapPost("/api/boards", async (CreateBoardDto dto, IBoardService boardService, ClaimsPrincipal user) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    var board = await boardService.CreateAsync(dto.BoardName, null, userId!);
    return TypedResults.Created($"/api/boards/{board.Id}", new { board.Id, board.Name, board.Description });
    
}).RequireAuthorization();

app.MapPost("/api/boards/{boardId}/members", async (
    int boardId,
    string userId,
    IBoardService boardService,
    IAuthorizationService authorizationService,
    ClaimsPrincipal user,
    ApplicationDbContext db) =>
{
    var authResult = await authorizationService.AuthorizeAsync(user, boardId, "IsBoardOwner");
    if (!authResult.Succeeded) return Results.Forbid();

    var member = new BoardMember { BoardId = boardId, UserId = userId, Role = BoardRole.Member };
    db.BoardMembers.Add(member);
    await db.SaveChangesAsync();

    return Results.Ok(member);
}).RequireAuthorization();

app.Run();

public partial class Program { }

