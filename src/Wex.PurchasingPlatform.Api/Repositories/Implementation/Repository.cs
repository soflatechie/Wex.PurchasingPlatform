using Microsoft.EntityFrameworkCore;
using Wex.PurchasingPlatform.Api.Data;
using Wex.PurchasingPlatform.Api.Repositories.Interfaces;

namespace Wex.PurchasingPlatform.Api.Repositories.Implementation;

public class Repository<TEntity>(AppDbContext context) : IRepository<TEntity> where TEntity : class
{
    protected AppDbContext Context { get; } = context;

    public async Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<TEntity>().FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<TEntity>().ToListAsync(cancellationToken);
    }

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Context.Set<TEntity>().Add(entity);
        await Context.SaveChangesAsync(cancellationToken);

        return entity;
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Context.Set<TEntity>().Update(entity);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(object id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);

        if (entity is null)
            return false;

        Context.Set<TEntity>().Remove(entity);
        await Context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
