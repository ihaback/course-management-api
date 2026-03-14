using Backend.Domain.Modules.InstructorRoles.Contracts;
using Backend.Domain.Modules.InstructorRoles.Models;
using Backend.Infrastructure.Persistence.EFC.Context;
using Backend.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.EFC.Repositories;

public class InstructorRoleRepository(CoursesOnlineDbContext context)
    : RepositoryBase<InstructorRole, int, InstructorRoleEntity, CoursesOnlineDbContext>(context), IInstructorRoleRepository
{
    protected override InstructorRole ToModel(InstructorRoleEntity entity) =>
        InstructorRole.Reconstitute(entity.Id, entity.Name);

    protected override InstructorRoleEntity ToEntity(InstructorRole role)
        => new()
        {
            Id = role.Id,
            Name = role.Name
        };

    public override async Task<InstructorRole> AddAsync(InstructorRole role, CancellationToken cancellationToken)
    {
        var entity = ToEntity(role);
        entity.Id = default;
        _context.InstructorRoles.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return ToModel(entity);
    }

    public override async Task<IReadOnlyList<InstructorRole>> GetAllAsync(CancellationToken cancellationToken)
    {
        var entities = await _context.InstructorRoles
            .AsNoTracking()
            .OrderByDescending(r => r.Id)
            .ToListAsync(cancellationToken);

        return [.. entities.Select(ToModel)];
    }

    public override async Task<InstructorRole?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await _context.InstructorRoles
            .AsNoTracking()
            .SingleOrDefaultAsync(r => r.Id == id, cancellationToken);

        return entity == null ? null : ToModel(entity);
    }

    public override async Task<InstructorRole?> UpdateAsync(int id, InstructorRole role, CancellationToken cancellationToken)
    {
        var entity = await _context.InstructorRoles
            .SingleOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (entity == null)
            return null;

        entity.Name = role.Name;
        await _context.SaveChangesAsync(cancellationToken);

        return ToModel(entity);
    }

    public override async Task<bool> RemoveAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await _context.InstructorRoles.SingleOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (entity == null)
            return false;

        _context.InstructorRoles.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

}
