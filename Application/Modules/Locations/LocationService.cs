using Backend.Application.Common;
using Backend.Application.Modules.Locations.Inputs;

using Backend.Domain.Modules.Locations.Contracts;
using Backend.Domain.Modules.Locations.Models;

namespace Backend.Application.Modules.Locations;

public class LocationService(ILocationRepository locationRepository) : ILocationService
{
    private readonly ILocationRepository _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));

    public async Task<Result<Location>> CreateLocationAsync(CreateLocationInput location, CancellationToken cancellationToken = default)
    {
        try
        {
            if (location == null)
            {
                return Result<Location>.BadRequest("Location cannot be null.");
            }

            var newLocation = Location.Create(
                location.StreetName,
                location.PostalCode,
                location.City
            );

            var createdLocation = await _locationRepository.AddAsync(newLocation, cancellationToken);

            return Result<Location>.Ok(createdLocation);
        }
        catch (ArgumentException ex)
        {
            return Result<Location>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<Location>.Error("An error occurred while creating the location.");
        }
    }

    public async Task<Result<IReadOnlyList<Location>>> GetAllLocationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var locations = await _locationRepository.GetAllAsync(cancellationToken);

            if (!locations.Any())
            {
                return Result<IReadOnlyList<Location>>.Ok(locations);
            }

            return Result<IReadOnlyList<Location>>.Ok(locations);
        }
        catch (Exception)
        {
            return Result<IReadOnlyList<Location>>.Error("An error occurred while retrieving locations.");
        }
    }

    public async Task<Result<Location>> GetLocationByIdAsync(int locationId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (locationId <= 0)
            {
                return Result<Location>.BadRequest("Location ID must be greater than zero.");
            }

            var existingLocation = await _locationRepository.GetByIdAsync(locationId, cancellationToken);

            if (existingLocation == null)
            {
                return Result<Location>.NotFound($"Location with ID '{locationId}' not found.");
            }

            return Result<Location>.Ok(existingLocation);
        }
        catch (Exception)
        {
            return Result<Location>.Error("An error occurred while retrieving the location.");
        }
    }

    public async Task<Result<Location>> UpdateLocationAsync(UpdateLocationInput location, CancellationToken cancellationToken = default)
    {
        try
        {
            if (location == null)
            {
                return Result<Location>.BadRequest("Location cannot be null.");
            }

            if (location.Id <= 0)
            {
                return Result<Location>.BadRequest("Location ID must be greater than zero.");
            }

            var existingLocation = await _locationRepository.GetByIdAsync(location.Id, cancellationToken);
            if (existingLocation == null)
            {
                return Result<Location>.NotFound($"Location with ID '{location.Id}' not found.");
            }

            existingLocation.Update(
                location.StreetName,
                location.PostalCode,
                location.City
            );

            var updatedLocation = await _locationRepository.UpdateAsync(existingLocation.Id, existingLocation, cancellationToken);

            if (updatedLocation == null)
            {
                return Result<Location>.Error("Failed to update location.");
            }

            return Result<Location>.Ok(updatedLocation);
        }
        catch (ArgumentException ex)
        {
            return Result<Location>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<Location>.Error("An error occurred while updating the location.");
        }
    }

    public async Task<Result<bool>> DeleteLocationAsync(int locationId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (locationId <= 0)
            {
                return Result<bool>.BadRequest("Location ID must be greater than zero.");
            }

            var existingLocation = await _locationRepository.GetByIdAsync(locationId, cancellationToken);
            if (existingLocation == null)
            {
                return Result<bool>.NotFound($"Location with ID '{locationId}' not found.");
            }

            var hasInPlaceLocations = await _locationRepository.HasInPlaceLocationsAsync(locationId, cancellationToken);
            if (hasInPlaceLocations)
            {
                return Result<bool>.Conflict($"Cannot delete location with ID '{locationId}' because it has in-place locations. Please delete the in-place locations first.");
            }

            var isDeleted = await _locationRepository.RemoveAsync(locationId, cancellationToken);

            if (!isDeleted)
            {
                return Result<bool>.Error("Failed to delete location.");
            }

            return Result<bool>.Ok(true);
        }
        catch (Exception)
        {
            return Result<bool>.Error("An error occurred while deleting the location.");
        }
    }
}

