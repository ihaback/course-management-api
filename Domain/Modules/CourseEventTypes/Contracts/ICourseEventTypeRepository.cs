using Backend.Domain.Modules.CourseEventTypes.Models;
using Backend.Domain.Common.Base;

namespace Backend.Domain.Modules.CourseEventTypes.Contracts;

public interface ICourseEventTypeRepository : IRepositoryBase<CourseEventType, int>
{
    Task<CourseEventType?> GetCourseEventTypeByTypeNameAsync(string typeName, CancellationToken cancellationToken);
    Task<bool> IsInUseAsync(int courseEventTypeId, CancellationToken cancellationToken);
}
