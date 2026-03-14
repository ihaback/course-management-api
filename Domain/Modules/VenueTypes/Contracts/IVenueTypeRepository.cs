using Backend.Domain.Common.Base;
using Backend.Domain.Modules.VenueTypes.Models;

namespace Backend.Domain.Modules.VenueTypes.Contracts;

public interface IVenueTypeRepository : IRepositoryBase<VenueType, int>
{
    Task<VenueType?> GetByNameAsync(string name, CancellationToken cancellationToken);
    Task<bool> IsInUseAsync(int venueTypeId, CancellationToken cancellationToken);
}
