using Backend.Domain.Modules.CourseRegistrationStatuses.Contracts;
using Backend.Domain.Modules.CourseRegistrationStatuses.Models;
using Backend.Infrastructure.Persistence.EFC.Context;
using Backend.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.EFC.Repositories;

public sealed class CourseRegistrationStatusRepository(
    CoursesOnlineDbContext context)
    : RepositoryBase<CourseRegistrationStatus, int, CourseRegistrationStatusEntity, CoursesOnlineDbContext>(context), ICourseRegistrationStatusRepository
{
    protected override CourseRegistrationStatus ToModel(CourseRegistrationStatusEntity entity)
        => CourseRegistrationStatus.Reconstitute(entity.Id, entity.Name);

    protected override CourseRegistrationStatusEntity ToEntity(CourseRegistrationStatus status)
        => new()
        {
            Id = status.Id,
            Name = status.Name
        };

    public override async Task<CourseRegistrationStatus> AddAsync(CourseRegistrationStatus status, CancellationToken cancellationToken)
    {
        var currentMaxId = await _context.CourseRegistrationStatuses
            .AsNoTracking()
            .MaxAsync(s => (int?)s.Id, cancellationToken);

        var entity = new CourseRegistrationStatusEntity
        {
            Id = (currentMaxId ?? -1) + 1,
            Name = status.Name
        };

        _context.CourseRegistrationStatuses.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return ToModel(entity);
    }

    public override async Task<IReadOnlyList<CourseRegistrationStatus>> GetAllAsync(CancellationToken cancellationToken)
    {
        var entities = await _context.CourseRegistrationStatuses
            .AsNoTracking()
            .OrderByDescending(s => s.Id)
            .ToListAsync(cancellationToken);

        return [.. entities.Select(ToModel)];
    }

    public override async Task<CourseRegistrationStatus?> GetByIdAsync(int statusId, CancellationToken cancellationToken)
    {
        if (statusId < 0)
            throw new ArgumentException("Status ID must be zero or positive.", nameof(statusId));

        var entity = await _context.CourseRegistrationStatuses
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Id == statusId, cancellationToken);

        return entity == null ? null : ToModel(entity);
    }

    public async Task<CourseRegistrationStatus?> GetCourseRegistrationStatusByNameAsync(string name, CancellationToken cancellationToken)
    {
        var entity = await _context.CourseRegistrationStatuses
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Name == name, cancellationToken);

        return entity == null ? null : ToModel(entity);
    }

    public override async Task<CourseRegistrationStatus?> UpdateAsync(int id, CourseRegistrationStatus status, CancellationToken cancellationToken)
    {
        var entity = await _context.CourseRegistrationStatuses
            .SingleOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (entity == null)
            throw new KeyNotFoundException($"Course registration status '{status.Id}' not found.");

        entity.Name = status.Name;
        await _context.SaveChangesAsync(cancellationToken);

        return ToModel(entity);
    }

    public override async Task<bool> RemoveAsync(int statusId, CancellationToken cancellationToken)
    {
        var entity = await _context.CourseRegistrationStatuses
            .SingleOrDefaultAsync(s => s.Id == statusId, cancellationToken);

        if (entity == null)
            throw new KeyNotFoundException($"Course registration status '{statusId}' not found.");

        _context.CourseRegistrationStatuses.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> IsInUseAsync(int statusId, CancellationToken cancellationToken)
    {
        if (statusId < 0)
            throw new ArgumentException("Status ID must be zero or positive.", nameof(statusId));

        return await _context.CourseRegistrations
            .AsNoTracking()
            .AnyAsync(cr => cr.CourseRegistrationStatusId == statusId, cancellationToken);
    }

}
