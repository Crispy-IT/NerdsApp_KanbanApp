namespace KanbanApp.Backend.Services;

using Data;
using Microsoft.EntityFrameworkCore;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;
        return new UserProfileDto(user.Id, user.UserName, user.Email, user.Bio, user.ProfilePictureUrl);
    }

    public async Task<UserProfileDto?> UpdateUserProfileAsync(string userId, string? bio, string? profilePictureUrl)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;
        user.Bio = bio;
        if (profilePictureUrl != null) user.ProfilePictureUrl = profilePictureUrl;
        await _context.SaveChangesAsync();
        return new UserProfileDto(user.Id, user.UserName, user.Email, user.Bio, user.ProfilePictureUrl);
    }
}