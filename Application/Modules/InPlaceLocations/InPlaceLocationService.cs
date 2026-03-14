using Backend.Application.Common;
using Backend.Application.Modules.InPlaceLocations.Inputs;

using Backend.Domain.Modules.InPlaceLocations.Contracts;
using Backend.Domain.Modules.InPlaceLocations.Models;

namespace Backend.Application.Modules.InPlaceLocations;

public class InPlaceLocationService(IInPlaceLocationRepository inPlaceLocationRepository) : IInPlaceLocationService
{
    private readonly IInPlaceLocationRepository _inPlaceLocationRepository = inPlaceLocationRepository ?? throw new ArgumentNullException(nameof(inPlaceLocationRepository));

    public async Task<Result<InPlaceLocation>> CreateInPlaceLocationAsync(CreateInPlaceLocationInput inPlaceLocation, CancellationToken cancellationToken = default)
    {
        try
        {
            if (inPlaceLocation == null)
            {
                return Result<InPlaceLocation>.BadRequest("In-place location cannot be null.");
            }

            var newInPlaceLocation = InPlaceLocation.Create(
                inPlaceLocation.LocationId,
                inPlaceLocation.RoomNumber,
                inPlaceLocation.Seats
            );

            var createdInPlaceLocation = await _inPlaceLocationRepository.AddAsync(newInPlaceLocation, cancellationToken);

            return Result<InPlaceLocation>.Ok(createdInPlaceLocation);
        }
        catch (ArgumentException ex)
        {
            return Result<InPlaceLocation>.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<InPlaceLocation>.Error($"An error occurred while creating the in-place location: {ex.Message}");
        }
    }

    public async Task<Result<IReadOnlyList<InPlaceLocation>>> GetAllInPlaceLocationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var inPlaceLocations = await _inPlaceLocationRepository.GetAllAsync(cancellationToken);

            if (!inPlaceLocations.Any())
            {
                return Result<IReadOnlyList<InPlaceLocation>>.Ok(inPlaceLocations);
            }

            return Result<IReadOnlyList<InPlaceLocation>>.Ok(inPlaceLocations);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<InPlaceLocation>>.Error($"An error occurred while retrieving in-place locations: {ex.Message}");
        }
    }

    public async Task<Result<InPlaceLocation>> GetInPlaceLocationByIdAsync(int inPlaceLocationId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (inPlaceLocationId <= 0)
            {
                return Result<InPlaceLocation>.BadRequest("In-place location ID must be greater than zero.");
            }

            var inPlaceLocation = await _inPlaceLocationRepository.GetByIdAsync(inPlaceLocationId, cancellationToken);

            if (inPlaceLocation == null)
            {
                return Result<InPlaceLocation>.NotFound($"In-place location with ID '{inPlaceLocationId}' not found.");
            }

            return Result<InPlaceLocation>.Ok(inPlaceLocation);
        }
        catch (Exception ex)
        {
            return Result<InPlaceLocation>.Error($"An error occurred while retrieving the in-place location: {ex.Message}");
        }
    }

    public async Task<Result<IReadOnlyList<InPlaceLocation>>> GetInPlaceLocationsByLocationIdAsync(int locationId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (locationId <= 0)
            {
                return Result<IReadOnlyList<InPlaceLocation>>.BadRequest("Location ID must be greater than zero.");
            }

            var inPlaceLocations = await _inPlaceLocationRepository.GetInPlaceLocationsByLocationIdAsync(locationId, cancellationToken);

            if (!inPlaceLocations.Any())
            {
                return Result<IReadOnlyList<InPlaceLocation>>.Ok(inPlaceLocations);
            }

            return Result<IReadOnlyList<InPlaceLocation>>.Ok(inPlaceLocations);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<InPlaceLocation>>.Error($"An error occurred while retrieving in-place locations: {ex.Message}");
        }
    }

    public async Task<Result<InPlaceLocation>> UpdateInPlaceLocationAsync(UpdateInPlaceLocationInput inPlaceLocation, CancellationToken cancellationToken = default)
    {
        try
        {
            if (inPlaceLocation == null)
            {
                return Result<InPlaceLocation>.BadRequest("In-place location cannot be null.");
            }

            if (inPlaceLocation.Id <= 0)
            {
                return Result<InPlaceLocation>.BadRequest("In-place location ID must be greater than zero.");
            }

            var existingInPlaceLocation = await _inPlaceLocationRepository.GetByIdAsync(inPlaceLocation.Id, cancellationToken);
            if (existingInPlaceLocation == null)
            {
                return Result<InPlaceLocation>.NotFound($"In-place location with ID '{inPlaceLocation.Id}' not found.");
            }

            existingInPlaceLocation.Update(
                inPlaceLocation.LocationId,
                inPlaceLocation.RoomNumber,
                inPlaceLocation.Seats
            );

            var updatedInPlaceLocation = await _inPlaceLocationRepository.UpdateAsync(existingInPlaceLocation.Id, existingInPlaceLocation, cancellationToken);

            if (updatedInPlaceLocation == null)
            {
                return Result<InPlaceLocation>.Error("Failed to update in-place location.");
            }

            return Result<InPlaceLocation>.Ok(updatedInPlaceLocation);
        }
        catch (ArgumentException ex)
        {
            return Result<InPlaceLocation>.BadRequest(ex.Message);
        }
        catch (Exception ex) when (ex.GetType().Name == "DbUpdateException")
        {
            return Result<InPlaceLocation>.Conflict("Cannot update because the requested location reference is invalid.");
        }
        catch (Exception ex)
        {
            return Result<InPlaceLocation>.Error($"An error occurred while updating the in-place location: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteInPlaceLocationAsync(int inPlaceLocationId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (inPlaceLocationId <= 0)
            {
                return Result<bool>.BadRequest("In-place location ID must be greater than zero.");
            }

            var existingInPlaceLocation = await _inPlaceLocationRepository.GetByIdAsync(inPlaceLocationId, cancellationToken);
            if (existingInPlaceLocation == null)
            {
                return Result<bool>.NotFound($"In-place location with ID '{inPlaceLocationId}' not found.");
            }

            var hasCourseEvents = await _inPlaceLocationRepository.HasCourseEventsAsync(inPlaceLocationId, cancellationToken);
            if (hasCourseEvents)
            {
                return Result<bool>.Conflict($"Cannot delete in-place location with ID '{inPlaceLocationId}' because it is assigned to course events. Please remove the assignments first.");
            }

            var isDeleted = await _inPlaceLocationRepository.RemoveAsync(inPlaceLocationId, cancellationToken);

            if (!isDeleted)
            {
                return Result<bool>.Error("Failed to delete in-place location.");
            }

            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Error($"An error occurred while deleting the in-place location: {ex.Message}");
        }
    }
}

