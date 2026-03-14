using Backend.Domain.Modules.CourseEventTypes.Models;
using Backend.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Tests.Integration.Infrastructure;

[Collection(SqliteInMemoryCollection.Name)]
public class CourseEventTypeRepository_Tests(SqliteInMemoryFixture fixture)
{
    [Fact]
    public async Task CreateCourseEventTypeAsync_ShouldPersist_And_BeReadableByIdAndName()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new CourseEventTypeRepository(context);
        var name = $"Lecture-{Guid.NewGuid():N}";

        var created = await repo.AddAsync(CourseEventType.Create(name), CancellationToken.None);
        var byId = await repo.GetByIdAsync(created.Id, CancellationToken.None);
        var byName = await repo.GetCourseEventTypeByTypeNameAsync(name, CancellationToken.None);

        Assert.True(created.Id > 0);
        Assert.NotNull(byId);
        Assert.NotNull(byName);
        Assert.Equal(name, created.Name);
        Assert.Equal(created.Id, byId!.Id);
        Assert.Equal(name, byId.Name);
        Assert.Equal(created.Id, byName!.Id);
        Assert.Equal(name, byName.Name);

        var persisted = await context.CourseEventTypes
            .AsNoTracking()
            .SingleAsync(x => x.Id == created.Id, CancellationToken.None);

        Assert.Equal(created.Id, persisted.Id);
        Assert.Equal(name, persisted.Name);
    }

    [Fact]
    public async Task GetAllCourseEventTypesAsync_ShouldIncludeCreatedType()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new CourseEventTypeRepository(context);
        var created = await repo.AddAsync(CourseEventType.Create($"Type-{Guid.NewGuid():N}"), CancellationToken.None);

        var all = await repo.GetAllAsync(CancellationToken.None);

        Assert.Contains(all, x => x.Id == created.Id);
    }

    [Fact]
    public async Task GetAllCourseEventTypesAsync_ShouldReturnDescendingById()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new CourseEventTypeRepository(context);
        var first = await repo.AddAsync(CourseEventType.Create($"TypeA-{Guid.NewGuid():N}"), CancellationToken.None);
        var second = await repo.AddAsync(CourseEventType.Create($"TypeB-{Guid.NewGuid():N}"), CancellationToken.None);

        var all = await repo.GetAllAsync(CancellationToken.None);

        var firstIndex = all.ToList().FindIndex(x => x.Id == first.Id);
        var secondIndex = all.ToList().FindIndex(x => x.Id == second.Id);

        Assert.True(firstIndex >= 0);
        Assert.True(secondIndex >= 0);
        Assert.True(secondIndex < firstIndex);
    }

    [Fact]
    public async Task UpdateCourseEventTypeAsync_ShouldPersistNewTypeName()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new CourseEventTypeRepository(context);
        var created = await repo.AddAsync(CourseEventType.Create($"Type-{Guid.NewGuid():N}"), CancellationToken.None);

        var updated = await repo.UpdateAsync(created.Id, CourseEventType.Reconstitute(created.Id, "UpdatedType"), CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal("UpdatedType", updated!.Name);

        var persisted = await context.CourseEventTypes
            .AsNoTracking()
            .SingleAsync(x => x.Id == created.Id, CancellationToken.None);

        Assert.Equal("UpdatedType", persisted.Name);
    }

    [Fact]
    public async Task IsInUseAsync_ShouldReturnTrueWhenReferencedByCourseEvent()
    {
        await using var context = fixture.CreateDbContext();
        var type = await RepositoryTestDataHelper.CreateCourseEventTypeAsync(context);
        await RepositoryTestDataHelper.CreateCourseEventAsync(context, typeId: type.Id);
        var repo = new CourseEventTypeRepository(context);

        var inUse = await repo.IsInUseAsync(type.Id, CancellationToken.None);

        Assert.True(inUse);
    }

    [Fact]
    public async Task DeleteCourseEventTypeAsync_ShouldRemoveType()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new CourseEventTypeRepository(context);
        var created = await repo.AddAsync(CourseEventType.Create($"Type-{Guid.NewGuid():N}"), CancellationToken.None);

        var deleted = await repo.RemoveAsync(created.Id, CancellationToken.None);
        var loaded = await repo.GetByIdAsync(created.Id, CancellationToken.None);

        Assert.True(deleted);
        Assert.Null(loaded);
    }
}

