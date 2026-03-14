using Backend.Domain.Modules.InPlaceLocations.Contracts;
using Backend.Domain.Modules.InPlaceLocations.Models;
using Backend.Infrastructure.Persistence.Entities;
using Backend.Infrastructure.Persistence.EFC.Context;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.EFC.Repositories;

public class InPlaceLocationRepository(CoursesOnlineDbContext context)
    : RepositoryBase<InPlaceLocation, int, InPlaceLocationEntity, CoursesOnlineDbContext>(context), IInPlaceLocationRepository
{
    protected override InPlaceLocation ToModel(InPlaceLocationEntity entity)
        => InPlaceLocation.Reconstitute(entity.Id, entity.LocationId, entity.RoomNumber, entity.Seats);

    protected override InPlaceLocationEntity ToEntity(InPlaceLocation inPlaceLocation)
        => new()
        {
            Id = inPlaceLocation.Id,
            LocationId = inPlaceLocation.LocationId,
            RoomNumber = inPlaceLocation.RoomNumber,
            Seats = inPlaceLocation.Seats
        };

    public override async Task<InPlaceLocation> AddAsync(InPlaceLocation inPlaceLocation, CancellationToken cancellationToken)
    {
        var entity = ToEntity(inPlaceLocation);
        entity.Id = default;
        _context.InPlaceLocations.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return ToModel(entity);
    }

    public override async Task<bool> RemoveAsync(int inPlaceLocationId, CancellationToken cancellationToken)
    {
        var entity = await _context.InPlaceLocations.SingleOrDefaultAsync(ipl => ipl.Id == inPlaceLocationId, cancellationToken);
        if (entity == null)
            throw new KeyNotFoundException($"In-place location '{inPlaceLocationId}' not found.");

        _context.InPlaceLocations.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public override async Task<IReadOnlyList<InPlaceLocation>> GetAllAsync(CancellationToken cancellationToken)
    {
        var entities = await _context.InPlaceLocations
            .AsNoTracking()
            .OrderByDescending(ipl => ipl.Id)
            .ToListAsync(cancellationToken);

        return [.. entities.Select(ToModel)];
    }

    public override async Task<InPlaceLocation?> GetByIdAsync(int inPlaceLocationId, CancellationToken cancellationToken)
    {
        var entity = await _context.InPlaceLocations
            .AsNoTracking()
            .SingleOrDefaultAsync(ipl => ipl.Id == inPlaceLocationId, cancellationToken);

        return entity == null ? null : ToModel(entity);
    }

    public async Task<IReadOnlyList<InPlaceLocation>> GetInPlaceLocationsByLocationIdAsync(int locationId, CancellationToken cancellationToken)
    {
        var entities = await _context.InPlaceLocations
            .AsNoTracking()
            .Where(ipl => ipl.LocationId == locationId)
            .OrderBy(ipl => ipl.RoomNumber)
            .ToListAsync(cancellationToken);

        return [.. entities.Select(ToModel)];
    }

    public override async Task<InPlaceLocation?> UpdateAsync(int id, InPlaceLocation inPlaceLocation, CancellationToken cancellationToken)
    {
        var entity = await _context.InPlaceLocations.SingleOrDefaultAsync(ipl => ipl.Id == id, cancellationToken);
        if (entity is null)
            throw new KeyNotFoundException($"In-place location '{inPlaceLocation.Id}' not found.");

        entity.LocationId = inPlaceLocation.LocationId;
        entity.RoomNumber = inPlaceLocation.RoomNumber;
        entity.Seats = inPlaceLocation.Seats;

        await _context.SaveChangesAsync(cancellationToken);

        return ToModel(entity);
    }

    public async Task<bool> HasCourseEventsAsync(int inPlaceLocationId, CancellationToken cancellationToken)
    {
        return await _context.InPlaceLocations
            .AsNoTracking()
            .Where(ipl => ipl.Id == inPlaceLocationId)
            .SelectMany(ipl => ipl.CourseEvents)
            .AnyAsync(cancellationToken);
    }
}
