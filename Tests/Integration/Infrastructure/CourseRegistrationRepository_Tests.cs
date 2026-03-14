using Backend.Domain.Modules.CourseRegistrations.Models;
using Backend.Domain.Modules.CourseRegistrationStatuses.Models;
using Backend.Domain.Modules.PaymentMethods.Models;
using Backend.Infrastructure.Persistence.EFC.Context;
using Backend.Infrastructure.Persistence.Entities;
using Backend.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Tests.Integration.Infrastructure;

[Collection(SqliteInMemoryCollection.Name)]
public class CourseRegistrationRepository_Tests(SqliteInMemoryFixture fixture)
{
    private sealed class TestableCourseRegistrationRepository(CoursesOnlineDbContext context)
        : CourseRegistrationRepository(context)
    {
        public CourseRegistration MapToModel(CourseRegistrationEntity entity) => base.ToModel(entity);
    }

    [Fact]
    public async Task CreateCourseRegistrationAsync_ShouldPersist_And_BeReadableById()
    {
        await using var context = fixture.CreateDbContext();
        var participant = await RepositoryTestDataHelper.CreateParticipantAsync(context);
        var courseEvent = await RepositoryTestDataHelper.CreateCourseEventAsync(context);
        var repo = new CourseRegistrationRepository(context);

        var input = CourseRegistration.Reconstitute(
            Guid.NewGuid(),
            participant.Id,
            courseEvent.Id,
            DateTime.UtcNow,
            CourseRegistrationStatus.Pending,
            PaymentMethod.Reconstitute(1, "Card"));

        var created = await repo.AddAsync(input, CancellationToken.None);
        var byId = await repo.GetByIdAsync(created.Id, CancellationToken.None);

        Assert.NotNull(byId);
        Assert.Equal(input.Id, created.Id);
        Assert.Equal(participant.Id, byId!.ParticipantId);
        Assert.Equal(courseEvent.Id, byId.CourseEventId);
        Assert.Equal(CourseRegistrationStatus.Pending.Id, byId.Status.Id);
        Assert.Equal(PaymentMethod.Reconstitute(1, "Card"), byId.PaymentMethod);

        var persisted = await context.CourseRegistrations
            .AsNoTracking()
            .SingleAsync(x => x.Id == created.Id, CancellationToken.None);

        Assert.Equal(input.Id, persisted.Id);
        Assert.Equal(participant.Id, persisted.ParticipantId);
        Assert.Equal(courseEvent.Id, persisted.CourseEventId);
        Assert.Equal(CourseRegistrationStatus.Pending.Id, persisted.CourseRegistrationStatusId);
        Assert.Equal(PaymentMethod.Reconstitute(1, "Card").Id, persisted.PaymentMethodId);
    }

    [Fact]
    public async Task CreateRegistrationWithSeatCheckAsync_ShouldReturnNullWhenNoSeats()
    {
        await using var context = fixture.CreateDbContext();
        var participant = await RepositoryTestDataHelper.CreateParticipantAsync(context);
        var courseEvent = await RepositoryTestDataHelper.CreateCourseEventAsync(context, seats: 1);
        await RepositoryTestDataHelper.CreateCourseRegistrationAsync(context, participantId: participant.Id, courseEventId: courseEvent.Id);
        var secondParticipant = await RepositoryTestDataHelper.CreateParticipantAsync(context);
        var repo = new CourseRegistrationRepository(context);

        var second = await repo.CreateRegistrationWithSeatCheckAsync(
            CourseRegistration.Reconstitute(
                Guid.NewGuid(),
                secondParticipant.Id,
                courseEvent.Id,
                DateTime.UtcNow,
                CourseRegistrationStatus.Pending,
                PaymentMethod.Reconstitute(1, "Card")),
            CancellationToken.None);

        Assert.Null(second);

        var registrationCount = await context.CourseRegistrations
            .AsNoTracking()
            .CountAsync(x => x.CourseEventId == courseEvent.Id, CancellationToken.None);

        Assert.Equal(1, registrationCount);
    }

    [Fact]
    public async Task GetAllCourseRegistrationsAsync_ShouldContainCreatedRegistration()
    {
        await using var context = fixture.CreateDbContext();
        var created = await RepositoryTestDataHelper.CreateCourseRegistrationAsync(context);
        var repo = new CourseRegistrationRepository(context);

        var all = await repo.GetAllAsync(CancellationToken.None);

        Assert.Contains(all, x => x.Id == created.Id);
    }

