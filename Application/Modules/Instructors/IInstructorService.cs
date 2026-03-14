using Backend.Application.Common;
using Backend.Application.Modules.Instructors.Inputs;
using Backend.Application.Modules.Instructors.Outputs;
using Backend.Domain.Modules.Instructors.Models;

namespace Backend.Application.Modules.Instructors;

public interface IInstructorService
{
    Task<Result<Instructor>> CreateInstructorAsync(CreateInstructorInput instructor, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<Instructor>>> GetAllInstructorsAsync(CancellationToken cancellationToken = default);
    Task<Result<InstructorDetails>> GetInstructorByIdAsync(Guid instructorId, CancellationToken cancellationToken = default);
    Task<Result<Instructor>> UpdateInstructorAsync(UpdateInstructorInput instructor, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteInstructorAsync(Guid instructorId, CancellationToken cancellationToken = default);
}
