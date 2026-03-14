using Backend.Domain.Modules.VenueTypes.Contracts;
using Backend.Domain.Modules.VenueTypes.Models;
using Backend.Infrastructure.Persistence.EFC.Context;
using Backend.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.EFC.Repositories;

public sealed class VenueTypeRepository(CoursesOnlineDbContext context)
    : RepositoryBase<VenueType, int, VenueTypeEntity, CoursesOnlineDbContext>(context), IVenueTypeRepository
{
    protected override VenueType ToModel(VenueTypeEntity entity)
        => VenueType.Reconstitute(entity.Id, entity.Name);

    protected override VenueTypeEntity ToEntity(VenueType venueType)
        => new()
        {
            Id = venueType.Id,
            Name = venueType.Name
        };

    public override async Task<VenueType> AddAsync(VenueType venueType, CancellationToken cancellationToken)
    {
        var currentMaxId = await _context.VenueTypes
            .AsNoTracking()
            .MaxAsync(vt => (int?)vt.Id, cancellationToken);

        var entity = new VenueTypeEntity
        {
            Id = (currentMaxId ?? 0) + 1,
            Name = venueType.Name
        };

        _context.VenueTypes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return ToModel(entity);
    }

    public override async Task<IReadOnlyList<VenueType>> GetAllAsync(CancellationToken cancellationToken)
    {
        var entities = await _context.VenueTypes
            .AsNoTracking()
            .OrderByDescending(vt => vt.Id)
            .ToListAsync(cancellationToken);

        return [.. entities.Select(ToModel)];
    }

    public override async Task<VenueType?> GetByIdAsync(int venueTypeId, CancellationToken cancellationToken)
    {
        if (venueTypeId <= 0)
            throw new ArgumentException("Venue type ID must be greater than zero.", nameof(venueTypeId));

        var entity = await _context.VenueTypes
            .AsNoTracking()
            .SingleOrDefaultAsync(vt => vt.Id == venueTypeId, cancellationToken);

        return entity == null ? null : ToModel(entity);
    }

    public async Task<VenueType?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        var entity = await _context.VenueTypes
            .AsNoTracking()
            .SingleOrDefaultAsync(vt => vt.Name == name, cancellationToken);

        return entity == null ? null : ToModel(entity);
    }

    public override async Task<VenueType?> UpdateAsync(int id, VenueType venueType, CancellationToken cancellationToken)
    {
        var entity = await _context.VenueTypes
            .SingleOrDefaultAsync(vt => vt.Id == id, cancellationToken);

        if (entity == null)
            throw new KeyNotFoundException($"Venue type '{venueType.Id}' not found.");

        entity.Name = venueType.Name;
        await _context.SaveChangesAsync(cancellationToken);

        return ToModel(entity);
    }

    public override async Task<bool> RemoveAsync(int venueTypeId, CancellationToken cancellationToken)
    {
        var entity = await _context.VenueTypes
            .SingleOrDefaultAsync(vt => vt.Id == venueTypeId, cancellationToken);

        if (entity == null)
            throw new KeyNotFoundException($"Venue type '{venueTypeId}' not found.");

        _context.VenueTypes.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> IsInUseAsync(int venueTypeId, CancellationToken cancellationToken)
    {
        if (venueTypeId <= 0)
            throw new ArgumentException("Venue type ID must be greater than zero.", nameof(venueTypeId));

        return await _context.CourseEvents
            .AsNoTracking()
            .AnyAsync(ce => ce.VenueTypeId == venueTypeId, cancellationToken);
    }
}
