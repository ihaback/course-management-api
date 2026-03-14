using Backend.Domain.Modules.CourseEvents.Models;
using Backend.Domain.Modules.VenueTypes.Models;
using Backend.Infrastructure.Persistence.EFC.Context;
using Backend.Infrastructure.Persistence.Entities;
using Backend.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Tests.Integration.Infrastructure;

[Collection(SqliteInMemoryCollection.Name)]
public class CourseEventRepository_Tests(SqliteInMemoryFixture fixture)
{
    private sealed class TestableCourseEventRepository(CoursesOnlineDbContext context)
        : CourseEventRepository(context)
    {
        public CourseEvent MapToModel(CourseEventEntity entity) => base.ToModel(entity);
    }

    [Fact]
    public async Task CreateCourseEventAsync_ShouldPersist_And_BeReadableByIdAndCourseId()
    {
        await using var context = fixture.CreateDbContext();
        var course = await RepositoryTestDataHelper.CreateCourseAsync(context);
        var type = await RepositoryTestDataHelper.CreateCourseEventTypeAsync(context);
        var repo = new CourseEventRepository(context);

        var input = CourseEvent.Reconstitute(Guid.NewGuid(), course.Id, DateTime.UtcNow.AddDays(1), 100m, 10, type.Id, VenueType.Reconstitute(1, "InPerson"));
        var created = await repo.AddAsync(input, CancellationToken.None);
        var byId = await repo.GetByIdAsync(created.Id, CancellationToken.None);
        var byCourse = await repo.GetCourseEventsByCourseIdAsync(course.Id, CancellationToken.None);

        Assert.NotNull(byId);
        Assert.Contains(byCourse, x => x.Id == created.Id);
        Assert.Equal(input.Id, created.Id);
        Assert.Equal(input.Id, byId!.Id);
        Assert.Equal(input.CourseId, byId.CourseId);
        Assert.Equal(input.CourseEventTypeId, byId.CourseEventTypeId);
        Assert.Equal(input.Seats, byId.Seats);
        Assert.Equal(input.Price, byId.Price);
        Assert.Equal(input.VenueType, byId.VenueType);
        Assert.Equal(input.CourseId, created.CourseId);
        Assert.Equal(input.CourseEventTypeId, created.CourseEventTypeId);
        Assert.Equal(input.Seats, created.Seats);
        Assert.Equal(input.Price, created.Price);
        Assert.Equal(input.VenueType, created.VenueType);

        var persisted = await context.CourseEvents
            .AsNoTracking()
            .SingleAsync(x => x.Id == created.Id, CancellationToken.None);

        Assert.Equal(input.Id, persisted.Id);
        Assert.Equal(input.CourseId, persisted.CourseId);
        Assert.Equal(input.CourseEventTypeId, persisted.CourseEventTypeId);
        Assert.Equal(input.Seats, persisted.Seats);
        Assert.Equal(input.Price.Value, persisted.Price);
        Assert.Equal(input.VenueType.Id, persisted.VenueTypeId);
    }

    [Fact]
    public async Task GetAllCourseEventsAsync_ShouldContainCreatedEvent()
    {
        await using var context = fixture.CreateDbContext();
        var created = await RepositoryTestDataHelper.CreateCourseEventAsync(context);
        var repo = new CourseEventRepository(context);

        var all = await repo.GetAllAsync(CancellationToken.None);

        Assert.Contains(all, x => x.Id == created.Id);
    }

    [Fact]
    public async Task GetAllCourseEventsAsync_ShouldReturnDescendingByCreatedAt()
    {
        await using var context = fixture.CreateDbContext();
        var first = await RepositoryTestDataHelper.CreateCourseEventAsync(context);
        var second = await RepositoryTestDataHelper.CreateCourseEventAsync(context);
        var repo = new CourseEventRepository(context);

        var firstEntity = await context.CourseEvents.SingleAsync(x => x.Id == first.Id, CancellationToken.None);
        var secondEntity = await context.CourseEvents.SingleAsync(x => x.Id == second.Id, CancellationToken.None);
        firstEntity.CreatedAtUtc = DateTime.UtcNow.AddMinutes(-2);
        secondEntity.CreatedAtUtc = DateTime.UtcNow;
        await context.SaveChangesAsync(CancellationToken.None);

        var all = await repo.GetAllAsync(CancellationToken.None);
        var firstIndex = all.ToList().FindIndex(x => x.Id == first.Id);
        var secondIndex = all.ToList().FindIndex(x => x.Id == second.Id);

        Assert.True(firstIndex >= 0);
        Assert.True(secondIndex >= 0);
        Assert.True(secondIndex < firstIndex);
    }

