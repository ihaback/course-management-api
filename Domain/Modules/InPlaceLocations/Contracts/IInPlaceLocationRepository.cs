using Backend.Domain.Modules.InPlaceLocations.Models;
using Backend.Domain.Common.Base;

namespace Backend.Domain.Modules.InPlaceLocations.Contracts;

public interface IInPlaceLocationRepository : IRepositoryBase<InPlaceLocation, int>
{
    Task<IReadOnlyList<InPlaceLocation>> GetInPlaceLocationsByLocationIdAsync(int locationId, CancellationToken cancellationToken);
    Task<bool> HasCourseEventsAsync(int inPlaceLocationId, CancellationToken cancellationToken);
}
