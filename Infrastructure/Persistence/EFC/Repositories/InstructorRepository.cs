using Backend.Domain.Modules.InstructorRoles.Models;
using Backend.Domain.Modules.Instructors.Contracts;
using Backend.Domain.Modules.Instructors.Models;
using Backend.Infrastructure.Persistence.EFC.Context;
using Backend.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.EFC.Repositories;

public class InstructorRepository(CoursesOnlineDbContext context)
    : RepositoryBase<Instructor, Guid, InstructorEntity, CoursesOnlineDbContext>(context), IInstructorRepository
{
    protected override Instructor ToModel(InstructorEntity entity)
    {
        if (entity.InstructorRole == null)
            throw new InvalidOperationException("Instructor role must be loaded from database.");

        var role = InstructorRole.Reconstitute(entity.InstructorRole.Id, entity.InstructorRole.Name);
        return Instructor.Reconstitute(entity.Id, entity.Name, role);
    }

    protected override InstructorEntity ToEntity(Instructor instructor)
        => new()
        {
            Id = instructor.Id,
            Name = instructor.Name,
            InstructorRoleId = instructor.InstructorRoleId
        };

    public override async Task<Instructor> AddAsync(Instructor instructor, CancellationToken cancellationToken)
    {
        var roleExists = await _context.InstructorRoles
            .AsNoTracking()
            .SingleOrDefaultAsync(r => r.Id == instructor.InstructorRoleId, cancellationToken);
        if (roleExists == null)
            throw new KeyNotFoundException($"Instructor role '{instructor.InstructorRoleId}' not found.");

        var entity = ToEntity(instructor);
        _context.Instructors.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        var role = InstructorRole.Reconstitute(roleExists.Id, roleExists.Name);
        return Instructor.Reconstitute(entity.Id, entity.Name, role);
    }

    public override async Task<bool> RemoveAsync(Guid instructorId, CancellationToken cancellationToken)
    {
        var entity = await _context.Instructors.SingleOrDefaultAsync(i => i.Id == instructorId, cancellationToken);
        if (entity == null)
            throw new KeyNotFoundException($"Instructor '{instructorId}' not found.");

        _context.Instructors.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public override async Task<IReadOnlyList<Instructor>> GetAllAsync(CancellationToken cancellationToken)
    {
        var entities = await _context.Instructors
            .AsNoTracking()
            .Include(i => i.InstructorRole)
            .OrderByDescending(i => i.Id)
            .ToListAsync(cancellationToken);

        return [.. entities.Select(ToModel)];
    }

    public override async Task<Instructor?> GetByIdAsync(Guid instructorId, CancellationToken cancellationToken)
    {
        var entity = await _context.Instructors
            .AsNoTracking()
            .Include(i => i.InstructorRole)
            .SingleOrDefaultAsync(i => i.Id == instructorId, cancellationToken);

        return entity == null ? null : ToModel(entity);
    }

    public override async Task<Instructor?> UpdateAsync(Guid id, Instructor instructor, CancellationToken cancellationToken)
    {
        var roleEntity = await _context.InstructorRoles
            .AsNoTracking()
            .SingleOrDefaultAsync(r => r.Id == instructor.InstructorRoleId, cancellationToken);
        if (roleEntity == null)
            throw new KeyNotFoundException($"Instructor role '{instructor.InstructorRoleId}' not found.");

        var entity = await _context.Instructors.SingleOrDefaultAsync(i => i.Id == id, cancellationToken);
        if (entity == null)
            throw new KeyNotFoundException($"Instructor '{instructor.Id}' not found.");

        entity.Name = instructor.Name;
        entity.InstructorRoleId = instructor.InstructorRoleId;
        await _context.SaveChangesAsync(cancellationToken);

        var role = InstructorRole.Reconstitute(roleEntity.Id, roleEntity.Name);
        return Instructor.Reconstitute(entity.Id, entity.Name, role);
    }

    public async Task<bool> HasCourseEventsAsync(Guid instructorId, CancellationToken cancellationToken)
    {
        return await _context.Instructors
            .AsNoTracking()
            .Where(i => i.Id == instructorId)
            .SelectMany(i => i.CourseEvents)
            .AnyAsync(cancellationToken);
    }
}
