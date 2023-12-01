namespace App.Database.Repositories;

public interface IRepository<TEntity>
{
    Task<Guid> Create(TEntity item);

    Task<TEntity?> Get<T>(T id);
    
    Task<TEntity> Update(TEntity updated);

    Task Delete(Guid uuid);
}