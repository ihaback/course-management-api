using Backend.Domain.Modules.InPlaceLocations.Models;
using Backend.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Tests.Integration.Infrastructure;

[Collection(SqliteInMemoryCollection.Name)]
public class InPlaceLocationRepository_Tests(SqliteInMemoryFixture fixture)
{
    [Fact]
    public async Task CreateInPlaceLocationAsync_ShouldPersist_And_BeQueryableByLocation()
    {
        await using var context = fixture.CreateDbContext();
        var location = await RepositoryTestDataHelper.CreateLocationAsync(context);
        var repo = new InPlaceLocationRepository(context);
        var input = InPlaceLocation.Reconstitute(0, location.Id, 101, 20);

        var created = await repo.AddAsync(input, CancellationToken.None);
        var loaded = await repo.GetByIdAsync(created.Id, CancellationToken.None);
        var byLocation = await repo.GetInPlaceLocationsByLocationIdAsync(location.Id, CancellationToken.None);

        Assert.NotNull(loaded);
        Assert.Contains(byLocation, x => x.Id == created.Id);
        Assert.Equal(input.LocationId, created.LocationId);
        Assert.Equal(input.RoomNumber, created.RoomNumber);
        Assert.Equal(input.Seats, created.Seats);

        var persisted = await context.InPlaceLocations
            .AsNoTracking()
            .SingleAsync(x => x.Id == created.Id, CancellationToken.None);

        Assert.Equal(created.Id, persisted.Id);
        Assert.Equal(input.LocationId, persisted.LocationId);
        Assert.Equal(input.RoomNumber, persisted.RoomNumber);
        Assert.Equal(input.Seats, persisted.Seats);
    }

    [Fact]
    public async Task GetAllInPlaceLocationsAsync_ShouldContainCreatedEntity()
    {
        await using var context = fixture.CreateDbContext();
        var created = await RepositoryTestDataHelper.CreateInPlaceLocationAsync(context);
        var repo = new InPlaceLocationRepository(context);

        var all = await repo.GetAllAsync(CancellationToken.None);

        Assert.Contains(all, x => x.Id == created.Id);
    }

    [Fact]
    public async Task GetAllInPlaceLocationsAsync_ShouldReturnDescendingById()
    {
        await using var context = fixture.CreateDbContext();
        var first = await RepositoryTestDataHelper.CreateInPlaceLocationAsync(context);
        var second = await RepositoryTestDataHelper.CreateInPlaceLocationAsync(context);
        var repo = new InPlaceLocationRepository(context);

        var all = await repo.GetAllAsync(CancellationToken.None);
        var firstIndex = all.ToList().FindIndex(x => x.Id == first.Id);
        var secondIndex = all.ToList().FindIndex(x => x.Id == second.Id);

        Assert.True(firstIndex >= 0);
        Assert.True(secondIndex >= 0);
        Assert.True(secondIndex < firstIndex);
    }

    [Fact]
    public async Task UpdateInPlaceLocationAsync_ShouldPersistChanges()
    {
        await using var context = fixture.CreateDbContext();
        var created = await RepositoryTestDataHelper.CreateInPlaceLocationAsync(context);
        var repo = new InPlaceLocationRepository(context);

        var updated = await repo.UpdateAsync(
            created.Id,
            InPlaceLocation.Reconstitute(created.Id, created.LocationId, 202, 30),
            CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal(202, updated!.RoomNumber);

        var persisted = await context.InPlaceLocations
            .AsNoTracking()
            .SingleAsync(x => x.Id == created.Id, CancellationToken.None);

        Assert.Equal(202, persisted.RoomNumber);
        Assert.Equal(30, persisted.Seats);
    }

    [Fact]
    public async Task HasCourseEventsAsync_ShouldReturnTrueWhenLinked()
    {
        await using var context = fixture.CreateDbContext();
        var inPlace = await RepositoryTestDataHelper.CreateInPlaceLocationAsync(context);
        var courseEvent = await RepositoryTestDataHelper.CreateCourseEventAsync(context);
        await RepositoryTestDataHelper.LinkInPlaceLocationToCourseEventAsync(context, inPlace.Id, courseEvent.Id);
        var repo = new InPlaceLocationRepository(context);

        var hasEvents = await repo.HasCourseEventsAsync(inPlace.Id, CancellationToken.None);

        Assert.True(hasEvents);
    }

    [Fact]
    public async Task DeleteInPlaceLocationAsync_ShouldRemoveEntity()
    {
        await using var context = fixture.CreateDbContext();
        var created = await RepositoryTestDataHelper.CreateInPlaceLocationAsync(context);
        var repo = new InPlaceLocationRepository(context);

        var deleted = await repo.RemoveAsync(created.Id, CancellationToken.None);
        var loaded = await repo.GetByIdAsync(created.Id, CancellationToken.None);

        Assert.True(deleted);
        Assert.Null(loaded);
    }
}

