namespace KanbanApp.Backend.Services;

public interface IUserService
{
    Task<UserProfileDto?> GetUserProfileAsync(string userId);
    Task<UserProfileDto?> UpdateUserProfileAsync(string userId, string? bio, string? profilePictureUrl);
}

public record UserProfileDto(string Id, string? UserName, string? Email, string? Bio, string? ProfilePictureUrl);