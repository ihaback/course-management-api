using Backend.Application.Common;
using Backend.Application.Modules.InstructorRoles.Inputs;
using Backend.Domain.Modules.InstructorRoles.Models;

namespace Backend.Application.Modules.InstructorRoles;

public interface IInstructorRoleService
{
    Task<Result<InstructorRole>> CreateInstructorRoleAsync(CreateInstructorRoleInput input, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<InstructorRole>>> GetAllInstructorRolesAsync(CancellationToken cancellationToken = default);
    Task<Result<InstructorRole>> GetInstructorRoleByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<InstructorRole>> UpdateInstructorRoleAsync(UpdateInstructorRoleInput input, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteInstructorRoleAsync(int id, CancellationToken cancellationToken = default);
}
