using Backend.Domain.Modules.Courses.Models;
using Backend.Infrastructure.Persistence.Entities;
using Backend.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Backend.Tests.Integration.Infrastructure;

[Collection(SqliteInMemoryCollection.Name)]
public class CoursesRepository_Tests(SqliteInMemoryFixture fixture)
{
    private static void InvokeToCourseEventModel(CourseEventEntity entity)
    {
        var method = typeof(CourseRepository).GetMethod(
            "ToCourseEventModel",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);
        _ = method!.Invoke(null, [entity]);
    }

    [Fact]
    public async Task CreateCourseAsync_ShouldAddCourseToDatabase_And_Return_Course()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new CourseRepository(context);
        var input = Course.Reconstitute(Guid.NewGuid(), $"Test Course {Guid.NewGuid():N}", "A course for testing", 5);

        var course = await repo.AddAsync(input, CancellationToken.None);

        Assert.NotNull(course);
        Assert.Equal(input.Id, course.Id);
        Assert.Equal(input.Title, course.Title);
        Assert.Equal(input.Description, course.Description);
        Assert.Equal(input.DurationInDays, course.DurationInDays);

        var persisted = await context.Courses
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == input.Id, CancellationToken.None);

        Assert.NotNull(persisted);
        Assert.Equal(input.Id, persisted!.Id);
        Assert.Equal(input.Title, persisted!.Title);
        Assert.Equal(input.Description, persisted.Description);
        Assert.Equal(input.DurationInDays, persisted.DurationInDays);
    }

    [Fact]
    public async Task GetAllCoursesAsync_ShouldReturnCreatedCourses()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new CourseRepository(context);

        await repo.AddAsync(Course.Reconstitute(Guid.NewGuid(), $"C-{Guid.NewGuid():N}", "D1", 1), CancellationToken.None);
        await repo.AddAsync(Course.Reconstitute(Guid.NewGuid(), $"C-{Guid.NewGuid():N}", "D2", 2), CancellationToken.None);

        var courses = await repo.GetAllAsync(CancellationToken.None);

        Assert.True(courses.Count >= 2);
    }

    [Fact]
    public async Task GetAllCoursesAsync_ShouldReturnDescendingByCreatedAt()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new CourseRepository(context);
        var first = await repo.AddAsync(Course.Reconstitute(Guid.NewGuid(), $"OrderA-{Guid.NewGuid():N}", "D1", 1), CancellationToken.None);
        var second = await repo.AddAsync(Course.Reconstitute(Guid.NewGuid(), $"OrderB-{Guid.NewGuid():N}", "D2", 1), CancellationToken.None);

        var firstEntity = await context.Courses.SingleAsync(x => x.Id == first.Id, CancellationToken.None);
        var secondEntity = await context.Courses.SingleAsync(x => x.Id == second.Id, CancellationToken.None);
        firstEntity.CreatedAtUtc = DateTime.UtcNow.AddMinutes(-2);
        secondEntity.CreatedAtUtc = DateTime.UtcNow;
        await context.SaveChangesAsync(CancellationToken.None);

        var courses = await repo.GetAllAsync(CancellationToken.None);
        var firstIndex = courses.ToList().FindIndex(x => x.Id == first.Id);
        var secondIndex = courses.ToList().FindIndex(x => x.Id == second.Id);

        Assert.True(firstIndex >= 0);
        Assert.True(secondIndex >= 0);
        Assert.True(secondIndex < firstIndex);
    }

    [Fact]
    public async Task GetCourseByIdAsync_ShouldReturnCourseWithEvents()
    {
        await using var context = fixture.CreateDbContext();
        var course = await RepositoryTestDataHelper.CreateCourseAsync(context);
        var eventType = await RepositoryTestDataHelper.CreateCourseEventTypeAsync(context);
        var createdEvent = await RepositoryTestDataHelper.CreateCourseEventAsync(context, course.Id, eventType.Id);
        var repo = new CourseRepository(context);

        var loaded = await repo.GetByIdWithEventsAsync(course.Id, CancellationToken.None);

        Assert.NotNull(loaded);
        Assert.Equal(course.Id, loaded!.Course.Id);
        Assert.NotEmpty(loaded.Events);
        var loadedEvent = Assert.Single(loaded.Events);
        Assert.Equal(createdEvent.Id, loadedEvent.Id);
        Assert.Equal(eventType.Id, loadedEvent.CourseEventType.Id);
        Assert.Equal(eventType.Name, loadedEvent.CourseEventType.Name);
        Assert.Equal(1, loadedEvent.VenueType.Id);
        Assert.Equal("InPerson", loadedEvent.VenueType.Name);
    }

    [Fact]
    public async Task UpdateCourseAsync_ShouldPersistChanges()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new CourseRepository(context);
        var course = await RepositoryTestDataHelper.CreateCourseAsync(context);

        var updated = await repo.UpdateAsync(
            course.Id,
            Course.Reconstitute(course.Id, "Updated Title", "Updated Description", 10),
            CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal("Updated Title", updated!.Title);

        var persisted = await context.Courses
            .AsNoTracking()
            .SingleAsync(x => x.Id == course.Id, CancellationToken.None);

        Assert.Equal("Updated Title", persisted.Title);
        Assert.Equal("Updated Description", persisted.Description);
        Assert.Equal(10, persisted.DurationInDays);
    }

    [Fact]
    public async Task HasCourseEventsAsync_ShouldReturnTrueWhenEventExists()
    {
        await using var context = fixture.CreateDbContext();
        var course = await RepositoryTestDataHelper.CreateCourseAsync(context);
        await RepositoryTestDataHelper.CreateCourseEventAsync(context, course.Id);
        var repo = new CourseRepository(context);

        var hasEvents = await repo.HasCourseEventsAsync(course.Id, CancellationToken.None);

        Assert.True(hasEvents);
    }

    [Fact]
    public async Task DeleteCourseAsync_ShouldRemoveCourse()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new CourseRepository(context);
        var course = await RepositoryTestDataHelper.CreateCourseAsync(context);

        var deleted = await repo.RemoveAsync(course.Id, CancellationToken.None);
        var loaded = await repo.GetByIdWithEventsAsync(course.Id, CancellationToken.None);

        Assert.True(deleted);
        Assert.Null(loaded);
    }

    [Fact]
    public void ToCourseEventModel_ShouldThrow_WhenCourseEventTypeIsNotLoaded()
    {
        var entity = new CourseEventEntity
        {
            Id = Guid.NewGuid(),
            CourseId = Guid.NewGuid(),
            EventDate = DateTime.UtcNow.AddDays(1),
            Price = 100m,
            Seats = 10,
            CourseEventTypeId = 1,
            VenueTypeId = 1,
            CourseEventType = null!,
            VenueType = new VenueTypeEntity { Id = 1, Name = "InPerson" }
        };

        var ex = Assert.Throws<TargetInvocationException>(() => InvokeToCourseEventModel(entity));
        Assert.IsType<InvalidOperationException>(ex.InnerException);
        Assert.Equal("Course event type must be loaded from database.", ex.InnerException!.Message);
    }

    [Fact]
    public void ToCourseEventModel_ShouldThrow_WhenVenueTypeIsNotLoaded()
    {
        var entity = new CourseEventEntity
        {
            Id = Guid.NewGuid(),
            CourseId = Guid.NewGuid(),
            EventDate = DateTime.UtcNow.AddDays(1),
            Price = 100m,
            Seats = 10,
            CourseEventTypeId = 1,
            VenueTypeId = 1,
            CourseEventType = new CourseEventTypeEntity { Id = 1, Name = "Online" },
            VenueType = null!
        };

        var ex = Assert.Throws<TargetInvocationException>(() => InvokeToCourseEventModel(entity));
        Assert.IsType<InvalidOperationException>(ex.InnerException);
        Assert.Equal("Venue type must be loaded from database.", ex.InnerException!.Message);
    }
}

