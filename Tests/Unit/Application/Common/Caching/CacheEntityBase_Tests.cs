using Backend.Application.Common.Caching;
using Backend.Domain.Modules.VenueTypes.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Backend.Tests.Unit.Application.Common.Caching;

/// <summary>
/// Tests for <see cref="CacheEntityBase{TEntity,TId}"/> using a real <see cref="MemoryCache"/>
/// so that actual store/retrieve/evict behaviour is verified rather than just call counts.
/// </summary>
public class CacheEntityBase_Tests
{
    private sealed class NameIndexedCache(IMemoryCache cache)
        : CacheEntityBase<VenueType, int>(cache)
    {
        protected override int GetId(VenueType entity) => entity.Id;

        protected override IEnumerable<(string PropertyName, string Value)> GetCachedProperties(VenueType entity)
            => [("name", entity.Name)];

        public Task<VenueType?> GetById(int id, Func<CancellationToken, Task<VenueType?>> factory, CancellationToken ct)
            => GetOrCreateByIdAsync(id, factory, ct);

        public Task<VenueType?> GetByName(string name, Func<CancellationToken, Task<VenueType?>> factory, CancellationToken ct)
            => GetOrCreateByPropertyNameAsync("name", name, factory, ct);

        public Task<IReadOnlyList<VenueType>> GetAll(Func<CancellationToken, Task<IReadOnlyList<VenueType>>> factory, CancellationToken ct)
            => GetOrCreateAllAsync(factory, ct);
    }

    private static NameIndexedCache CreateCache() =>
        new(new MemoryCache(new MemoryCacheOptions()));

    [Fact]
    public async Task SetEntity_PopulatesIdKey_FactoryNeverCalled()
    {
        var cache = CreateCache();
        var entity = VenueType.Reconstitute(1, "Online");
        cache.SetEntity(entity);

        var calls = 0;
        var result = await cache.GetById(1, _ => { calls++; return Task.FromResult<VenueType?>(null); }, default);

        Assert.Equal(entity, result);
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task SetEntity_PopulatesNameKey_FactoryNeverCalled()
    {
        var cache = CreateCache();
        var entity = VenueType.Reconstitute(1, "Online");
        cache.SetEntity(entity);

        var calls = 0;
        var result = await cache.GetByName("Online", _ => { calls++; return Task.FromResult<VenueType?>(null); }, default);

        Assert.Equal(entity, result);
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task SetEntity_NameLookup_IsCaseInsensitive()
    {
        var cache = CreateCache();
        var entity = VenueType.Reconstitute(1, "OnSiTe");
        cache.SetEntity(entity);

        var calls = 0;
        var result = await cache.GetByName("onsite", _ => { calls++; return Task.FromResult<VenueType?>(null); }, default);

        Assert.Equal(entity, result);
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task GetById_CallsFactory_OnFirstCall_ThenReturnsCachedResult()
    {
        var cache = CreateCache();
        var entity = VenueType.Reconstitute(2, "Hybrid");
        var calls = 0;

        var first = await cache.GetById(2, _ => { calls++; return Task.FromResult<VenueType?>(entity); }, default);
        var second = await cache.GetById(2, _ => { calls++; return Task.FromResult<VenueType?>(entity); }, default);

        Assert.Equal(entity, first);
        Assert.Equal(entity, second);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task GetByName_CallsFactory_OnFirstCall_ThenReturnsCachedResult()
    {
        var cache = CreateCache();
        var entity = VenueType.Reconstitute(3, "Virtual");
        var calls = 0;

        var first = await cache.GetByName("Virtual", _ => { calls++; return Task.FromResult<VenueType?>(entity); }, default);
        var second = await cache.GetByName("Virtual", _ => { calls++; return Task.FromResult<VenueType?>(entity); }, default);

        Assert.Equal(entity, first);
        Assert.Equal(entity, second);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task GetAll_CallsFactory_OnFirstCall_ThenReturnsCachedList()
    {
        var cache = CreateCache();
        IReadOnlyList<VenueType> list = [VenueType.Reconstitute(1, "A"), VenueType.Reconstitute(2, "B")];
        var calls = 0;

        var first = await cache.GetAll(_ => { calls++; return Task.FromResult(list); }, default);
        var second = await cache.GetAll(_ => { calls++; return Task.FromResult(list); }, default);

        Assert.Equal(2, first.Count);
        Assert.Equal(2, second.Count);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task ResetEntity_EvictsIdKey()
    {
        var cache = CreateCache();
        var entity = VenueType.Reconstitute(4, "InPerson");
        cache.SetEntity(entity);

        cache.ResetEntity(entity);

        var calls = 0;
        await cache.GetById(4, _ => { calls++; return Task.FromResult<VenueType?>(entity); }, default);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task ResetEntity_EvictsNameKey()
    {
        var cache = CreateCache();
        var entity = VenueType.Reconstitute(4, "InPerson");
        cache.SetEntity(entity);

        cache.ResetEntity(entity);

        var calls = 0;
        await cache.GetByName("InPerson", _ => { calls++; return Task.FromResult<VenueType?>(entity); }, default);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task ResetEntity_EvictsAllKey()
    {
        var cache = CreateCache();
        IReadOnlyList<VenueType> list = [VenueType.Reconstitute(1, "A")];
        await cache.GetAll(_ => Task.FromResult(list), default);

        var entity = VenueType.Reconstitute(1, "A");
        cache.ResetEntity(entity);

        var calls = 0;
        await cache.GetAll(_ => { calls++; return Task.FromResult(list); }, default);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task ResetEntity_CalledAfterMutation_LeavesStaleNameEntry()
    {
        var cache = CreateCache();
        var entity = VenueType.Reconstitute(1, "Old");

        cache.SetEntity(entity);
        entity.Update("New");
        cache.ResetEntity(entity);

        var calls = 0;
        await cache.GetByName("Old", _ => { calls++; return Task.FromResult<VenueType?>(entity); }, default);
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task ResetEntity_CalledBeforeMutation_CorrectlyEvictsOldName()
    {
        var cache = CreateCache();
        var entity = VenueType.Reconstitute(1, "Old");

        cache.SetEntity(entity);
        cache.ResetEntity(entity);
        entity.Update("New");
        cache.SetEntity(entity);

        var oldCalls = 0;
        await cache.GetByName("Old", _ => { oldCalls++; return Task.FromResult<VenueType?>(entity); }, default);

        var newCalls = 0;
        await cache.GetByName("New", _ => { newCalls++; return Task.FromResult<VenueType?>(entity); }, default);

        Assert.Equal(1, oldCalls);
        Assert.Equal(0, newCalls);
    }
}
