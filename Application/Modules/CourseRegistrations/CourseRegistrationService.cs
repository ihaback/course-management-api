using Backend.Application.Common;
using Backend.Application.Modules.CourseRegistrations.Inputs;
using Backend.Application.Modules.CourseRegistrations.Outputs;
using Backend.Domain.Modules.CourseEvents.Contracts;
using Backend.Domain.Modules.CourseRegistrationStatuses.Contracts;
using Backend.Domain.Modules.CourseRegistrationStatuses.Models;
using Backend.Domain.Modules.CourseRegistrations.Contracts;
using Backend.Domain.Modules.CourseRegistrations.Models;
using Backend.Domain.Modules.PaymentMethods.Contracts;
using Backend.Domain.Modules.PaymentMethods.Models;
using Backend.Domain.Modules.Participants.Contracts;

namespace Backend.Application.Modules.CourseRegistrations;

public sealed class CourseRegistrationService(
    ICourseRegistrationRepository courseRegistrationRepository,
    IParticipantRepository participantRepository,
    ICourseEventRepository courseEventRepository,
    ICourseRegistrationStatusRepository statusRepository,
    IPaymentMethodRepository paymentMethodRepository) : ICourseRegistrationService
{
    private readonly ICourseRegistrationRepository _courseRegistrationRepository = courseRegistrationRepository ?? throw new ArgumentNullException(nameof(courseRegistrationRepository));
    private readonly IParticipantRepository _participantRepository = participantRepository ?? throw new ArgumentNullException(nameof(participantRepository));
    private readonly ICourseEventRepository _courseEventRepository = courseEventRepository ?? throw new ArgumentNullException(nameof(courseEventRepository));
    private readonly ICourseRegistrationStatusRepository _statusRepository = statusRepository ?? throw new ArgumentNullException(nameof(statusRepository));
    private readonly IPaymentMethodRepository _paymentMethodRepository = paymentMethodRepository ?? throw new ArgumentNullException(nameof(paymentMethodRepository));

    public async Task<Result<CourseRegistration>> CreateCourseRegistrationAsync(CreateCourseRegistrationInput courseRegistration, CancellationToken cancellationToken = default)
    {
        try
        {
            if (courseRegistration == null)
            {
                return Result<CourseRegistration>.BadRequest("Course registration cannot be null.");
            }

            if (courseRegistration.ParticipantId == Guid.Empty)
            {
                return Result<CourseRegistration>.BadRequest("Participant ID cannot be empty.");
            }

            if (courseRegistration.CourseEventId == Guid.Empty)
            {
                return Result<CourseRegistration>.BadRequest("Course event ID cannot be empty.");
            }

            var participant = await _participantRepository.GetByIdAsync(courseRegistration.ParticipantId, cancellationToken);
            if (participant is null)
            {
                return Result<CourseRegistration>.NotFound($"Participant with ID '{courseRegistration.ParticipantId}' not found.");
            }

            var courseEvent = await _courseEventRepository.GetByIdAsync(courseRegistration.CourseEventId, cancellationToken);
            if (courseEvent is null)
            {
                return Result<CourseRegistration>.NotFound($"Course event with ID '{courseRegistration.CourseEventId}' not found.");
            }

            var status = await _statusRepository.GetByIdAsync(courseRegistration.StatusId, cancellationToken);
            if (status is null)
            {
                return Result<CourseRegistration>.NotFound($"Course registration status with ID '{courseRegistration.StatusId}' not found.");
            }

            var paymentMethod = await _paymentMethodRepository.GetByIdAsync(courseRegistration.PaymentMethodId, cancellationToken);
            if (paymentMethod is null)
            {
                return Result<CourseRegistration>.NotFound($"Payment method with ID '{courseRegistration.PaymentMethodId}' not found.");
            }

            var newCourseRegistration = CourseRegistration.Create(
                courseRegistration.ParticipantId,
                courseRegistration.CourseEventId,
                DateTime.UtcNow,
                status,
                paymentMethod
            );

            var createdCourseRegistration = await _courseRegistrationRepository.AddAsync(newCourseRegistration, cancellationToken);

            return Result<CourseRegistration>.Ok(createdCourseRegistration);
        }
        catch (ArgumentException ex)
        {
            return Result<CourseRegistration>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<CourseRegistration>.Error("An error occurred while creating the course registration.");
        }
    }

    public async Task<Result<IReadOnlyList<CourseRegistration>>> GetAllCourseRegistrationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var courseRegistrations = await _courseRegistrationRepository.GetAllAsync(cancellationToken);

            return Result<IReadOnlyList<CourseRegistration>>.Ok(courseRegistrations);
        }
        catch (Exception)
        {
            return Result<IReadOnlyList<CourseRegistration>>.Error("An error occurred while retrieving course registrations.");
        }
    }

    public async Task<Result<CourseRegistrationDetails>> GetCourseRegistrationByIdAsync(Guid courseRegistrationId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (courseRegistrationId == Guid.Empty)
            {
                return Result<CourseRegistrationDetails>.BadRequest("Course registration ID cannot be empty.");
            }

            var courseRegistration = await _courseRegistrationRepository.GetByIdAsync(courseRegistrationId, cancellationToken);

            if (courseRegistration == null)
            {
                return Result<CourseRegistrationDetails>.NotFound($"Course registration with ID '{courseRegistrationId}' not found.");
            }

            var participant = await _participantRepository.GetByIdAsync(courseRegistration.ParticipantId, cancellationToken);
            var participantName = participant == null
                ? courseRegistration.ParticipantId.ToString()
                : $"{participant.FirstName} {participant.LastName}".Trim();

            var courseEvent = await _courseEventRepository.GetByIdAsync(courseRegistration.CourseEventId, cancellationToken);

            var details = new CourseRegistrationDetails(
                courseRegistration.Id,
                new RegistrationGuidLookupItem(
                    courseRegistration.ParticipantId,
                    participantName),
                new RegistrationCourseEventItem(
                    courseRegistration.CourseEventId,
                    courseEvent?.EventDate),
                courseRegistration.RegistrationDate,
                new RegistrationLookupItem(courseRegistration.Status.Id, courseRegistration.Status.Name),
                new RegistrationLookupItem(courseRegistration.PaymentMethod.Id, courseRegistration.PaymentMethod.Name)
            );

            return Result<CourseRegistrationDetails>.Ok(details);
        }
        catch (Exception)
        {
            return Result<CourseRegistrationDetails>.Error("An error occurred while retrieving the course registration.");
        }
    }

    public async Task<Result<IReadOnlyList<CourseRegistration>>> GetCourseRegistrationsByParticipantIdAsync(Guid participantId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (participantId == Guid.Empty)
            {
                return Result<IReadOnlyList<CourseRegistration>>.BadRequest("Participant ID cannot be empty.");
            }

            var courseRegistrations = await _courseRegistrationRepository.GetCourseRegistrationsByParticipantIdAsync(participantId, cancellationToken);

            return Result<IReadOnlyList<CourseRegistration>>.Ok(courseRegistrations);
        }
        catch (Exception)
        {
            return Result<IReadOnlyList<CourseRegistration>>.Error("An error occurred while retrieving course registrations.");
        }
    }

    public async Task<Result<IReadOnlyList<CourseRegistration>>> GetCourseRegistrationsByCourseEventIdAsync(Guid courseEventId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (courseEventId == Guid.Empty)
            {
                return Result<IReadOnlyList<CourseRegistration>>.BadRequest("Course event ID cannot be empty.");
            }

            var courseRegistrations = await _courseRegistrationRepository.GetCourseRegistrationsByCourseEventIdAsync(courseEventId, cancellationToken);

            return Result<IReadOnlyList<CourseRegistration>>.Ok(courseRegistrations);
        }
        catch (Exception)
        {
            return Result<IReadOnlyList<CourseRegistration>>.Error("An error occurred while retrieving course registrations.");
        }
    }

    public async Task<Result<CourseRegistration>> UpdateCourseRegistrationAsync(UpdateCourseRegistrationInput courseRegistration, CancellationToken cancellationToken = default)
    {
        try
        {
            if (courseRegistration == null)
            {
                return Result<CourseRegistration>.BadRequest("Course registration cannot be null.");
            }

            if (courseRegistration.Id == Guid.Empty)
            {
                return Result<CourseRegistration>.BadRequest("Course registration ID cannot be empty.");
            }

            if (courseRegistration.ParticipantId == Guid.Empty)
            {
                return Result<CourseRegistration>.BadRequest("Participant ID cannot be empty.");
            }

            if (courseRegistration.CourseEventId == Guid.Empty)
            {
                return Result<CourseRegistration>.BadRequest("Course event ID cannot be empty.");
            }

            var existingCourseRegistration = await _courseRegistrationRepository.GetByIdAsync(courseRegistration.Id, cancellationToken);
            if (existingCourseRegistration == null)
            {
                return Result<CourseRegistration>.NotFound($"Course registration with ID '{courseRegistration.Id}' not found.");
            }

            var participant = await _participantRepository.GetByIdAsync(courseRegistration.ParticipantId, cancellationToken);
            if (participant is null)
            {
                return Result<CourseRegistration>.NotFound($"Participant with ID '{courseRegistration.ParticipantId}' not found.");
            }

            var courseEvent = await _courseEventRepository.GetByIdAsync(courseRegistration.CourseEventId, cancellationToken);
            if (courseEvent is null)
            {
                return Result<CourseRegistration>.NotFound($"Course event with ID '{courseRegistration.CourseEventId}' not found.");
            }

            var status = await _statusRepository.GetByIdAsync(courseRegistration.StatusId, cancellationToken);
            if (status is null)
            {
                return Result<CourseRegistration>.NotFound($"Course registration status with ID '{courseRegistration.StatusId}' not found.");
            }

            var paymentMethod = await _paymentMethodRepository.GetByIdAsync(courseRegistration.PaymentMethodId, cancellationToken);
            if (paymentMethod is null)
            {
                return Result<CourseRegistration>.NotFound($"Payment method with ID '{courseRegistration.PaymentMethodId}' not found.");
            }

            existingCourseRegistration.Update(
                courseRegistration.ParticipantId,
                courseRegistration.CourseEventId,
                existingCourseRegistration.RegistrationDate,
                status,
                paymentMethod
            );

            var updatedCourseRegistration = await _courseRegistrationRepository.UpdateAsync(existingCourseRegistration.Id, existingCourseRegistration, cancellationToken);

            if (updatedCourseRegistration == null)
            {
                return Result<CourseRegistration>.Error("Failed to update course registration.");
            }

            return Result<CourseRegistration>.Ok(updatedCourseRegistration);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("modified by another user"))
        {
            return Result<CourseRegistration>.Conflict("The course registration was modified by another user. Please refresh and try again.");
        }
        catch (ArgumentException ex)
        {
            return Result<CourseRegistration>.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Result<CourseRegistration>.Error("An error occurred while updating the course registration.");
        }
    }

    public async Task<Result<bool>> DeleteCourseRegistrationAsync(Guid courseRegistrationId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (courseRegistrationId == Guid.Empty)
            {
                return Result<bool>.BadRequest("Course registration ID cannot be empty.");
            }

            var existingCourseRegistration = await _courseRegistrationRepository.GetByIdAsync(courseRegistrationId, cancellationToken);
            if (existingCourseRegistration == null)
            {
                return Result<bool>.NotFound($"Course registration with ID '{courseRegistrationId}' not found.");
            }

            var isDeleted = await _courseRegistrationRepository.RemoveAsync(courseRegistrationId, cancellationToken);

            if (!isDeleted)
            {
                return Result<bool>.Error("Failed to delete course registration.");
            }

            return Result<bool>.Ok(true);
        }
        catch (Exception)
        {
            return Result<bool>.Error("An error occurred while deleting the course registration.");
        }
    }

}

