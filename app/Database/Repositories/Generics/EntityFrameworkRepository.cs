using System;
using System.Threading.Tasks;
using App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace App.Database.Repositories.Generics;

public class EntityFrameworkRepository<TEntity, TContext>
    where TEntity : class, IEntity
    where TContext : DbContext
{
    protected readonly TContext _context;

    public EntityFrameworkRepository(TContext context)
        => _context = context;
    
    public virtual async Task<Guid> Create(TEntity item)
    {
        item.Uuid = Guid.NewGuid();
        _context.Add(item);

        await _context.SaveChangesAsync();

        return item.Uuid;
    }

    public virtual async Task<TEntity?> Get<T>(T id, params Func<EntityEntry<TEntity>, Task>[]? include)
    {
        var item = await _context.FindAsync<TEntity>(id);
        if (include is null || item is null) 
            return item;

        foreach (var action in include)
        {
            var entry = _context.Entry(item);
            await action(entry);
        }

        return item;
    }

    public virtual async Task<TEntity> Update(TEntity updated)
    {
        _context.Entry(updated).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return updated;
    }

    public virtual async Task Delete(Guid uuid)
    {
        var item = await _context.FindAsync<TEntity>(uuid);
        if (item is null)
            return;
        
        _context.Remove(item);
        await _context.SaveChangesAsync();
    }
}