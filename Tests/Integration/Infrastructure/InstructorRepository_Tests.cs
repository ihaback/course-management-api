using Backend.Domain.Modules.Instructors.Models;
using Backend.Infrastructure.Persistence.EFC.Context;
using Backend.Infrastructure.Persistence.Entities;
using Backend.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Tests.Integration.Infrastructure;

[Collection(SqliteInMemoryCollection.Name)]
public class InstructorRepository_Tests(SqliteInMemoryFixture fixture)
{
    private sealed class TestableInstructorRepository(CoursesOnlineDbContext context)
        : InstructorRepository(context)
    {
        public Instructor MapToModel(InstructorEntity entity) => base.ToModel(entity);
    }

    [Fact]
    public async Task CreateInstructorAsync_ShouldPersist_And_BeReadableById()
    {
        await using var context = fixture.CreateDbContext();
        var role = await RepositoryTestDataHelper.CreateInstructorRoleAsync(context);
        var repo = new InstructorRepository(context);
        var input = Instructor.Reconstitute(Guid.NewGuid(), "Test Instructor", role);

        var created = await repo.AddAsync(input, CancellationToken.None);
        var loaded = await repo.GetByIdAsync(created.Id, CancellationToken.None);

        Assert.NotNull(loaded);
        Assert.Equal(input.Id, created.Id);
        Assert.Equal(input.Name, created.Name);
        Assert.Equal(role.Id, created.InstructorRoleId);
        Assert.Equal(role.Id, loaded!.InstructorRoleId);
        Assert.Equal(role.Name, loaded.Role.Name);

        var persisted = await context.Instructors
            .AsNoTracking()
            .SingleAsync(x => x.Id == input.Id, CancellationToken.None);

        Assert.Equal(input.Id, persisted.Id);
        Assert.Equal(input.Name, persisted.Name);
        Assert.Equal(role.Id, persisted.InstructorRoleId);
    }

    [Fact]
    public async Task GetAllInstructorsAsync_ShouldContainCreatedInstructor()
    {
        await using var context = fixture.CreateDbContext();
        var created = await RepositoryTestDataHelper.CreateInstructorAsync(context);
        var repo = new InstructorRepository(context);

        var all = await repo.GetAllAsync(CancellationToken.None);

        Assert.Contains(all, x => x.Id == created.Id);
    }

    [Fact]
    public async Task GetAllInstructorsAsync_ShouldReturnDescendingById()
    {
        await using var context = fixture.CreateDbContext();
        _ = await RepositoryTestDataHelper.CreateInstructorAsync(context);
        _ = await RepositoryTestDataHelper.CreateInstructorAsync(context);
        var repo = new InstructorRepository(context);

        var all = await repo.GetAllAsync(CancellationToken.None);

        for (var i = 1; i < all.Count; i++)
        {
            Assert.True(all[i - 1].Id.CompareTo(all[i].Id) >= 0);
        }
    }

    [Fact]
    public async Task UpdateInstructorAsync_ShouldPersistChanges()
    {
        await using var context = fixture.CreateDbContext();
        var instructor = await RepositoryTestDataHelper.CreateInstructorAsync(context);
        var repo = new InstructorRepository(context);

        var updated = await repo.UpdateAsync(
            instructor.Id,
            Instructor.Reconstitute(instructor.Id, "Updated Instructor", instructor.Role),
            CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal("Updated Instructor", updated!.Name);

        var persisted = await context.Instructors
            .AsNoTracking()
            .SingleAsync(x => x.Id == instructor.Id, CancellationToken.None);

        Assert.Equal("Updated Instructor", persisted.Name);
    }

    [Fact]
    public async Task HasCourseEventsAsync_ShouldReturnTrueWhenLinked()
    {
        await using var context = fixture.CreateDbContext();
        var instructor = await RepositoryTestDataHelper.CreateInstructorAsync(context);
        var eventEntity = await RepositoryTestDataHelper.CreateCourseEventAsync(context);
        await RepositoryTestDataHelper.LinkInstructorToCourseEventAsync(context, instructor.Id, eventEntity.Id);
        var repo = new InstructorRepository(context);

        var hasEvents = await repo.HasCourseEventsAsync(instructor.Id, CancellationToken.None);

        Assert.True(hasEvents);
    }

    [Fact]
    public async Task DeleteInstructorAsync_ShouldRemoveEntity()
    {
        await using var context = fixture.CreateDbContext();
        var instructor = await RepositoryTestDataHelper.CreateInstructorAsync(context);
        var repo = new InstructorRepository(context);

        var deleted = await repo.RemoveAsync(instructor.Id, CancellationToken.None);
        var loaded = await repo.GetByIdAsync(instructor.Id, CancellationToken.None);

        Assert.True(deleted);
        Assert.Null(loaded);
    }

    [Fact]
    public async Task ToModel_ShouldThrow_WhenInstructorRoleIsNotLoaded()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new TestableInstructorRepository(context);
        var entity = new InstructorEntity
        {
            Id = Guid.NewGuid(),
            Name = "Instructor",
            InstructorRoleId = 1,
            InstructorRole = null!
        };

        var ex = Assert.Throws<InvalidOperationException>(() => repo.MapToModel(entity));
        Assert.Equal("Instructor role must be loaded from database.", ex.Message);
    }
}

