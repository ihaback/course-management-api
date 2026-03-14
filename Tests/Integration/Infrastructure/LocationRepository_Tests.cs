using Backend.Domain.Modules.Locations.Models;
using Backend.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Tests.Integration.Infrastructure;

[Collection(SqliteInMemoryCollection.Name)]
public class LocationRepository_Tests(SqliteInMemoryFixture fixture)
{
    [Fact]
    public async Task CreateLocationAsync_ShouldPersist_And_BeReadableById()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new LocationRepository(context);
        var input = Location.Reconstitute(0, "Test Street 1", "12345", "Test City");

        var created = await repo.AddAsync(input, CancellationToken.None);
        var loaded = await repo.GetByIdAsync(created.Id, CancellationToken.None);

        Assert.NotNull(loaded);
        Assert.Equal(created.Id, loaded!.Id);
        Assert.Equal(input.StreetName, created.StreetName);
        Assert.Equal(input.PostalCode, created.PostalCode);
        Assert.Equal(input.City, created.City);

        var persisted = await context.Locations
            .AsNoTracking()
            .SingleAsync(x => x.Id == created.Id, CancellationToken.None);

        Assert.Equal(created.Id, persisted.Id);
        Assert.Equal(input.StreetName, persisted.StreetName);
        Assert.Equal(input.PostalCode, persisted.PostalCode);
        Assert.Equal(input.City, persisted.City);
    }

    [Fact]
    public async Task GetAllLocationsAsync_ShouldContainCreatedLocation()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new LocationRepository(context);
        var created = await repo.AddAsync(Location.Reconstitute(0, $"Street-{Guid.NewGuid():N}", "54321", "Town"), CancellationToken.None);

        var all = await repo.GetAllAsync(CancellationToken.None);

        Assert.Contains(all, x => x.Id == created.Id);
    }

    [Fact]
    public async Task GetAllLocationsAsync_ShouldReturnDescendingById()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new LocationRepository(context);
        var first = await repo.AddAsync(Location.Reconstitute(0, $"StreetA-{Guid.NewGuid():N}", "11111", "A"), CancellationToken.None);
        var second = await repo.AddAsync(Location.Reconstitute(0, $"StreetB-{Guid.NewGuid():N}", "22222", "B"), CancellationToken.None);

        var all = await repo.GetAllAsync(CancellationToken.None);
        var firstIndex = all.ToList().FindIndex(x => x.Id == first.Id);
        var secondIndex = all.ToList().FindIndex(x => x.Id == second.Id);

        Assert.True(firstIndex >= 0);
        Assert.True(secondIndex >= 0);
        Assert.True(secondIndex < firstIndex);
    }

    [Fact]
    public async Task UpdateLocationAsync_ShouldPersistChanges()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new LocationRepository(context);
        var created = await repo.AddAsync(Location.Reconstitute(0, $"Street-{Guid.NewGuid():N}", "54321", "Town"), CancellationToken.None);

        var updated = await repo.UpdateAsync(created.Id, Location.Reconstitute(created.Id, "New Street", "11111", "New City"), CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal("New City", updated!.City);

        var persisted = await context.Locations
            .AsNoTracking()
            .SingleAsync(x => x.Id == created.Id, CancellationToken.None);

        Assert.Equal("New Street", persisted.StreetName);
        Assert.Equal("11111", persisted.PostalCode);
        Assert.Equal("New City", persisted.City);
    }

    [Fact]
    public async Task HasInPlaceLocationsAsync_ShouldReturnTrueWhenReferenced()
    {
        await using var context = fixture.CreateDbContext();
        var location = await RepositoryTestDataHelper.CreateLocationAsync(context);
        await RepositoryTestDataHelper.CreateInPlaceLocationAsync(context, location.Id);
        var repo = new LocationRepository(context);

        var hasChildren = await repo.HasInPlaceLocationsAsync(location.Id, CancellationToken.None);

        Assert.True(hasChildren);
    }

    [Fact]
    public async Task DeleteLocationAsync_ShouldRemoveLocation()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new LocationRepository(context);
        var created = await repo.AddAsync(Location.Reconstitute(0, $"Street-{Guid.NewGuid():N}", "54321", "Town"), CancellationToken.None);

        var deleted = await repo.RemoveAsync(created.Id, CancellationToken.None);
        var loaded = await repo.GetByIdAsync(created.Id, CancellationToken.None);

        Assert.True(deleted);
        Assert.Null(loaded);
    }
}

