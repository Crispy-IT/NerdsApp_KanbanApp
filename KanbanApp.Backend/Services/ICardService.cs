namespace KanbanApp.Backend.Services;
using Models;

public interface ICardService
{
    Task<Card?> CreateAsync(int boardId, int columnId, string title, string? description);
    Task<Card?> UpdateAsync(int boardId, int cardId, string title, string? description, int columnId, string? assignedToUserId);
    Task<bool> DeleteAsync(int boardId, int cardId);
    Task<Card?> AssignCardAsync(int cardId, string userId);
}