    [Fact]
    public async Task GetAllCourseRegistrationsAsync_ShouldReturnDescendingByRegistrationDate()
    {
        await using var context = fixture.CreateDbContext();
        var participant = await RepositoryTestDataHelper.CreateParticipantAsync(context);
        var secondParticipant = await RepositoryTestDataHelper.CreateParticipantAsync(context);
        var courseEvent = await RepositoryTestDataHelper.CreateCourseEventAsync(context);
        var repo = new CourseRegistrationRepository(context);

        var first = await repo.AddAsync(
            CourseRegistration.Reconstitute(
                Guid.NewGuid(),
                participant.Id,
                courseEvent.Id,
                DateTime.UtcNow.AddMinutes(-5),
                CourseRegistrationStatus.Pending,
                PaymentMethod.Reconstitute(1, "Card")),
            CancellationToken.None);

        var second = await repo.AddAsync(
            CourseRegistration.Reconstitute(
                Guid.NewGuid(),
                secondParticipant.Id,
                courseEvent.Id,
                DateTime.UtcNow,
                CourseRegistrationStatus.Pending,
                PaymentMethod.Reconstitute(1, "Card")),
            CancellationToken.None);

        var firstEntity = await context.CourseRegistrations.SingleAsync(x => x.Id == first.Id, CancellationToken.None);
        var secondEntity = await context.CourseRegistrations.SingleAsync(x => x.Id == second.Id, CancellationToken.None);
        firstEntity.RegistrationDate = DateTime.UtcNow.AddMinutes(-2);
        secondEntity.RegistrationDate = DateTime.UtcNow;
        await context.SaveChangesAsync(CancellationToken.None);

        var all = await repo.GetAllAsync(CancellationToken.None);
        var firstIndex = all.ToList().FindIndex(x => x.Id == first.Id);
        var secondIndex = all.ToList().FindIndex(x => x.Id == second.Id);

        Assert.True(firstIndex >= 0);
        Assert.True(secondIndex >= 0);
        Assert.True(secondIndex < firstIndex);
    }

    [Fact]
    public async Task GetCourseRegistrationsByParticipantIdAsync_ShouldReturnParticipantRegistrations()
    {
        await using var context = fixture.CreateDbContext();
        var participant = await RepositoryTestDataHelper.CreateParticipantAsync(context);
        var created = await RepositoryTestDataHelper.CreateCourseRegistrationAsync(context, participantId: participant.Id);
        var repo = new CourseRegistrationRepository(context);

        var byParticipant = await repo.GetCourseRegistrationsByParticipantIdAsync(participant.Id, CancellationToken.None);

        Assert.Contains(byParticipant, x => x.Id == created.Id);
    }

    [Fact]
    public async Task GetCourseRegistrationsByCourseEventIdAsync_ShouldReturnEventRegistrations()
    {
        await using var context = fixture.CreateDbContext();
        var courseEvent = await RepositoryTestDataHelper.CreateCourseEventAsync(context);
        var created = await RepositoryTestDataHelper.CreateCourseRegistrationAsync(context, courseEventId: courseEvent.Id);
        var repo = new CourseRegistrationRepository(context);

        var byCourseEvent = await repo.GetCourseRegistrationsByCourseEventIdAsync(courseEvent.Id, CancellationToken.None);

        Assert.Contains(byCourseEvent, x => x.Id == created.Id);
    }

