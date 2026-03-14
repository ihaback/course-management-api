using Backend.Domain.Modules.CourseRegistrationStatuses.Models;
using Backend.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Tests.Integration.Infrastructure;

[Collection(SqliteInMemoryCollection.Name)]
public class CourseRegistrationStatusRepository_Tests(SqliteInMemoryFixture fixture)
{
    [Fact]
    public async Task CreateCourseRegistrationStatusAsync_ShouldPersist_And_BeReadableByName()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new CourseRegistrationStatusRepository(context);
        var name = $"Status-{Guid.NewGuid():N}";

        var created = await repo.AddAsync(CourseRegistrationStatus.Create(name), CancellationToken.None);
        var byName = await repo.GetCourseRegistrationStatusByNameAsync(name, CancellationToken.None);

        Assert.NotNull(byName);
        Assert.Equal(created.Id, byName!.Id);
        Assert.Equal(name, created.Name);
        Assert.Equal(name, byName.Name);

        var persisted = await context.CourseRegistrationStatuses
            .AsNoTracking()
            .SingleAsync(x => x.Id == created.Id, CancellationToken.None);

        Assert.Equal(created.Id, persisted.Id);
        Assert.Equal(name, persisted.Name);
    }

    [Fact]
    public async Task GetAllCourseRegistrationStatusesAsync_ShouldContainSeededStatuses()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new CourseRegistrationStatusRepository(context);

        var all = await repo.GetAllAsync(CancellationToken.None);

        Assert.Contains(all, x => x.Id == 0 && x.Name == "Pending");
        Assert.Contains(all, x => x.Id == 1 && x.Name == "Paid");
    }

    [Fact]
    public async Task GetAllCourseRegistrationStatusesAsync_ShouldReturnDescendingById()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new CourseRegistrationStatusRepository(context);

        var all = await repo.GetAllAsync(CancellationToken.None);

        for (var i = 1; i < all.Count; i++)
        {
            Assert.True(all[i - 1].Id >= all[i].Id);
        }
    }

    [Fact]
    public async Task GetCourseRegistrationStatusByIdAsync_ShouldReturnStatus()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new CourseRegistrationStatusRepository(context);

        var loaded = await repo.GetByIdAsync(0, CancellationToken.None);

        Assert.NotNull(loaded);
        Assert.Equal("Pending", loaded!.Name);
    }

    [Fact]
    public async Task GetCourseRegistrationStatusByIdAsync_ShouldReturnNull_WhenStatusDoesNotExist()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new CourseRegistrationStatusRepository(context);

        var loaded = await repo.GetByIdAsync(999_999, CancellationToken.None);

        Assert.Null(loaded);
    }

    [Fact]
    public async Task UpdateCourseRegistrationStatusAsync_ShouldPersistChanges()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new CourseRegistrationStatusRepository(context);
        var created = await repo.AddAsync(CourseRegistrationStatus.Create($"Status-{Guid.NewGuid():N}"), CancellationToken.None);

        var updated = await repo.UpdateAsync(created.Id, CourseRegistrationStatus.Reconstitute(created.Id, "Renamed"), CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal("Renamed", updated!.Name);

        var persisted = await context.CourseRegistrationStatuses
            .AsNoTracking()
            .SingleAsync(x => x.Id == created.Id, CancellationToken.None);

        Assert.Equal("Renamed", persisted.Name);
    }

    [Fact]
    public async Task IsInUseAsync_ShouldReturnTrueWhenReferencedByRegistration()
    {
        await using var context = fixture.CreateDbContext();
        await RepositoryTestDataHelper.CreateCourseRegistrationAsync(context, status: CourseRegistrationStatus.Pending);
        var repo = new CourseRegistrationStatusRepository(context);

        var inUse = await repo.IsInUseAsync(0, CancellationToken.None);

        Assert.True(inUse);
    }

    [Fact]
    public async Task DeleteCourseRegistrationStatusAsync_ShouldRemoveStatus()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new CourseRegistrationStatusRepository(context);
        var created = await repo.AddAsync(CourseRegistrationStatus.Create($"Status-{Guid.NewGuid():N}"), CancellationToken.None);

        var deleted = await repo.RemoveAsync(created.Id, CancellationToken.None);
        var loaded = await repo.GetByIdAsync(created.Id, CancellationToken.None);

        Assert.True(deleted);
        Assert.Null(loaded);
    }

    [Fact]
    public async Task CreateCourseRegistrationStatusAsync_ShouldThrow_WhenNameAlreadyExists()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new CourseRegistrationStatusRepository(context);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            repo.AddAsync(CourseRegistrationStatus.Create("Pending"), CancellationToken.None));
    }

    [Fact]
    public async Task DeleteCourseRegistrationStatusAsync_ShouldThrow_WhenStatusIsInUseByRegistration()
    {
        await using var context = fixture.CreateDbContext();
        await RepositoryTestDataHelper.CreateCourseRegistrationAsync(context, status: CourseRegistrationStatus.Pending);
        var repo = new CourseRegistrationStatusRepository(context);

        var exception = await Record.ExceptionAsync(() => repo.RemoveAsync(CourseRegistrationStatus.Pending.Id, CancellationToken.None));

        Assert.NotNull(exception);
        Assert.True(exception is InvalidOperationException or DbUpdateException);
    }

    [Fact]
    public async Task GetCourseRegistrationStatusByIdAsync_ShouldThrow_WhenIdIsNegative()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new CourseRegistrationStatusRepository(context);

        await Assert.ThrowsAsync<ArgumentException>(() => repo.GetByIdAsync(-1, CancellationToken.None));
    }

    [Fact]
    public async Task IsInUseAsync_ShouldThrow_WhenIdIsNegative()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new CourseRegistrationStatusRepository(context);

        await Assert.ThrowsAsync<ArgumentException>(() => repo.IsInUseAsync(-1, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateCourseRegistrationStatusAsync_ShouldThrow_WhenStatusDoesNotExist()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new CourseRegistrationStatusRepository(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            repo.UpdateAsync(999_999, CourseRegistrationStatus.Reconstitute(999_999, "Missing"), CancellationToken.None));
    }

    [Fact]
    public async Task DeleteCourseRegistrationStatusAsync_ShouldThrow_WhenStatusDoesNotExist()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new CourseRegistrationStatusRepository(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => repo.RemoveAsync(999_999, CancellationToken.None));
    }
}

