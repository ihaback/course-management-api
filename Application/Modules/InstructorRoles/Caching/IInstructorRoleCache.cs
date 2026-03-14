using Backend.Application.Common.Caching;
using Backend.Domain.Modules.InstructorRoles.Models;

namespace Backend.Application.Modules.InstructorRoles.Caching;

public interface IInstructorRoleCache : ICacheEntityBase<InstructorRole, int>
{
    Task<IReadOnlyList<InstructorRole>> GetAllAsync(Func<CancellationToken, Task<IReadOnlyList<InstructorRole>>> factory, CancellationToken ct);
    Task<InstructorRole?> GetByIdAsync(int id, Func<CancellationToken, Task<InstructorRole?>> factory, CancellationToken ct);
}
