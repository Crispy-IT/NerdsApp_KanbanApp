namespace KanbanApp.Backend.DTOs;
public record CreateCardDto(string Title, string? Description, int ColumnId);
