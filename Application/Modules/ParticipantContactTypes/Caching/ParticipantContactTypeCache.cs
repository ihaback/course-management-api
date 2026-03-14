using Backend.Application.Common.Caching;
using Backend.Domain.Modules.ParticipantContactTypes.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Backend.Application.Modules.ParticipantContactTypes.Caching;

public sealed class ParticipantContactTypeCache(IMemoryCache cache) : CacheEntityBase<ParticipantContactType, int>(cache), IParticipantContactTypeCache
{
    protected override int GetId(ParticipantContactType entity) => entity.Id;

    protected override IEnumerable<(string PropertyName, string Value)> GetCachedProperties(ParticipantContactType entity)
        => [("name", entity.Name)];

    public Task<ParticipantContactType?> GetByIdAsync(int id, Func<CancellationToken, Task<ParticipantContactType?>> factory, CancellationToken ct)
        => GetOrCreateByIdAsync(id, factory, ct);

    public Task<ParticipantContactType?> GetByNameAsync(string name, Func<CancellationToken, Task<ParticipantContactType?>> factory, CancellationToken ct)
        => GetOrCreateByPropertyNameAsync("name", name, factory, ct);

    public Task<IReadOnlyList<ParticipantContactType>> GetAllAsync(Func<CancellationToken, Task<IReadOnlyList<ParticipantContactType>>> factory, CancellationToken ct)
        => GetOrCreateAllAsync(factory, ct);
}
