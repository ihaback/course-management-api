using Backend.Application.Modules.VenueTypes;
using Backend.Application.Modules.VenueTypes.Caching;
using Backend.Application.Modules.VenueTypes.Inputs;
using Backend.Domain.Modules.VenueTypes.Contracts;
using Backend.Domain.Modules.VenueTypes.Models;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;

namespace Backend.Tests.Unit.Application.Modules.VenueTypes;

/// <summary>
/// Cache-integration tests for <see cref="VenueTypeService"/> that use a real
/// <see cref="VenueTypeCache"/> backed by a real <see cref="MemoryCache"/>.
/// These tests verify actual cache behaviour — not just that methods were called.
/// </summary>
public class VenueTypeService_CacheIntegration_Tests
{
    private static (VenueTypeService service, VenueTypeCache cache, IVenueTypeRepository repo) Create()
    {
        var repo = Substitute.For<IVenueTypeRepository>();
        var cache = new VenueTypeCache(new MemoryCache(new MemoryCacheOptions()));
        return (new VenueTypeService(cache, repo), cache, repo);
    }

    [Fact]
    public async Task UpdateVenueTypeAsync_EvictsOldNameKey_AndCachesNewName()
    {
        var (service, cache, repo) = Create();
        var original = VenueType.Reconstitute(1, "Onsite");
        var renamed = VenueType.Reconstitute(1, "Remote");

        cache.SetEntity(original);

        repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(original);
        repo.UpdateAsync(1, Arg.Any<VenueType>(), Arg.Any<CancellationToken>()).Returns(renamed);

        await service.UpdateVenueTypeAsync(new UpdateVenueTypeInput(1, "Remote"), CancellationToken.None);

        var oldNameCalls = 0;
        await cache.GetByNameAsync("Onsite",
            _ => { oldNameCalls++; return Task.FromResult<VenueType?>(renamed); },
            CancellationToken.None);

        var newNameCalls = 0;
        await cache.GetByNameAsync("Remote",
            _ => { newNameCalls++; return Task.FromResult<VenueType?>(renamed); },
            CancellationToken.None);

        Assert.Equal(1, oldNameCalls);
        Assert.Equal(0, newNameCalls);
    }

    [Fact]
    public async Task GetVenueTypeByNameAsync_HitsCache_OnSecondCall_RepoCalledOnce()
    {
        var (service, _, repo) = Create();
        var entity = VenueType.Reconstitute(1, "Virtual");

        repo.GetByNameAsync("Virtual", Arg.Any<CancellationToken>()).Returns(entity);

        await service.GetVenueTypeByNameAsync("Virtual", CancellationToken.None);
        var second = await service.GetVenueTypeByNameAsync("Virtual", CancellationToken.None);

        Assert.True(second.Success);
        Assert.Equal("Virtual", second.Value!.Name);
        await repo.Received(1).GetByNameAsync("Virtual", Arg.Any<CancellationToken>());
    }
}
