using System.Security.Claims;
using KanbanApp.Backend.DTOs;
using KanbanApp.Backend.Models;
using KanbanApp.Backend.Services;
using KanbanApp.Backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace KanbanApp.Backend.Endpoints;

public static class BoardEndpoints
{
    public static void MapBoardEndpoints(this WebApplication app)
    {
        app.MapGet("/api/boards", async (IBoardService service, ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var boards = await service.GetAllByUserAsync(userId!);
            return Results.Ok(boards);
        }).RequireAuthorization();

        app.MapPost("/api/boards", async (CreateBoardDto dto, IBoardService boardService, ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var board = await boardService.CreateAsync(dto.BoardName, null, userId!);
            return TypedResults.Created($"/api/boards/{board.Id}", new { board.Id, board.Name, board.Description });
        }).RequireAuthorization();

        app.MapGet("/api/boards/{boardId}", async (
            int boardId, IBoardService boardService,
            IAuthorizationService authorizationService, ClaimsPrincipal user) =>
        {
            var authResult = await authorizationService.AuthorizeAsync(user, boardId, "IsBoardMember");
            if (!authResult.Succeeded) return Results.Forbid();

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var board = await boardService.GetByIdAsync(boardId, userId!);
            if (board == null) return Results.NotFound();

            var dto = new BoardDetailDto(
                board.Id, board.Name, board.Description, board.CreatedAt,
                board.Columns.Select(c => new ColumnDto(
                    c.Id, c.Name, c.Position,
                    c.Cards.Select(card => new CardDto(
                        card.Id, card.Title, card.Description, card.Position, card.CreatedAt
                    )).ToList()
                )).ToList()
            );
            return Results.Ok(dto);
        }).RequireAuthorization();

        app.MapPut("/api/boards/{boardId}", async (
            int boardId, UpdateBoardDto dto, IBoardService boardService,
            IAuthorizationService authorizationService, ClaimsPrincipal user) =>
        {
            var authResult = await authorizationService.AuthorizeAsync(user, boardId, "IsBoardOwner");
            if (!authResult.Succeeded) return Results.Forbid();

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var board = await boardService.UpdateAsync(boardId, userId!, dto.Name, null);
            return board is null ? Results.NotFound() : Results.Ok(new { board.Id, board.Name });
        }).RequireAuthorization();

        app.MapDelete("/api/boards/{boardId}", async (
            int boardId, IBoardService boardService,
            IAuthorizationService authorizationService, ClaimsPrincipal user) =>
        {
            var authResult = await authorizationService.AuthorizeAsync(user, boardId, "IsBoardOwner");
            if (!authResult.Succeeded) return Results.Forbid();

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var deleted = await boardService.DeleteAsync(boardId, userId!);
            return deleted ? Results.NoContent() : Results.NotFound();
        }).RequireAuthorization();

        app.MapPost("/api/boards/{boardId}/members", async (
            int boardId, string userId,
            IAuthorizationService authorizationService,
            ClaimsPrincipal user, ApplicationDbContext db) =>
        {
            var authResult = await authorizationService.AuthorizeAsync(user, boardId, "IsBoardOwner");
            if (!authResult.Succeeded) return Results.Forbid();

            var alreadyMember = await db.BoardMembers
                .AnyAsync(m => m.BoardId == boardId && m.UserId == userId);
            if (alreadyMember) return Results.Conflict("User is already a member of this board.");

            var member = new BoardMember { BoardId = boardId, UserId = userId, Role = BoardRole.Member };
            db.BoardMembers.Add(member);
            await db.SaveChangesAsync();
            return Results.Ok(member);
        }).RequireAuthorization();
    }
}