using Backend.Application.Common.Caching;
using Backend.Domain.Modules.InstructorRoles.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Backend.Application.Modules.InstructorRoles.Caching;

public sealed class InstructorRoleCache(IMemoryCache cache) : CacheEntityBase<InstructorRole, int>(cache), IInstructorRoleCache
{
    protected override int GetId(InstructorRole entity) => entity.Id;

    public Task<InstructorRole?> GetByIdAsync(int id, Func<CancellationToken, Task<InstructorRole?>> factory, CancellationToken ct)
        => GetOrCreateByIdAsync(id, factory, ct);

    public Task<IReadOnlyList<InstructorRole>> GetAllAsync(Func<CancellationToken, Task<IReadOnlyList<InstructorRole>>> factory, CancellationToken ct)
        => GetOrCreateAllAsync(factory, ct);
}
