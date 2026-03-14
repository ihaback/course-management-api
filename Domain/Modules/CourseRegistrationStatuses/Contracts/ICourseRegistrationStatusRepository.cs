using Backend.Domain.Modules.CourseRegistrationStatuses.Models;
using Backend.Domain.Common.Base;

namespace Backend.Domain.Modules.CourseRegistrationStatuses.Contracts;

public interface ICourseRegistrationStatusRepository : IRepositoryBase<CourseRegistrationStatus, int>
{
    Task<CourseRegistrationStatus?> GetCourseRegistrationStatusByNameAsync(string name, CancellationToken cancellationToken);
    Task<bool> IsInUseAsync(int statusId, CancellationToken cancellationToken);
}
