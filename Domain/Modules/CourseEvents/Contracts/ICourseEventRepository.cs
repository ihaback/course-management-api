using Backend.Domain.Modules.CourseEvents.Models;
using Backend.Domain.Common.Base;

namespace Backend.Domain.Modules.CourseEvents.Contracts
{
    public interface ICourseEventRepository : IRepositoryBase<CourseEvent, Guid>
    {
        Task<IReadOnlyList<CourseEvent>> GetCourseEventsByCourseIdAsync(Guid courseId, CancellationToken cancellationToken);
        Task<bool> HasRegistrationsAsync(Guid courseEventId, CancellationToken cancellationToken);
    }
}

