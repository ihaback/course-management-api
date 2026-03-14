using Backend.Application.Common;
using Backend.Application.Modules.InPlaceLocations.Inputs;
using Backend.Domain.Modules.InPlaceLocations.Models;

namespace Backend.Application.Modules.InPlaceLocations;

public interface IInPlaceLocationService
{
    Task<Result<InPlaceLocation>> CreateInPlaceLocationAsync(CreateInPlaceLocationInput inPlaceLocation, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<InPlaceLocation>>> GetAllInPlaceLocationsAsync(CancellationToken cancellationToken = default);
    Task<Result<InPlaceLocation>> GetInPlaceLocationByIdAsync(int inPlaceLocationId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<InPlaceLocation>>> GetInPlaceLocationsByLocationIdAsync(int locationId, CancellationToken cancellationToken = default);
    Task<Result<InPlaceLocation>> UpdateInPlaceLocationAsync(UpdateInPlaceLocationInput inPlaceLocation, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteInPlaceLocationAsync(int inPlaceLocationId, CancellationToken cancellationToken = default);
}
