using App.Database.Contexts;
using App.Models;
using Microsoft.EntityFrameworkCore;

namespace App.Database.Services;

public interface IUserService
{
    Task<User?> GetByEmail(string email);
}

public class UserService : IUserService
{
    private readonly UserContext _context;

    public UserService(UserContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmail(string email)
    {
        var item = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (item is null)
            return null;
        
        await _context.Entry(item).Reference(nameof(User.Credentials)).LoadAsync();

        return item;   
    }
}