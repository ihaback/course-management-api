using Backend.Domain.Modules.Instructors.Models;
using Backend.Domain.Common.Base;

namespace Backend.Domain.Modules.Instructors.Contracts;

public interface IInstructorRepository : IRepositoryBase<Instructor, Guid>
{
    Task<bool> HasCourseEventsAsync(Guid instructorId, CancellationToken cancellationToken);
}

