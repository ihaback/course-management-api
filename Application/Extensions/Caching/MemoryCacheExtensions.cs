using Microsoft.Extensions.Caching.Memory;

namespace Backend.Application.Extensions.Caching;

public static class MemoryCacheExtensions
{
    public static async Task<T?> GetOrCreateAsync<T>(this IMemoryCache cache, object key, Func<ICacheEntry, CancellationToken, Task<T?>> action, CancellationToken ct = default)
    {
        if (cache.TryGetValue(key, out T? existing))
            return existing;

        using var entry = cache.CreateEntry(key);
        var created = await action(entry, ct);
        entry.Value = created;
        return created;
    }
}
