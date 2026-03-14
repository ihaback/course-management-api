using Backend.Application.Common;
using Backend.Application.Modules.VenueTypes;
using Backend.Application.Modules.VenueTypes.Caching;
using Backend.Application.Modules.VenueTypes.Inputs;
using Backend.Domain.Modules.VenueTypes.Contracts;
using Backend.Domain.Modules.VenueTypes.Models;
using NSubstitute;
using NSubstitute.ClearExtensions;

namespace Backend.Tests.Unit.Application.Modules.VenueTypes;

public class VenueTypeService_Tests
{
    [Fact]
    public async Task GetAll_Should_Return_Items()
    {
        var repo = Substitute.For<IVenueTypeRepository>();
        var cache = Substitute.For<IVenueTypeCache>();
        cache.GetAllAsync(Arg.Any<Func<CancellationToken, Task<IReadOnlyList<VenueType>>>>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Func<CancellationToken, Task<IReadOnlyList<VenueType>>>>()(ci.Arg<CancellationToken>()));
        repo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns([VenueType.Reconstitute(1, "InPerson"), VenueType.Reconstitute(2, "Online")]);
        var service = new VenueTypeService(cache, repo);

        var result = await service.GetAllVenueTypesAsync();

        Assert.True(result.Success);
        Assert.Null(result.ErrorType);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public async Task GetById_Should_Return_From_Cache_Without_Hitting_Repository()
    {
        var repo = Substitute.For<IVenueTypeRepository>();
        var cache = Substitute.For<IVenueTypeCache>();
        var cached = VenueType.Reconstitute(3, "Hybrid");

        cache.GetByIdAsync(
                3,
                Arg.Any<Func<CancellationToken, Task<VenueType?>>>(),
                Arg.Any<CancellationToken>())
            .Returns(cached);

        var service = new VenueTypeService(cache, repo);

        var result = await service.GetVenueTypeByIdAsync(3, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(cached, result.Value);
        await repo.DidNotReceive().GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_Should_Reset_And_Set_Cache()
    {
        var repo = Substitute.For<IVenueTypeRepository>();
        var cache = Substitute.For<IVenueTypeCache>();
        var existing = VenueType.Reconstitute(4, "Onsite");
        var updated = VenueType.Reconstitute(4, "Remote");

        repo.GetByIdAsync(existing.Id, Arg.Any<CancellationToken>())
            .Returns(existing);
        repo.UpdateAsync(existing.Id, Arg.Any<VenueType>(), Arg.Any<CancellationToken>())
            .Returns(updated);

        var service = new VenueTypeService(cache, repo);

        var result = await service.UpdateVenueTypeAsync(new UpdateVenueTypeInput(existing.Id, "Remote"), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(updated, result.Value);
        cache.Received(1).ResetEntity(existing);
        cache.Received(1).SetEntity(updated);
    }
}

