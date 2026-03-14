using Backend.Domain.Modules.CourseEvents.Models;
using Backend.Domain.Modules.VenueTypes.Models;
using Backend.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Tests.Integration.Infrastructure;

[Collection(SqliteInMemoryCollection.Name)]
public class VenueTypeRepository_Tests(SqliteInMemoryFixture fixture)
{
    [Fact]
    public async Task CreateVenueTypeAsync_ShouldPersist_And_BeReadableByIdAndName()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new VenueTypeRepository(context);
        var name = $"Venue-{Guid.NewGuid():N}";

        var created = await repo.AddAsync(VenueType.Reconstitute(1, name), CancellationToken.None);
        var byId = await repo.GetByIdAsync(created.Id, CancellationToken.None);
        var byName = await repo.GetByNameAsync(name, CancellationToken.None);

        Assert.NotNull(byId);
        Assert.NotNull(byName);
        Assert.Equal(name, byId!.Name);
        Assert.Equal(created.Id, byName!.Id);

        var persisted = await context.VenueTypes
            .AsNoTracking()
            .SingleAsync(x => x.Id == created.Id, CancellationToken.None);

        Assert.Equal(name, persisted.Name);
    }

    [Fact]
    public async Task GetAllVenueTypesAsync_ShouldReturnDescendingById()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new VenueTypeRepository(context);
        var first = await repo.AddAsync(VenueType.Reconstitute(1, $"VenueA-{Guid.NewGuid():N}"), CancellationToken.None);
        var second = await repo.AddAsync(VenueType.Reconstitute(1, $"VenueB-{Guid.NewGuid():N}"), CancellationToken.None);

        var all = await repo.GetAllAsync(CancellationToken.None);
        var firstIndex = all.ToList().FindIndex(x => x.Id == first.Id);
        var secondIndex = all.ToList().FindIndex(x => x.Id == second.Id);

        Assert.True(firstIndex >= 0);
        Assert.True(secondIndex >= 0);
        Assert.True(secondIndex < firstIndex);
    }

    [Fact]
    public async Task UpdateVenueTypeAsync_ShouldPersistChanges()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new VenueTypeRepository(context);
        var created = await repo.AddAsync(VenueType.Reconstitute(1, $"Venue-{Guid.NewGuid():N}"), CancellationToken.None);

        var newName = $"Hybrid-{Guid.NewGuid():N}";
        var updated = await repo.UpdateAsync(created.Id, VenueType.Reconstitute(created.Id, newName), CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal(newName, updated!.Name);

        var persisted = await context.VenueTypes.AsNoTracking().SingleAsync(x => x.Id == created.Id, CancellationToken.None);
        Assert.Equal(newName, persisted.Name);
    }

    [Fact]
    public async Task IsInUseAsync_ShouldReturnTrue_WhenReferencedByCourseEvent()
    {
        await using var context = fixture.CreateDbContext();
        var venueTypeRepo = new VenueTypeRepository(context);
        var venueType = await venueTypeRepo.AddAsync(
            VenueType.Reconstitute(1, $"Venue-{Guid.NewGuid():N}"),
            CancellationToken.None);
        var course = await RepositoryTestDataHelper.CreateCourseAsync(context);
        var type = await RepositoryTestDataHelper.CreateCourseEventTypeAsync(context);
        var eventRepo = new CourseEventRepository(context);

        await eventRepo.AddAsync(
            CourseEvent.Reconstitute(
                Guid.NewGuid(),
                course.Id,
                DateTime.UtcNow.AddDays(1),
                100m,
                10,
                type.Id,
                VenueType.Reconstitute(venueType.Id, venueType.Name)),
            CancellationToken.None);

        var inUse = await venueTypeRepo.IsInUseAsync(venueType.Id, CancellationToken.None);

        Assert.True(inUse);
    }

    [Fact]
    public async Task DeleteVenueTypeAsync_ShouldRemoveVenueType()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new VenueTypeRepository(context);
        var created = await repo.AddAsync(VenueType.Reconstitute(1, $"Venue-{Guid.NewGuid():N}"), CancellationToken.None);

        var removed = await repo.RemoveAsync(created.Id, CancellationToken.None);
        var byId = await repo.GetByIdAsync(created.Id, CancellationToken.None);

        Assert.True(removed);
        Assert.Null(byId);

        var exists = await context.VenueTypes.AnyAsync(x => x.Id == created.Id, CancellationToken.None);
        Assert.False(exists);
    }
}