    [Fact]
    public async Task UpdateCourseRegistrationAsync_ShouldPersistChanges()
    {
        await using var context = fixture.CreateDbContext();
        var participant = await RepositoryTestDataHelper.CreateParticipantAsync(context);
        var courseEvent = await RepositoryTestDataHelper.CreateCourseEventAsync(context);
        var created = await RepositoryTestDataHelper.CreateCourseRegistrationAsync(context, participant.Id, courseEvent.Id);
        var repo = new CourseRegistrationRepository(context);

        var updated = await repo.UpdateAsync(
            created.Id,
            CourseRegistration.Reconstitute(
                created.Id,
                created.ParticipantId,
                created.CourseEventId,
                created.RegistrationDate,
                CourseRegistrationStatus.Paid,
                PaymentMethod.Reconstitute(2, "Invoice")),
            CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal(CourseRegistrationStatus.Paid.Id, updated!.Status.Id);

        var persisted = await context.CourseRegistrations
            .AsNoTracking()
            .SingleAsync(x => x.Id == created.Id, CancellationToken.None);

        Assert.Equal(CourseRegistrationStatus.Paid.Id, persisted.CourseRegistrationStatusId);
        Assert.Equal(PaymentMethod.Reconstitute(2, "Invoice").Id, persisted.PaymentMethodId);
    }

    [Fact]
    public async Task DeleteCourseRegistrationAsync_ShouldRemoveRegistration()
    {
        await using var context = fixture.CreateDbContext();
        var created = await RepositoryTestDataHelper.CreateCourseRegistrationAsync(context);
        var repo = new CourseRegistrationRepository(context);

        var deleted = await repo.RemoveAsync(created.Id, CancellationToken.None);
        var loaded = await repo.GetByIdAsync(created.Id, CancellationToken.None);

        Assert.True(deleted);
        Assert.Null(loaded);
    }

    [Fact]
    public async Task GetCourseRegistrationByIdAsync_ShouldIncludeJoinedStatusName()
    {
        await using var context = fixture.CreateDbContext();
        var created = await RepositoryTestDataHelper.CreateCourseRegistrationAsync(context, status: CourseRegistrationStatus.Paid);
        var repo = new CourseRegistrationRepository(context);

        var loaded = await repo.GetByIdAsync(created.Id, CancellationToken.None);

        Assert.NotNull(loaded);
        Assert.Equal(CourseRegistrationStatus.Paid.Id, loaded!.Status.Id);
        Assert.Equal("Paid", loaded.Status.Name);
        Assert.Equal(created.ParticipantId, loaded.ParticipantId);
        Assert.Equal(created.CourseEventId, loaded.CourseEventId);
    }

    [Fact]
    public async Task CreateCourseRegistrationAsync_ShouldThrow_WhenForeignKeysAreInvalid()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new CourseRegistrationRepository(context);
        var existingEvent = await RepositoryTestDataHelper.CreateCourseEventAsync(context);

        var input = CourseRegistration.Reconstitute(
            Guid.NewGuid(),
            Guid.NewGuid(),
            existingEvent.Id,
            DateTime.UtcNow,
            CourseRegistrationStatus.Pending,
            PaymentMethod.Reconstitute(1, "Card"));

        await Assert.ThrowsAsync<DbUpdateException>(() => repo.AddAsync(input, CancellationToken.None));
    }

    [Fact]
    public async Task GetCourseRegistrationByIdAsync_ShouldReturnNull_WhenRegistrationDoesNotExist()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new CourseRegistrationRepository(context);

        var loaded = await repo.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(loaded);
    }

    [Fact]
    public async Task DeleteCourseRegistrationAsync_ShouldThrow_WhenRegistrationDoesNotExist()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new CourseRegistrationRepository(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => repo.RemoveAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task ToModel_ShouldThrow_WhenStatusIsNotLoaded()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new TestableCourseRegistrationRepository(context);
        var entity = new CourseRegistrationEntity
        {
            Id = Guid.NewGuid(),
            ParticipantId = Guid.NewGuid(),
            CourseEventId = Guid.NewGuid(),
            RegistrationDate = DateTime.UtcNow,
            CourseRegistrationStatusId = 1,
            PaymentMethodId = 1,
            PaymentMethod = new PaymentMethodEntity { Id = 1, Name = "Card" },
            CourseRegistrationStatus = null!
        };

        var ex = Assert.Throws<InvalidOperationException>(() => repo.MapToModel(entity));
        Assert.Equal("Course registration status must be loaded from database.", ex.Message);
    }

    [Fact]
    public async Task ToModel_ShouldThrow_WhenPaymentMethodIsNotLoaded()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new TestableCourseRegistrationRepository(context);
        var entity = new CourseRegistrationEntity
        {
            Id = Guid.NewGuid(),
            ParticipantId = Guid.NewGuid(),
            CourseEventId = Guid.NewGuid(),
            RegistrationDate = DateTime.UtcNow,
            CourseRegistrationStatusId = 1,
            PaymentMethodId = 1,
            CourseRegistrationStatus = new CourseRegistrationStatusEntity { Id = 1, Name = "Paid" },
            PaymentMethod = null!
        };

        var ex = Assert.Throws<InvalidOperationException>(() => repo.MapToModel(entity));
        Assert.Equal("Payment method must be loaded from database.", ex.Message);
    }
}

