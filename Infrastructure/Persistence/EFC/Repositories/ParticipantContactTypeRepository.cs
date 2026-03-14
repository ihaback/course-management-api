using Backend.Domain.Modules.ParticipantContactTypes.Contracts;
using Backend.Domain.Modules.ParticipantContactTypes.Models;
using Backend.Infrastructure.Persistence.EFC.Context;
using Backend.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.EFC.Repositories;

public sealed class ParticipantContactTypeRepository(CoursesOnlineDbContext context)
    : RepositoryBase<ParticipantContactType, int, ParticipantContactTypeEntity, CoursesOnlineDbContext>(context), IParticipantContactTypeRepository
{
    protected override ParticipantContactType ToModel(ParticipantContactTypeEntity entity)
        => ParticipantContactType.Reconstitute(entity.Id, entity.Name);

    protected override ParticipantContactTypeEntity ToEntity(ParticipantContactType participantContactType)
        => new()
        {
            Id = participantContactType.Id,
            Name = participantContactType.Name
        };

    public override async Task<ParticipantContactType> AddAsync(ParticipantContactType participantContactType, CancellationToken cancellationToken)
    {
        var currentMaxId = await _context.ParticipantContactTypes
            .AsNoTracking()
            .MaxAsync(pct => (int?)pct.Id, cancellationToken);

        var entity = new ParticipantContactTypeEntity
        {
            Id = (currentMaxId ?? 0) + 1,
            Name = participantContactType.Name
        };

        _context.ParticipantContactTypes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return ToModel(entity);
    }

    public override async Task<IReadOnlyList<ParticipantContactType>> GetAllAsync(CancellationToken cancellationToken)
    {
        var entities = await _context.ParticipantContactTypes
            .AsNoTracking()
            .OrderByDescending(pct => pct.Id)
            .ToListAsync(cancellationToken);

        return [.. entities.Select(ToModel)];
    }

    public override async Task<ParticipantContactType?> GetByIdAsync(int participantContactTypeId, CancellationToken cancellationToken)
    {
        if (participantContactTypeId <= 0)
            throw new ArgumentException("Participant contact type ID must be greater than zero.", nameof(participantContactTypeId));

        var entity = await _context.ParticipantContactTypes
            .AsNoTracking()
            .SingleOrDefaultAsync(pct => pct.Id == participantContactTypeId, cancellationToken);

        return entity == null ? null : ToModel(entity);
    }

    public async Task<ParticipantContactType?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        var entity = await _context.ParticipantContactTypes
            .AsNoTracking()
            .SingleOrDefaultAsync(pct => pct.Name == name, cancellationToken);

        return entity == null ? null : ToModel(entity);
    }

    public override async Task<ParticipantContactType?> UpdateAsync(int id, ParticipantContactType participantContactType, CancellationToken cancellationToken)
    {
        var entity = await _context.ParticipantContactTypes
            .SingleOrDefaultAsync(pct => pct.Id == id, cancellationToken);

        if (entity == null)
            throw new KeyNotFoundException($"Participant contact type '{participantContactType.Id}' not found.");

        entity.Name = participantContactType.Name;
        await _context.SaveChangesAsync(cancellationToken);

        return ToModel(entity);
    }

    public override async Task<bool> RemoveAsync(int participantContactTypeId, CancellationToken cancellationToken)
    {
        var entity = await _context.ParticipantContactTypes
            .SingleOrDefaultAsync(pct => pct.Id == participantContactTypeId, cancellationToken);

        if (entity == null)
            throw new KeyNotFoundException($"Participant contact type '{participantContactTypeId}' not found.");

        _context.ParticipantContactTypes.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> IsInUseAsync(int participantContactTypeId, CancellationToken cancellationToken)
    {
        if (participantContactTypeId <= 0)
            throw new ArgumentException("Participant contact type ID must be greater than zero.", nameof(participantContactTypeId));

        return await _context.Participants
            .AsNoTracking()
            .AnyAsync(p => p.ContactTypeId == participantContactTypeId, cancellationToken);
    }
}
