using Backend.Domain.Modules.Participants.Contracts;
using Backend.Domain.Modules.Participants.Models;
using Backend.Domain.Modules.ParticipantContactTypes.Models;
using Backend.Infrastructure.Persistence.EFC.Context;
using Backend.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.EFC.Repositories;

public class ParticipantRepository(CoursesOnlineDbContext context)
    : RepositoryBase<Participant, Guid, ParticipantEntity, CoursesOnlineDbContext>(context), IParticipantRepository
{
    protected override Participant ToModel(ParticipantEntity entity)
    {
        var contactTypeEntity = entity.ContactType
            ?? throw new InvalidOperationException("Participant contact type must be loaded from database.");

        var contactType = ParticipantContactType.Reconstitute(contactTypeEntity.Id, contactTypeEntity.Name);

        return Participant.Reconstitute(
            entity.Id,
            entity.FirstName,
            entity.LastName,
            entity.Email,
            entity.PhoneNumber,
            contactType);
    }

    protected override ParticipantEntity ToEntity(Participant participant)
    {
        var contactType = participant.ContactType
            ?? throw new InvalidOperationException("Participant contact type must be set when mapping participant.");

        return new ParticipantEntity
        {
            Id = participant.Id,
            FirstName = participant.FirstName,
            LastName = participant.LastName,
            Email = participant.Email.Value,
            PhoneNumber = participant.PhoneNumber.Value,
            ContactTypeId = contactType.Id
        };
    }

    public override async Task<Participant> AddAsync(Participant participant, CancellationToken cancellationToken)
    {
        var entity = ToEntity(participant);

        _context.Participants.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        var created = await _context.Participants
            .AsNoTracking()
            .Include(p => p.ContactType)
            .SingleAsync(p => p.Id == entity.Id, cancellationToken);

        return ToModel(created);
    }

    public override async Task<bool> RemoveAsync(Guid participantId, CancellationToken cancellationToken)
    {
        using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var exists = await _context.Participants
                .AnyAsync(p => p.Id == participantId, cancellationToken);

            if (!exists)
                throw new KeyNotFoundException($"Participant '{participantId}' not found.");

            await _context.Database.ExecuteSqlAsync(
                $"DELETE FROM CourseRegistrations WHERE ParticipantId = {participantId}",
                cancellationToken);

            await _context.Database.ExecuteSqlAsync(
                $"DELETE FROM Participants WHERE Id = {participantId}",
                cancellationToken);

            await tx.CommitAsync(cancellationToken);
            return true;
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public override async Task<IReadOnlyList<Participant>> GetAllAsync(CancellationToken cancellationToken)
    {
        var entities = await _context.Participants
            .AsNoTracking()
            .Include(p => p.ContactType)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ThenByDescending(p => p.Id)
            .ToListAsync(cancellationToken);

        return [.. entities.Select(ToModel)];
    }

    public override async Task<Participant?> GetByIdAsync(Guid participantId, CancellationToken cancellationToken)
    {
        var entity = await _context.Participants
            .AsNoTracking()
            .Include(p => p.ContactType)
            .SingleOrDefaultAsync(p => p.Id == participantId, cancellationToken);

        return entity == null ? null : ToModel(entity);
    }

    public override async Task<Participant?> UpdateAsync(Guid id, Participant participant, CancellationToken cancellationToken)
    {
        var entity = await _context.Participants.SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (entity is null)
            throw new KeyNotFoundException($"Participant '{participant.Id}' not found.");

        entity.FirstName = participant.FirstName;
        entity.LastName = participant.LastName;
        entity.Email = participant.Email.Value;
        entity.PhoneNumber = participant.PhoneNumber.Value;
        entity.ContactTypeId = participant.ContactType.Id;
        entity.ModifiedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var updated = await _context.Participants
            .AsNoTracking()
            .Include(p => p.ContactType)
            .SingleAsync(p => p.Id == id, cancellationToken);

        return ToModel(updated);
    }

    public async Task<bool> HasRegistrationsAsync(Guid participantId, CancellationToken cancellationToken)
    {
        return await _context.CourseRegistrations
            .AsNoTracking()
            .AnyAsync(cr => cr.ParticipantId == participantId, cancellationToken);
    }
}
