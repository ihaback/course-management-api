using Backend.Application.Common;
using Backend.Application.Modules.Locations.Inputs;
using Backend.Domain.Modules.Locations.Models;

namespace Backend.Application.Modules.Locations;

public interface ILocationService
{
    Task<Result<Location>> CreateLocationAsync(CreateLocationInput location, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<Location>>> GetAllLocationsAsync(CancellationToken cancellationToken = default);
    Task<Result<Location>> GetLocationByIdAsync(int locationId, CancellationToken cancellationToken = default);
    Task<Result<Location>> UpdateLocationAsync(UpdateLocationInput location, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteLocationAsync(int locationId, CancellationToken cancellationToken = default);
}
