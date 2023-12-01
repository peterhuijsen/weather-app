using App.Database.Contexts;
using App.Database.Repositories.Generics;
using App.Models;
using Microsoft.EntityFrameworkCore;

namespace App.Database.Repositories;

public class UserRepository : EntityFrameworkRepository<User, UserContext>, IRepository<User>
{
    public UserRepository(UserContext context) : base(context) { }

    public Task<User?> Get<T>(T id)
        => Get(
            id, 
            entry => entry.Reference(u => u.Credentials)
                .LoadAsync(), 
            entry => entry.Reference(u => u.Credentials)
                .Query()
                .Include(c => c.Passkeys)
                .Include(c => c.OTP)
                .Include(c => c.OTP)
                    .ThenInclude(o => o.HashCodes)
                .Include(c => c.OTP)
                    .ThenInclude(o => o.TimeCodes)
                .LoadAsync()
        );
}