using Backend.Domain.Modules.CourseEvents.Contracts;
using Backend.Domain.Modules.CourseEvents.Models;
using Backend.Domain.Modules.CourseEventTypes.Models;
using Backend.Domain.Modules.VenueTypes.Models;
using Backend.Infrastructure.Persistence.EFC.Context;
using Backend.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.EFC.Repositories
{
    public class CourseEventRepository(CoursesOnlineDbContext context)
        : RepositoryBase<CourseEvent, Guid, CourseEventEntity, CoursesOnlineDbContext>(context), ICourseEventRepository
    {
        protected override CourseEvent ToModel(CourseEventEntity entity)
        {
            var courseEventTypeEntity = entity.CourseEventType
                ?? throw new InvalidOperationException("Course event type must be loaded from database.");
            var venueTypeEntity = entity.VenueType
                ?? throw new InvalidOperationException("Venue type must be loaded from database.");

            var courseEventType = CourseEventType.Reconstitute(courseEventTypeEntity.Id, courseEventTypeEntity.Name);
            var venueType = VenueType.Reconstitute(venueTypeEntity.Id, venueTypeEntity.Name);

            return CourseEvent.Reconstitute(
                entity.Id,
                entity.CourseId,
                entity.EventDate,
                entity.Price,
                entity.Seats,
                entity.CourseEventTypeId,
                venueType,
                courseEventType,
                venueType);
        }

        protected override CourseEventEntity ToEntity(CourseEvent courseEvent)
        {
            var venueType = courseEvent.VenueType
                ?? throw new InvalidOperationException("Venue type must be set when mapping course event.");

            return new CourseEventEntity
            {
                Id = courseEvent.Id,
                CourseId = courseEvent.CourseId,
                EventDate = courseEvent.EventDate,
                Price = courseEvent.Price.Value,
                Seats = courseEvent.Seats,
                CourseEventTypeId = courseEvent.CourseEventTypeId,
                VenueTypeId = venueType.Id
            };
        }

        public override async Task<CourseEvent> AddAsync(CourseEvent courseEvent, CancellationToken cancellationToken)
        {
            var entity = ToEntity(courseEvent);
            _context.CourseEvents.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            var created = await _context.CourseEvents
                .AsNoTracking()
                .Include(ce => ce.CourseEventType)
                .Include(ce => ce.VenueType)
                .SingleAsync(ce => ce.Id == entity.Id, cancellationToken);

            return ToModel(created);
        }

        public override async Task<bool> RemoveAsync(Guid courseEventId, CancellationToken cancellationToken)
        {
            using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var exists = await _context.CourseEvents
                    .AnyAsync(ce => ce.Id == courseEventId, cancellationToken);

                if (!exists)
                    throw new KeyNotFoundException($"Course event '{courseEventId}' not found.");

                await _context.Database.ExecuteSqlAsync(
                    $"DELETE FROM CourseRegistrations WHERE CourseEventId = {courseEventId}",
                    cancellationToken);

                await _context.Database.ExecuteSqlAsync(
                    $"DELETE FROM CourseEventInstructors WHERE CourseEventId = {courseEventId}",
                    cancellationToken);

                await _context.Database.ExecuteSqlAsync(
                    $"DELETE FROM InPlaceEventLocations WHERE CourseEventId = {courseEventId}",
                    cancellationToken);

                await _context.Database.ExecuteSqlAsync(
                    $"DELETE FROM CourseEvents WHERE Id = {courseEventId}",
                    cancellationToken);

                await tx.CommitAsync(cancellationToken);
                return true;
            }
            catch
            {
                await tx.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<bool> HasRegistrationsAsync(Guid courseEventId, CancellationToken cancellationToken)
        {
            return await _context.CourseRegistrations
                .AsNoTracking()
                .AnyAsync(cr => cr.CourseEventId == courseEventId, cancellationToken);
        }

        public override async Task<IReadOnlyList<CourseEvent>> GetAllAsync(CancellationToken cancellationToken)
        {
            var entities = await _context.CourseEvents
                .AsNoTracking()
                .Include(ce => ce.CourseEventType)
                .Include(ce => ce.VenueType)
                .OrderByDescending(ce => ce.CreatedAtUtc)
                .ThenByDescending(ce => ce.Id)
                .ToListAsync(cancellationToken);

            return [.. entities.Select(ToModel)];
        }

        public override async Task<CourseEvent?> GetByIdAsync(Guid courseEventId, CancellationToken cancellationToken)
        {
            var entity = await _context.CourseEvents
                .AsNoTracking()
                .Include(ce => ce.CourseEventType)
                .Include(ce => ce.VenueType)
                .SingleOrDefaultAsync(ce => ce.Id == courseEventId, cancellationToken);

            return entity == null ? null : ToModel(entity);
        }

        public async Task<IReadOnlyList<CourseEvent>> GetCourseEventsByCourseIdAsync(Guid courseId, CancellationToken cancellationToken)
        {
            var entities = await _context.CourseEvents
                .AsNoTracking()
                .Include(ce => ce.CourseEventType)
                .Include(ce => ce.VenueType)
                .Where(ce => ce.CourseId == courseId)
                .OrderBy(ce => ce.EventDate)
                .ToListAsync(cancellationToken);

            return [.. entities.Select(ToModel)];
        }

        public override async Task<CourseEvent?> UpdateAsync(Guid id, CourseEvent courseEvent, CancellationToken cancellationToken)
        {
            var entity = await _context.CourseEvents.SingleOrDefaultAsync(ce => ce.Id == id, cancellationToken);

            if (entity == null)
                throw new KeyNotFoundException($"Course event '{courseEvent.Id}' not found.");

            entity.CourseId = courseEvent.CourseId;
            entity.EventDate = courseEvent.EventDate;
            entity.Price = courseEvent.Price.Value;
            entity.Seats = courseEvent.Seats;
            entity.CourseEventTypeId = courseEvent.CourseEventTypeId;
            entity.VenueTypeId = courseEvent.VenueType.Id;
            entity.ModifiedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            var updated = await _context.CourseEvents
                .AsNoTracking()
                .Include(ce => ce.CourseEventType)
                .Include(ce => ce.VenueType)
                .SingleAsync(ce => ce.Id == id, cancellationToken);

            return ToModel(updated);
        }

    }
}

