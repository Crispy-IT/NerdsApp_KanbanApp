using System.Security.Claims;
using KanbanApp.Backend.DTOs;
using KanbanApp.Backend.Models;
using KanbanApp.Backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace KanbanApp.Backend.Endpoints;

public static class ColumnEndpoints
{
    public static void MapColumnEndpoints(this WebApplication app)
    {
        var columns = app.MapGroup("/api/boards/{boardId}/columns")
            .RequireAuthorization();

        columns.MapPost("/", async (int boardId, CreateColumnDto dto, ApplicationDbContext db,
            IAuthorizationService authorizationService, ClaimsPrincipal user) =>
        {
            var authResult = await authorizationService.AuthorizeAsync(user, boardId, "IsBoardMember");
            if (!authResult.Succeeded) return Results.Forbid();

            var position = dto.Position ?? await db.Columns.Where(c => c.BoardId == boardId).CountAsync();
            var column = new Column
            {
                Name = dto.Name,
                Position = position,
                BoardId = boardId,
                Color = dto.Color ?? "#00d4ff"
            };
            db.Columns.Add(column);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/boards/{boardId}/columns/{column.Id}",
                new { column.Id, column.Name, column.Position, column.Color });
        });

        columns.MapPut("/{columnId}", async (int boardId, int columnId, UpdateColumnDto dto, ApplicationDbContext db,
            IAuthorizationService authorizationService, ClaimsPrincipal user) =>
        {
            var authResult = await authorizationService.AuthorizeAsync(user, boardId, "IsBoardMember");
            if (!authResult.Succeeded) return Results.Forbid();

            var column = await db.Columns.FirstOrDefaultAsync(c => c.Id == columnId && c.BoardId == boardId);
            if (column == null) return Results.NotFound();
            column.Name = dto.Name;
            if (dto.Color != null) column.Color = dto.Color;
            await db.SaveChangesAsync();
            return Results.Ok(new { column.Id, column.Name, column.Position, column.Color });
        });

        columns.MapDelete("/{columnId}/cards", async (int boardId, int columnId, ApplicationDbContext db,
            IAuthorizationService authorizationService, ClaimsPrincipal user) =>
        {
            var authResult = await authorizationService.AuthorizeAsync(user, boardId, "IsBoardMember");
            if (!authResult.Succeeded) return Results.Forbid();

            var cards = await db.Cards.Where(c => c.ColumnId == columnId).ToListAsync();
            db.Cards.RemoveRange(cards);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        columns.MapDelete("/{columnId}", async (int boardId, int columnId, ApplicationDbContext db,
            IAuthorizationService authorizationService, ClaimsPrincipal user) =>
        {
            var authResult = await authorizationService.AuthorizeAsync(user, boardId, "IsBoardMember");
            if (!authResult.Succeeded) return Results.Forbid();

            var column = await db.Columns.Include(c => c.Cards)
                .FirstOrDefaultAsync(c => c.Id == columnId && c.BoardId == boardId);
            if (column == null) return Results.NotFound();
            if (column.Cards.Any()) return Results.BadRequest("Cannot delete column with existing cards.");
            db.Columns.Remove(column);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}