using Backend.Application.Common;
using Backend.Application.Modules.VenueTypes.Inputs;
using Backend.Domain.Modules.VenueTypes.Models;

namespace Backend.Application.Modules.VenueTypes;

public interface IVenueTypeService
{
    Task<Result<IReadOnlyList<VenueType>>> GetAllVenueTypesAsync(CancellationToken cancellationToken = default);
    Task<Result<VenueType>> GetVenueTypeByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<VenueType>> GetVenueTypeByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Result<VenueType>> CreateVenueTypeAsync(CreateVenueTypeInput input, CancellationToken cancellationToken = default);
    Task<Result<VenueType>> UpdateVenueTypeAsync(UpdateVenueTypeInput input, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteVenueTypeAsync(int id, CancellationToken cancellationToken = default);
}
