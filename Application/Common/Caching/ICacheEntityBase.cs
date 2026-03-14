namespace Backend.Application.Common.Caching;

public interface ICacheEntityBase<TEntity, TId>
{
    Task<IReadOnlyList<TEntity>> GetOrCreateAllAsync(Func<CancellationToken, Task<IReadOnlyList<TEntity>>> factory, CancellationToken ct);
    Task<TEntity?> GetOrCreateByIdAsync(TId id, Func<CancellationToken, Task<TEntity?>> factory, CancellationToken ct);
    Task<TEntity?> GetOrCreateByPropertyNameAsync(string cachedPropertyName, string cachedPropertyValue, Func<CancellationToken, Task<TEntity?>> factory, CancellationToken ct);
    void ResetEntity(TEntity entity);
    void SetEntity(TEntity entity);
}
