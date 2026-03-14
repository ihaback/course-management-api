using Backend.Domain.Modules.CourseEventTypes.Contracts;
using Backend.Domain.Modules.CourseEventTypes.Models;
using Backend.Infrastructure.Persistence.EFC.Context;
using Backend.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.EFC.Repositories;

public class CourseEventTypeRepository(CoursesOnlineDbContext context)
    : RepositoryBase<CourseEventType, int, CourseEventTypeEntity, CoursesOnlineDbContext>(context), ICourseEventTypeRepository
{
    protected override CourseEventType ToModel(CourseEventTypeEntity entity)
        => CourseEventType.Reconstitute(entity.Id, entity.Name);

    protected override CourseEventTypeEntity ToEntity(CourseEventType courseEventType)
        => new()
        {
            Id = courseEventType.Id,
            Name = courseEventType.Name
        };

    public override async Task<CourseEventType> AddAsync(CourseEventType courseEventType, CancellationToken cancellationToken)
    {
        var entity = ToEntity(courseEventType);
        entity.Id = default;
        _context.CourseEventTypes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return ToModel(entity);
    }

    public override async Task<bool> RemoveAsync(int courseEventTypeId, CancellationToken cancellationToken)
    {
        var entity = await _context.CourseEventTypes.SingleOrDefaultAsync(cet => cet.Id == courseEventTypeId, cancellationToken);
        if (entity == null)
            throw new KeyNotFoundException($"Course event type '{courseEventTypeId}' not found.");

        _context.CourseEventTypes.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public override async Task<IReadOnlyList<CourseEventType>> GetAllAsync(CancellationToken cancellationToken)
    {
        var entities = await _context.CourseEventTypes
            .AsNoTracking()
            .OrderByDescending(cet => cet.Id)
            .ToListAsync(cancellationToken);

        return [.. entities.Select(ToModel)];
    }

    public override async Task<CourseEventType?> GetByIdAsync(int courseEventTypeId, CancellationToken cancellationToken)
    {
        var entity = await _context.CourseEventTypes
            .AsNoTracking()
            .SingleOrDefaultAsync(cet => cet.Id == courseEventTypeId, cancellationToken);

        return entity == null ? null : ToModel(entity);
    }

    public async Task<CourseEventType?> GetCourseEventTypeByTypeNameAsync(string typeName, CancellationToken cancellationToken)
    {
        var entity = await _context.CourseEventTypes
            .AsNoTracking()
            .SingleOrDefaultAsync(cet => cet.Name == typeName, cancellationToken);

        return entity == null ? null : ToModel(entity);
    }

    public override async Task<CourseEventType?> UpdateAsync(int id, CourseEventType courseEventType, CancellationToken cancellationToken)
    {
        var entity = await _context.CourseEventTypes.SingleOrDefaultAsync(cet => cet.Id == id, cancellationToken);
        if (entity is null)
            throw new KeyNotFoundException($"Course event type '{courseEventType.Id}' not found.");

        entity.Name = courseEventType.Name;
        await _context.SaveChangesAsync(cancellationToken);

        return ToModel(entity);
    }

    public async Task<bool> IsInUseAsync(int courseEventTypeId, CancellationToken cancellationToken)
    {
        return await _context.CourseEvents
            .AsNoTracking()
            .AnyAsync(ce => ce.CourseEventTypeId == courseEventTypeId, cancellationToken);
    }
}




