using Backend.Domain.Common.Base;
using Backend.Domain.Modules.Courses.Models;

namespace Backend.Domain.Modules.Courses.Contracts;

public interface ICourseRepository : IRepositoryBase<Course, Guid>
{
    Task<CourseWithEvents?> GetByIdWithEventsAsync(Guid courseId, CancellationToken cancellationToken);
    Task<bool> HasCourseEventsAsync(Guid courseId, CancellationToken cancellationToken);
}
