using Backend.Application.Common;
using Backend.Application.Modules.Instructors.Inputs;
using Backend.Application.Modules.Instructors.Outputs;
using Backend.Domain.Modules.InstructorRoles.Contracts;
using Backend.Domain.Modules.Instructors.Contracts;
using Backend.Domain.Modules.Instructors.Models;

namespace Backend.Application.Modules.Instructors;

public sealed class InstructorService(IInstructorRepository instructorRepository, IInstructorRoleRepository instructorRoleRepository) : IInstructorService
{
    private readonly IInstructorRepository _instructorRepository = instructorRepository ?? throw new ArgumentNullException(nameof(instructorRepository));
    private readonly IInstructorRoleRepository _instructorRoleRepository = instructorRoleRepository ?? throw new ArgumentNullException(nameof(instructorRoleRepository));

    public async Task<Result<Instructor>> CreateInstructorAsync(CreateInstructorInput instructor, CancellationToken cancellationToken = default)
    {
        try
        {
            if (instructor == null)
            {
                return Result<Instructor>.BadRequest("Instructor cannot be null.");
            }

            var role = await _instructorRoleRepository.GetByIdAsync(instructor.InstructorRoleId, cancellationToken);
            if (role == null)
            {
                return Result<Instructor>.NotFound($"Instructor role with ID '{instructor.InstructorRoleId}' not found.");
            }

            var newInstructor = Instructor.Create(instructor.Name, role);

            var createdInstructor = await _instructorRepository.AddAsync(newInstructor, cancellationToken);

            return Result<Instructor>.Ok(createdInstructor);
        }
        catch (KeyNotFoundException ex)
        {
            return Result<Instructor>.NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Result<Instructor>.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<Instructor>.Error($"An error occurred while creating the instructor: {ex.Message}");
        }
    }

    public async Task<Result<IReadOnlyList<Instructor>>> GetAllInstructorsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var instructors = await _instructorRepository.GetAllAsync(cancellationToken);

            return Result<IReadOnlyList<Instructor>>.Ok(instructors);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Instructor>>.Error($"An error occurred while retrieving instructors: {ex.Message}");
        }
    }

    public async Task<Result<InstructorDetails>> GetInstructorByIdAsync(Guid instructorId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (instructorId == Guid.Empty)
            {
                return Result<InstructorDetails>.BadRequest("Instructor ID cannot be empty.");
            }

            var instructor = await _instructorRepository.GetByIdAsync(instructorId, cancellationToken);

            if (instructor == null)
            {
                return Result<InstructorDetails>.NotFound($"Instructor with ID '{instructorId}' not found.");
            }

            var details = new InstructorDetails(
                instructor.Id,
                instructor.Name,
                new InstructorLookupItem(instructor.Role.Id, instructor.Role.Name)
            );

            return Result<InstructorDetails>.Ok(details);
        }
        catch (KeyNotFoundException ex)
        {
            return Result<InstructorDetails>.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<InstructorDetails>.Error($"An error occurred while retrieving the instructor: {ex.Message}");
        }
    }

    public async Task<Result<Instructor>> UpdateInstructorAsync(UpdateInstructorInput instructor, CancellationToken cancellationToken = default)
    {
        try
        {
            if (instructor == null)
            {
                return Result<Instructor>.BadRequest("Instructor cannot be null.");
            }

            if (instructor.Id == Guid.Empty)
            {
                return Result<Instructor>.BadRequest("Instructor ID cannot be empty.");
            }

            var role = await _instructorRoleRepository.GetByIdAsync(instructor.InstructorRoleId, cancellationToken);
            if (role == null)
            {
                return Result<Instructor>.NotFound($"Instructor role with ID '{instructor.InstructorRoleId}' not found.");
            }

            var existingInstructor = await _instructorRepository.GetByIdAsync(instructor.Id, cancellationToken);
            if (existingInstructor == null)
            {
                return Result<Instructor>.NotFound($"Instructor with ID '{instructor.Id}' not found.");
            }

            existingInstructor.Update(instructor.Name, role);

            var updatedInstructor = await _instructorRepository.UpdateAsync(existingInstructor.Id, existingInstructor, cancellationToken);

            if (updatedInstructor == null)
            {
                return Result<Instructor>.Error("Failed to update instructor.");
            }

            return Result<Instructor>.Ok(updatedInstructor);
        }
        catch (ArgumentException ex)
        {
            return Result<Instructor>.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<Instructor>.Error($"An error occurred while updating the instructor: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteInstructorAsync(Guid instructorId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (instructorId == Guid.Empty)
            {
                return Result<bool>.BadRequest("Instructor ID cannot be empty.");
            }

            var existingInstructor = await _instructorRepository.GetByIdAsync(instructorId, cancellationToken);
            if (existingInstructor == null)
            {
                return Result<bool>.NotFound($"Instructor with ID '{instructorId}' not found.");
            }

            var hasCourseEvents = await _instructorRepository.HasCourseEventsAsync(instructorId, cancellationToken);
            if (hasCourseEvents)
            {
                return Result<bool>.Conflict($"Cannot delete instructor with ID '{instructorId}' because they are assigned to course events. Please remove the assignments first.");
            }

            var isDeleted = await _instructorRepository.RemoveAsync(instructorId, cancellationToken);

            if (!isDeleted)
            {
                return Result<bool>.Error("Failed to delete instructor.");
            }

            return Result<bool>.Ok(true);
        }
        catch (KeyNotFoundException ex)
        {
            return Result<bool>.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<bool>.Error($"An error occurred while deleting the instructor: {ex.Message}");
        }
    }
}