    [Fact]
    public async Task UpdateCourseEventAsync_ShouldPersistChanges()
    {
        await using var context = fixture.CreateDbContext();
        var courseEvent = await RepositoryTestDataHelper.CreateCourseEventAsync(context);
        var repo = new CourseEventRepository(context);

        var updated = await repo.UpdateAsync(
            courseEvent.Id,
            CourseEvent.Reconstitute(
                courseEvent.Id,
                courseEvent.CourseId,
                courseEvent.EventDate.AddDays(2),
                123m,
                15,
                courseEvent.CourseEventTypeId,
                VenueType.Reconstitute(3, "Hybrid")),
            CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal(123m, updated!.Price.Value);
        Assert.Equal(VenueType.Reconstitute(3, "Hybrid"), updated.VenueType);

        var persisted = await context.CourseEvents
            .AsNoTracking()
            .SingleAsync(x => x.Id == courseEvent.Id, CancellationToken.None);

        Assert.Equal(123m, persisted.Price);
        Assert.Equal(15, persisted.Seats);
        Assert.Equal(VenueType.Reconstitute(3, "Hybrid").Id, persisted.VenueTypeId);
    }

    [Fact]
    public async Task HasRegistrationsAsync_ShouldReturnTrueWhenRegistrationsExist()
    {
        await using var context = fixture.CreateDbContext();
        var courseEvent = await RepositoryTestDataHelper.CreateCourseEventAsync(context);
        await RepositoryTestDataHelper.CreateCourseRegistrationAsync(context, courseEventId: courseEvent.Id);
        var repo = new CourseEventRepository(context);

        var hasRegistrations = await repo.HasRegistrationsAsync(courseEvent.Id, CancellationToken.None);

        Assert.True(hasRegistrations);
    }

    [Fact]
    public async Task DeleteCourseEventAsync_ShouldRemoveEvent()
    {
        await using var context = fixture.CreateDbContext();
        var courseEvent = await RepositoryTestDataHelper.CreateCourseEventAsync(context);
        var repo = new CourseEventRepository(context);

        var deleted = await repo.RemoveAsync(courseEvent.Id, CancellationToken.None);
        var loaded = await repo.GetByIdAsync(courseEvent.Id, CancellationToken.None);

        Assert.True(deleted);
        Assert.Null(loaded);
    }

    [Fact]
    public async Task DeleteCourseEventAsync_ShouldAlsoRemoveRelations()
    {
        await using var context = fixture.CreateDbContext();
        var courseEvent = await RepositoryTestDataHelper.CreateCourseEventAsync(context);
        var participant = await RepositoryTestDataHelper.CreateParticipantAsync(context);
        await RepositoryTestDataHelper.CreateCourseRegistrationAsync(context, participantId: participant.Id, courseEventId: courseEvent.Id);
        var instructor = await RepositoryTestDataHelper.CreateInstructorAsync(context);
        await RepositoryTestDataHelper.LinkInstructorToCourseEventAsync(context, instructor.Id, courseEvent.Id);
        var inPlace = await RepositoryTestDataHelper.CreateInPlaceLocationAsync(context);
        await RepositoryTestDataHelper.LinkInPlaceLocationToCourseEventAsync(context, inPlace.Id, courseEvent.Id);

        var repo = new CourseEventRepository(context);
        var deleted = await repo.RemoveAsync(courseEvent.Id, CancellationToken.None);

        Assert.True(deleted);
        var registrations = await context.CourseRegistrations
            .AsNoTracking()
            .AnyAsync(cr => cr.CourseEventId == courseEvent.Id, CancellationToken.None);
        var instructors = await context.CourseEventInstructors
            .AsNoTracking()
            .AnyAsync(cei => cei.CourseEventId == courseEvent.Id, CancellationToken.None);
        var inPlaceLinks = await context.InPlaceEventLocations
            .AsNoTracking()
            .AnyAsync(ipl => ipl.CourseEventId == courseEvent.Id, CancellationToken.None);

        Assert.False(registrations);
        Assert.False(instructors);
        Assert.False(inPlaceLinks);
    }

    [Fact]
    public async Task GetCourseEventByIdAsync_ShouldIncludeJoinedCourseEventType()
    {
        await using var context = fixture.CreateDbContext();
        var course = await RepositoryTestDataHelper.CreateCourseAsync(context);
        var type = await new CourseEventTypeRepository(context)
            .AddAsync(Backend.Domain.Modules.CourseEventTypes.Models.CourseEventType.Create("Workshop"), CancellationToken.None);
        var repo = new CourseEventRepository(context);

        var created = await repo.AddAsync(
            CourseEvent.Reconstitute(
                Guid.NewGuid(),
                course.Id,
                DateTime.UtcNow.AddDays(2),
                249m,
                12,
                type.Id,
                VenueType.Reconstitute(2, "Online")),
            CancellationToken.None);

        var loaded = await repo.GetByIdAsync(created.Id, CancellationToken.None);

        Assert.NotNull(loaded);
        Assert.Equal(type.Id, loaded!.CourseEventType.Id);
        Assert.Equal("Workshop", loaded.CourseEventType.Name);
        Assert.Equal(VenueType.Reconstitute(2, "Online").Id, loaded.VenueType.Id);
    }

    [Fact]
    public async Task GetCourseEventByIdAsync_ShouldReturnNull_WhenCourseEventDoesNotExist()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new CourseEventRepository(context);

        var loaded = await repo.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(loaded);
    }

