using Backend.Application.Common.Caching;
using Backend.Domain.Modules.VenueTypes.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Backend.Application.Modules.VenueTypes.Caching;

public sealed class VenueTypeCache(IMemoryCache cache) : CacheEntityBase<VenueType, int>(cache), IVenueTypeCache
{
    protected override int GetId(VenueType entity) => entity.Id;

    protected override IEnumerable<(string PropertyName, string Value)> GetCachedProperties(VenueType entity)
        => [("name", entity.Name)];

    public Task<VenueType?> GetByIdAsync(int id, Func<CancellationToken, Task<VenueType?>> factory, CancellationToken ct)
        => GetOrCreateByIdAsync(id, factory, ct);

    public Task<VenueType?> GetByNameAsync(string name, Func<CancellationToken, Task<VenueType?>> factory, CancellationToken ct)
        => GetOrCreateByPropertyNameAsync("name", name, factory, ct);

    public Task<IReadOnlyList<VenueType>> GetAllAsync(Func<CancellationToken, Task<IReadOnlyList<VenueType>>> factory, CancellationToken ct)
        => GetOrCreateAllAsync(factory, ct);
}
