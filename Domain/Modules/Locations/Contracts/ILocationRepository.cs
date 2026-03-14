using Backend.Domain.Modules.Locations.Models;
using Backend.Domain.Common.Base;

namespace Backend.Domain.Modules.Locations.Contracts;

public interface ILocationRepository : IRepositoryBase<Location, int>
{
    Task<bool> HasInPlaceLocationsAsync(int locationId, CancellationToken cancellationToken);
}
