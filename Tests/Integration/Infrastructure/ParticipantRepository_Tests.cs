using Backend.Domain.Modules.Participants.Models;
using Backend.Domain.Modules.ParticipantContactTypes.Models;
using Backend.Infrastructure.Persistence.EFC.Context;
using Backend.Infrastructure.Persistence.Entities;
using Backend.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Tests.Integration.Infrastructure;

[Collection(SqliteInMemoryCollection.Name)]
public class ParticipantRepository_Tests(SqliteInMemoryFixture fixture)
{
    private sealed class TestableParticipantRepository(CoursesOnlineDbContext context)
        : ParticipantRepository(context)
    {
        public Participant MapToModel(ParticipantEntity entity) => base.ToModel(entity);
    }

    [Fact]
    public async Task CreateParticipantAsync_ShouldPersist_And_BeReadableById()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new ParticipantRepository(context);
        var input = Participant.Reconstitute(Guid.NewGuid(), "Ada", "Lovelace", $"ada-{Guid.NewGuid():N}@example.com", "123456789");

        var created = await repo.AddAsync(
            input,
            CancellationToken.None);

        var loaded = await repo.GetByIdAsync(created.Id, CancellationToken.None);

        Assert.NotNull(loaded);
        Assert.Equal(input.Id, created.Id);
        Assert.Equal(input.FirstName, created.FirstName);
        Assert.Equal(input.LastName, created.LastName);
        Assert.Equal(input.Email, created.Email);
        Assert.Equal(input.PhoneNumber, created.PhoneNumber);
        Assert.Equal(input.ContactType, created.ContactType);
        Assert.Equal(created.Email, loaded!.Email);

        var persisted = await context.Participants
            .AsNoTracking()
            .SingleAsync(x => x.Id == input.Id, CancellationToken.None);

        Assert.Equal(input.Id, persisted.Id);
        Assert.Equal(input.FirstName, persisted.FirstName);
        Assert.Equal(input.LastName, persisted.LastName);
        Assert.Equal(input.Email.Value, persisted.Email);
        Assert.Equal(input.PhoneNumber.Value, persisted.PhoneNumber);
        Assert.Equal(input.ContactType.Id, persisted.ContactTypeId);
    }

    [Fact]
    public async Task GetAllParticipantsAsync_ShouldContainCreatedParticipant()
    {
        await using var context = fixture.CreateDbContext();
        var created = await RepositoryTestDataHelper.CreateParticipantAsync(context);
        var repo = new ParticipantRepository(context);

        var all = await repo.GetAllAsync(CancellationToken.None);

        Assert.Contains(all, x => x.Id == created.Id);
    }

    [Fact]
    public async Task GetAllParticipantsAsync_ShouldReturnDescendingByCreatedAt()
    {
        await using var context = fixture.CreateDbContext();
        var first = await RepositoryTestDataHelper.CreateParticipantAsync(context);
        var second = await RepositoryTestDataHelper.CreateParticipantAsync(context);
        var repo = new ParticipantRepository(context);

        var firstEntity = await context.Participants.SingleAsync(x => x.Id == first.Id, CancellationToken.None);
        var secondEntity = await context.Participants.SingleAsync(x => x.Id == second.Id, CancellationToken.None);
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
    public async Task UpdateParticipantAsync_ShouldPersistChanges()
    {
        await using var context = fixture.CreateDbContext();
        var participant = await RepositoryTestDataHelper.CreateParticipantAsync(context);
        var repo = new ParticipantRepository(context);

        var updated = await repo.UpdateAsync(
            participant.Id,
            Participant.Reconstitute(participant.Id, "Updated", "Name", $"updated-{Guid.NewGuid():N}@example.com", "999999"),
            CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal("Updated", updated!.FirstName);

        var persisted = await context.Participants
            .AsNoTracking()
            .SingleAsync(x => x.Id == participant.Id, CancellationToken.None);

        Assert.Equal("Updated", persisted.FirstName);
        Assert.Equal("Name", persisted.LastName);
        Assert.StartsWith("updated-", persisted.Email);
    }

    [Fact]
    public async Task HasRegistrationsAsync_ShouldReturnTrueWhenParticipantHasRegistration()
    {
        await using var context = fixture.CreateDbContext();
        var participant = await RepositoryTestDataHelper.CreateParticipantAsync(context);
        await RepositoryTestDataHelper.CreateCourseRegistrationAsync(context, participantId: participant.Id);
        var repo = new ParticipantRepository(context);

        var hasRegistrations = await repo.HasRegistrationsAsync(participant.Id, CancellationToken.None);

        Assert.True(hasRegistrations);
    }

    [Fact]
    public async Task DeleteParticipantAsync_ShouldRemoveEntity()
    {
        await using var context = fixture.CreateDbContext();
        var participant = await RepositoryTestDataHelper.CreateParticipantAsync(context);
        var repo = new ParticipantRepository(context);

        var deleted = await repo.RemoveAsync(participant.Id, CancellationToken.None);
        var loaded = await repo.GetByIdAsync(participant.Id, CancellationToken.None);

        Assert.True(deleted);
        Assert.Null(loaded);
    }

    [Fact]
    public async Task DeleteParticipantAsync_ShouldAlsoDeleteCourseRegistrations()
    {
        await using var context = fixture.CreateDbContext();
        var participant = await RepositoryTestDataHelper.CreateParticipantAsync(context);
        var courseEvent = await RepositoryTestDataHelper.CreateCourseEventAsync(context);
        await RepositoryTestDataHelper.CreateCourseRegistrationAsync(
            context,
            participantId: participant.Id,
            courseEventId: courseEvent.Id);
        var repo = new ParticipantRepository(context);

        var deleted = await repo.RemoveAsync(participant.Id, CancellationToken.None);

        Assert.True(deleted);
        var remainingRegistration = await context.CourseRegistrations
            .AsNoTracking()
            .AnyAsync(cr => cr.ParticipantId == participant.Id, CancellationToken.None);
        Assert.False(remainingRegistration);
    }

    [Fact]
    public async Task GetParticipantByIdAsync_ShouldReturnContactType()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new ParticipantRepository(context);

        var created = await repo.AddAsync(
            Participant.Reconstitute(
                Guid.NewGuid(),
                "Billing",
                "Contact",
                $"billing-{Guid.NewGuid():N}@example.com",
                "555123",
                ParticipantContactType.Reconstitute(2, "Billing")),
            CancellationToken.None);

        var loaded = await repo.GetByIdAsync(created.Id, CancellationToken.None);

        Assert.NotNull(loaded);
        Assert.Equal(ParticipantContactType.Reconstitute(2, "Billing"), loaded!.ContactType);
    }

    [Fact]
    public async Task ToModel_ShouldThrow_WhenContactTypeIsNotLoaded()
    {
        await using var context = fixture.CreateDbContext();
        var repo = new TestableParticipantRepository(context);
        var entity = new ParticipantEntity
        {
            Id = Guid.NewGuid(),
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = $"ada-{Guid.NewGuid():N}@example.com",
            PhoneNumber = "123456789",
            ContactTypeId = 1,
            ContactType = null!
        };

        var ex = Assert.Throws<InvalidOperationException>(() => repo.MapToModel(entity));
        Assert.Equal("Participant contact type must be loaded from database.", ex.Message);
    }
}