    [Fact]
    public async Task UpdateCourseEventAsync_ShouldThrow_WhenCourseEventDoesNotExist()
    {
        await using var context = fixture.CreateDbContext();
        var course = await RepositoryTestDataHelper.CreateCourseAsync(context);
        var type = await RepositoryTestDataHelper.CreateCourseEventTypeAsync(context);
        var repo = new CourseEventRepository(context);
        var missingId = Guid.NewGuid();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            repo.UpdateAsync(
                missingId,
                CourseEvent.Reconstitute(
                    missingId,
                    course.Id,
                    DateTime.UtcNow.AddDays(1),
                    149m,
                    20,
                    type.Id,
                    VenueType.Reconstitute(1, "InPerson")),
                CancellationToken.None));
    }

    [Fact]
    public async Task CreateCourseEventAsync_ShouldThrow_WhenForeignKeysAreInvalid()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new CourseEventRepository(context);

        var input = CourseEvent.Reconstitute(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1),
            199m,
            10,
            999_999,
            VenueType.Reconstitute(1, "InPerson"));

        await Assert.ThrowsAsync<DbUpdateException>(() => repo.AddAsync(input, CancellationToken.None));
    }

    [Fact]
    public async Task ToModel_ShouldThrow_WhenCourseEventTypeIsNotLoaded()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new TestableCourseEventRepository(context);
        var entity = new CourseEventEntity
        {
            Id = Guid.NewGuid(),
            CourseId = Guid.NewGuid(),
            EventDate = DateTime.UtcNow.AddDays(1),
            Price = 100m,
            Seats = 10,
            CourseEventTypeId = 1,
            VenueTypeId = 1,
            VenueType = new VenueTypeEntity { Id = 1, Name = "InPerson" },
            CourseEventType = null!
        };

        var ex = Assert.Throws<InvalidOperationException>(() => repo.MapToModel(entity));
        Assert.Equal("Course event type must be loaded from database.", ex.Message);
    }

    [Fact]
    public async Task ToModel_ShouldThrow_WhenVenueTypeIsNotLoaded()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new TestableCourseEventRepository(context);
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

        var ex = Assert.Throws<InvalidOperationException>(() => repo.MapToModel(entity));
        Assert.Equal("Venue type must be loaded from database.", ex.Message);
    }
}

