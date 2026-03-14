using Backend.Domain.Modules.Locations.Contracts;
using Backend.Domain.Modules.Locations.Models;
using Backend.Infrastructure.Persistence.Entities;
using Backend.Infrastructure.Persistence.EFC.Context;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.EFC.Repositories;

public class LocationRepository(CoursesOnlineDbContext context)
    : RepositoryBase<Location, int, LocationEntity, CoursesOnlineDbContext>(context), ILocationRepository
{
    protected override Location ToModel(LocationEntity entity)
        => Location.Reconstitute(entity.Id, entity.StreetName, entity.PostalCode, entity.City);

    protected override LocationEntity ToEntity(Location location)
        => new()
        {
            Id = location.Id,
            StreetName = location.StreetName,
            PostalCode = location.PostalCode,
            City = location.City
        };

    public override async Task<Location> AddAsync(Location location, CancellationToken cancellationToken)
    {
        var entity = ToEntity(location);
        entity.Id = default;
        _context.Locations.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return ToModel(entity);
    }

    public override async Task<bool> RemoveAsync(int locationId, CancellationToken cancellationToken)
    {
        var entity = await _context.Locations.SingleOrDefaultAsync(l => l.Id == locationId, cancellationToken);
        if (entity == null)
            throw new KeyNotFoundException($"Location '{locationId}' not found.");

        _context.Locations.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public override async Task<IReadOnlyList<Location>> GetAllAsync(CancellationToken cancellationToken)
    {
        var entities = await _context.Locations
            .AsNoTracking()
            .OrderByDescending(l => l.Id)
            .ToListAsync(cancellationToken);

        return [.. entities.Select(ToModel)];
    }

    public override async Task<Location?> GetByIdAsync(int locationId, CancellationToken cancellationToken)
    {
        var entity = await _context.Locations
            .AsNoTracking()
            .SingleOrDefaultAsync(l => l.Id == locationId, cancellationToken);

        return entity == null ? null : ToModel(entity);
    }

    public override async Task<Location?> UpdateAsync(int id, Location location, CancellationToken cancellationToken)
    {
        var entity = await _context.Locations.SingleOrDefaultAsync(l => l.Id == id, cancellationToken);
        if (entity is null)
            throw new KeyNotFoundException($"Location '{location.Id}' not found.");

        entity.StreetName = location.StreetName;
        entity.PostalCode = location.PostalCode;
        entity.City = location.City;

        await _context.SaveChangesAsync(cancellationToken);

        return ToModel(entity);
    }

    public async Task<bool> HasInPlaceLocationsAsync(int locationId, CancellationToken cancellationToken)
    {
        return await _context.InPlaceLocations
            .AsNoTracking()
            .AnyAsync(ipl => ipl.LocationId == locationId, cancellationToken);
    }
}
