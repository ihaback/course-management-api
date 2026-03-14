using Backend.Domain.Modules.CourseRegistrations.Contracts;
using Backend.Domain.Modules.CourseRegistrations.Models;
using Backend.Domain.Modules.CourseRegistrationStatuses.Models;
using Backend.Domain.Modules.PaymentMethods.Models;
using Backend.Infrastructure.Persistence.EFC.Context;
using Backend.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.EFC.Repositories;

public class CourseRegistrationRepository(CoursesOnlineDbContext context)
    : RepositoryBase<CourseRegistration, Guid, CourseRegistrationEntity, CoursesOnlineDbContext>(context), ICourseRegistrationRepository
{
    protected override CourseRegistration ToModel(CourseRegistrationEntity entity)
    {
        var statusEntity = entity.CourseRegistrationStatus
            ?? throw new InvalidOperationException("Course registration status must be loaded from database.");
        var paymentMethodEntity = entity.PaymentMethod
            ?? throw new InvalidOperationException("Payment method must be loaded from database.");

        var status = CourseRegistrationStatus.Reconstitute(statusEntity.Id, statusEntity.Name);
        var paymentMethod = PaymentMethod.Reconstitute(paymentMethodEntity.Id, paymentMethodEntity.Name);

        return CourseRegistration.Reconstitute(
            entity.Id,
            entity.ParticipantId,
            entity.CourseEventId,
            entity.RegistrationDate,
            status,
            paymentMethod);
    }

    protected override CourseRegistrationEntity ToEntity(CourseRegistration courseRegistration)
    {
        var status = courseRegistration.Status
            ?? throw new InvalidOperationException("Course registration status must be set when mapping registration.");
        var paymentMethod = courseRegistration.PaymentMethod
            ?? throw new InvalidOperationException("Payment method must be set when mapping registration.");

        return new CourseRegistrationEntity
        {
            Id = courseRegistration.Id,
            ParticipantId = courseRegistration.ParticipantId,
            CourseEventId = courseRegistration.CourseEventId,
            CourseRegistrationStatusId = status.Id,
            PaymentMethodId = paymentMethod.Id
        };
    }

    public override async Task<CourseRegistration> AddAsync(CourseRegistration courseRegistration, CancellationToken cancellationToken)
    {
        using var tx = await _context.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable,
            cancellationToken);

        try
        {
            var availableSeats = await _context.Database
                .SqlQuery<int>(
                    $"""
                    SELECT ce.Seats - COALESCE(COUNT(cr.Id), 0) AS Value
                    FROM CourseEvents ce
                    LEFT JOIN CourseRegistrations cr ON ce.Id = cr.CourseEventId
                    WHERE ce.Id = {courseRegistration.CourseEventId}
                    GROUP BY ce.Id, ce.Seats
                    """)
                .FirstOrDefaultAsync(cancellationToken);

            if (availableSeats <= 0)
                throw new InvalidOperationException($"No available seats for course event '{courseRegistration.CourseEventId}'.");

            var entity = ToEntity(courseRegistration);

            _context.CourseRegistrations.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            var created = await _context.CourseRegistrations
                .AsNoTracking()
                .Include(cr => cr.CourseRegistrationStatus)
                .Include(cr => cr.PaymentMethod)
                .SingleAsync(cr => cr.Id == entity.Id, cancellationToken);

            return ToModel(created);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<CourseRegistration?> CreateRegistrationWithSeatCheckAsync(
        CourseRegistration courseRegistration,
        CancellationToken cancellationToken)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable,
            cancellationToken);

        try
        {
            var availableSeats = await _context.Database
                .SqlQuery<int>(
                    $"""
                    SELECT ce.Seats - COALESCE(COUNT(cr.Id), 0) AS Value
                    FROM CourseEvents ce
                    LEFT JOIN CourseRegistrations cr ON ce.Id = cr.CourseEventId
                    WHERE ce.Id = {courseRegistration.CourseEventId}
                    GROUP BY ce.Id, ce.Seats
                    """)
                .FirstOrDefaultAsync(cancellationToken);

            if (availableSeats <= 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return null;
            }

            var entity = ToEntity(courseRegistration);

            _context.CourseRegistrations.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var created = await _context.CourseRegistrations
                .AsNoTracking()
                .Include(cr => cr.CourseRegistrationStatus)
                .Include(cr => cr.PaymentMethod)
                .SingleAsync(cr => cr.Id == entity.Id, cancellationToken);

            return ToModel(created);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public override async Task<bool> RemoveAsync(Guid courseRegistrationId, CancellationToken cancellationToken)
    {
        var entity = await _context.CourseRegistrations.SingleOrDefaultAsync(cr => cr.Id == courseRegistrationId, cancellationToken);
        if (entity == null)
            throw new KeyNotFoundException($"Course registration '{courseRegistrationId}' not found.");

        _context.CourseRegistrations.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public override async Task<IReadOnlyList<CourseRegistration>> GetAllAsync(CancellationToken cancellationToken)
    {
        var entities = await _context.CourseRegistrations
            .AsNoTracking()
            .Include(cr => cr.CourseRegistrationStatus)
            .Include(cr => cr.PaymentMethod)
            .OrderByDescending(cr => cr.RegistrationDate)
            .ThenByDescending(cr => cr.Id)
            .ToListAsync(cancellationToken);

        return [.. entities.Select(ToModel)];
    }

    public override async Task<CourseRegistration?> GetByIdAsync(Guid courseRegistrationId, CancellationToken cancellationToken)
    {
        var entity = await _context.CourseRegistrations
            .AsNoTracking()
            .Include(cr => cr.CourseRegistrationStatus)
            .Include(cr => cr.PaymentMethod)
            .SingleOrDefaultAsync(cr => cr.Id == courseRegistrationId, cancellationToken);

        return entity == null ? null : ToModel(entity);
    }

    public async Task<IReadOnlyList<CourseRegistration>> GetCourseRegistrationsByParticipantIdAsync(Guid participantId, CancellationToken cancellationToken)
    {
        var entities = await _context.CourseRegistrations
            .AsNoTracking()
            .Include(cr => cr.CourseRegistrationStatus)
            .Include(cr => cr.PaymentMethod)
            .Where(cr => cr.ParticipantId == participantId)
            .OrderByDescending(cr => cr.RegistrationDate)
            .ThenByDescending(cr => cr.Id)
            .ToListAsync(cancellationToken);

        return [.. entities.Select(ToModel)];
    }

    public async Task<IReadOnlyList<CourseRegistration>> GetCourseRegistrationsByCourseEventIdAsync(Guid courseEventId, CancellationToken cancellationToken)
    {
        var entities = await _context.CourseRegistrations
            .AsNoTracking()
            .Include(cr => cr.CourseRegistrationStatus)
            .Include(cr => cr.PaymentMethod)
            .Where(cr => cr.CourseEventId == courseEventId)
            .OrderByDescending(cr => cr.RegistrationDate)
            .ThenByDescending(cr => cr.Id)
            .ToListAsync(cancellationToken);

        return [.. entities.Select(ToModel)];
    }

    public override async Task<CourseRegistration?> UpdateAsync(Guid id, CourseRegistration courseRegistration, CancellationToken cancellationToken)
    {
        var entity = await _context.CourseRegistrations.SingleOrDefaultAsync(cr => cr.Id == id, cancellationToken);

        if (entity == null)
            throw new KeyNotFoundException($"Course registration '{courseRegistration.Id}' not found.");

        entity.ParticipantId = courseRegistration.ParticipantId;
        entity.CourseEventId = courseRegistration.CourseEventId;
        entity.CourseRegistrationStatusId = courseRegistration.Status.Id;
        entity.PaymentMethodId = courseRegistration.PaymentMethod.Id;
        entity.ModifiedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var updated = await _context.CourseRegistrations
            .AsNoTracking()
            .Include(cr => cr.CourseRegistrationStatus)
            .Include(cr => cr.PaymentMethod)
            .SingleAsync(cr => cr.Id == id, cancellationToken);

        return ToModel(updated);
    }

}







