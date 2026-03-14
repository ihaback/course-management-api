using Microsoft.EntityFrameworkCore;
using Backend.Domain.Common.Base;

namespace Backend.Infrastructure.Persistence.EFC.Repositories;

public abstract class RepositoryBase<TModel, TId, TEntity, TDbContext>(TDbContext context)
    : IRepositoryBase<TModel, TId>
    where TEntity : class
    where TDbContext : DbContext
{
    protected readonly TDbContext _context = context;
    protected DbSet<TEntity> Set => _context.Set<TEntity>();

    protected abstract TEntity ToEntity(TModel model);
    protected abstract TModel ToModel(TEntity entity);

    public virtual async Task<TModel> AddAsync(TModel model, CancellationToken ct)
    {
        var entity = ToEntity(model);
        await Set.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        return ToModel(entity);
    }

    public virtual async Task<TModel?> GetByIdAsync(TId id, CancellationToken ct)
    {
        var entity = await Set.FindAsync([id], ct);
        return entity is null ? default : ToModel(entity);
    }

    public virtual async Task<IReadOnlyList<TModel>> GetAllAsync(CancellationToken ct)
    {
        var entities = await Set.AsNoTracking().ToListAsync(ct);
        return [.. entities.Select(ToModel)];
    }

    public virtual async Task<TModel?> UpdateAsync(TId id, TModel model, CancellationToken ct)
    {
        var entity = await Set.FindAsync([id], ct);
        if (entity is null)
            return default;

        var updated = ToEntity(model);
        _context.Entry(entity).CurrentValues.SetValues(updated);
        await _context.SaveChangesAsync(ct);

        return ToModel(entity);
    }

    public virtual async Task<bool> RemoveAsync(TId id, CancellationToken ct)
    {
        var entity = await Set.FindAsync([id], ct);
        if (entity is null)
            return false;

        Set.Remove(entity);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
