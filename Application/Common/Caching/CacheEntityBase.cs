using Backend.Application.Extensions.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace Backend.Application.Common.Caching;

public abstract class CacheEntityBase<TEntity, TId>(IMemoryCache cache) : ICacheEntityBase<TEntity, TId>
{
    protected virtual string Prefix => $"{nameof(TEntity).ToLowerInvariant()}";
    protected abstract TId GetId(TEntity entity);
    protected virtual IEnumerable<(string PropertyName, string Value)> GetCachedProperties(TEntity entity) => [];
    protected virtual string NormalizeCachedPropertyValue(string value) => value.Trim().ToLowerInvariant();

    protected virtual MemoryCacheEntryOptions EntityOptions => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
        SlidingExpiration = TimeSpan.FromMinutes(2),
    };

    protected virtual MemoryCacheEntryOptions ListOptions => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30),
        SlidingExpiration = TimeSpan.FromSeconds(10),
    };

    protected string IdKey(TId id) => $"{Prefix}:id:{id}";
    protected string IndexKey(string cachedPropertyName, string cachedPropertyValue) => $"{Prefix}:{cachedPropertyName}:{cachedPropertyValue}";
    protected string AllKey => $"{Prefix}:all";

    public virtual void SetEntity(TEntity entity)
    {
        cache.Set(IdKey(GetId(entity)), entity, EntityOptions);

        foreach (var (propertyName, value) in GetCachedProperties(entity))
        {
            if (string.IsNullOrWhiteSpace(value))
                continue;

            var normalizedValue = NormalizeCachedPropertyValue(value);
            cache.Set(IndexKey(propertyName, normalizedValue), entity, EntityOptions);
        }
    }

    public virtual void ResetEntity(TEntity entity)
    {
        cache.Remove(IdKey(GetId(entity)));

        foreach (var (propertyName, value) in GetCachedProperties(entity))
        {
            if (string.IsNullOrWhiteSpace(value))
                continue;

            var normalizedValue = NormalizeCachedPropertyValue(value);
            cache.Remove(IndexKey(propertyName, normalizedValue));
        }

        cache.Remove(AllKey);
    }

    public virtual Task<TEntity?> GetOrCreateByIdAsync(TId id, Func<CancellationToken, Task<TEntity?>> factory, CancellationToken ct)
        => cache.GetOrCreateAsync(
            IdKey(id),
            async (entry, token) =>
            {
                entry.SetOptions(EntityOptions);
                return await factory(token);
            },
            ct);

    public virtual Task<TEntity?> GetOrCreateByPropertyNameAsync(string cachedPropertyName, string cachedPropertyValue, Func<CancellationToken, Task<TEntity?>> factory, CancellationToken ct)
    {
        var normalizedValue = NormalizeCachedPropertyValue(cachedPropertyValue);

        return cache.GetOrCreateAsync(
            IndexKey(cachedPropertyName, normalizedValue),
            async (entry, token) =>
            {
                entry.SetOptions(EntityOptions);
                return await factory(token);
            },
            ct);
    }

    public async Task<IReadOnlyList<TEntity>> GetOrCreateAllAsync(Func<CancellationToken, Task<IReadOnlyList<TEntity>>> factory, CancellationToken ct)
    {
        var cachedEntities = await cache.GetOrCreateAsync(
            AllKey,
            async (entry, token) =>
            {
                entry.SetOptions(ListOptions);
                return await factory(token);
            },
            ct);

        return cachedEntities ?? [];
    }
}
