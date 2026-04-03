namespace KanbanApp.Backend.DTOs;

public record UpdateCardDto(string Title, string? Description, int ColumnId, string? AssignedToUserId);