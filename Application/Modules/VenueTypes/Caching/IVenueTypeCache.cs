using Backend.Application.Common.Caching;
using Backend.Domain.Modules.VenueTypes.Models;

namespace Backend.Application.Modules.VenueTypes.Caching;

public interface IVenueTypeCache : ICacheEntityBase<VenueType, int>
{
    Task<IReadOnlyList<VenueType>> GetAllAsync(Func<CancellationToken, Task<IReadOnlyList<VenueType>>> factory, CancellationToken ct);
    Task<VenueType?> GetByIdAsync(int id, Func<CancellationToken, Task<VenueType?>> factory, CancellationToken ct);
    Task<VenueType?> GetByNameAsync(string name, Func<CancellationToken, Task<VenueType?>> factory, CancellationToken ct);
}
