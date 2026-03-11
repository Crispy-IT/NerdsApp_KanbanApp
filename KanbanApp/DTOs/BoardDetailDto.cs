namespace KanbanApp.DTOs;

public record BoardDetailDto(
    int Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    List<ColumnDto> Columns
);

public record ColumnDto(
    int Id,
    string Name,
    int Position,
    List<CardDto> Cards
);

public record CardDto(
    int Id,
    string Title,
    string? Description,
    int Position,
    DateTime CreatedAt
);