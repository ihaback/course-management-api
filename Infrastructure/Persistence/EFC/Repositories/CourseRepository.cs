using Backend.Domain.Modules.CourseEvents.Models;
using Backend.Domain.Modules.CourseEventTypes.Models;
using Backend.Domain.Modules.Courses.Contracts;
using Backend.Domain.Modules.Courses.Models;
using Backend.Domain.Modules.VenueTypes.Models;
using Backend.Infrastructure.Persistence.EFC.Context;
using Backend.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.EFC.Repositories
{
    public class CourseRepository(CoursesOnlineDbContext context)
        : RepositoryBase<Course, Guid, CourseEntity, CoursesOnlineDbContext>(context), ICourseRepository
    {
        protected override Course ToModel(CourseEntity entity)
            => Course.Reconstitute(entity.Id, entity.Title, entity.Description, entity.DurationInDays);

        private static CourseEvent ToCourseEventModel(CourseEventEntity entity)
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

        protected override CourseEntity ToEntity(Course course)
            => new()
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                DurationInDays = course.DurationInDays
            };

        public override async Task<Course> AddAsync(Course course, CancellationToken cancellationToken)
        {
            var entity = ToEntity(course);
            _context.Courses.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return ToModel(entity);
        }

        public override async Task<bool> RemoveAsync(Guid courseId, CancellationToken cancellationToken)
        {
            var entity = await _context.Courses.SingleOrDefaultAsync(c => c.Id == courseId, cancellationToken);
            if (entity == null)
                throw new KeyNotFoundException($"Course '{courseId}' not found.");

            _context.Courses.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public override async Task<IReadOnlyList<Course>> GetAllAsync(CancellationToken cancellationToken)
        {
            var entities = await _context.Courses
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAtUtc)
                .ThenByDescending(c => c.Id)
                .ToListAsync(cancellationToken);

            return [.. entities.Select(ToModel)];
        }

        public async Task<CourseWithEvents?> GetByIdWithEventsAsync(Guid courseId, CancellationToken cancellationToken)
        {
            var entity = await _context.Courses
                .AsNoTracking()
                .Include(c => c.CourseEvents)
                    .ThenInclude(ce => ce.CourseEventType)
                .Include(c => c.CourseEvents)
                    .ThenInclude(ce => ce.VenueType)
                .SingleOrDefaultAsync(c => c.Id == courseId, cancellationToken);

            if (entity == null)
                return null;

            var course = ToModel(entity);
            var events = entity.CourseEvents
                .Select(ToCourseEventModel)
                .ToList();

            return new CourseWithEvents(course, events);
        }

        public override async Task<Course?> UpdateAsync(Guid id, Course course, CancellationToken cancellationToken)
        {
            var entity = await _context.Courses.SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
            if (entity is null)
                throw new KeyNotFoundException($"Course '{course.Id}' not found.");

            entity.Title = course.Title;
            entity.Description = course.Description;
            entity.DurationInDays = course.DurationInDays;
            entity.ModifiedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return ToModel(entity);
        }

        public async Task<bool> HasCourseEventsAsync(Guid courseId, CancellationToken cancellationToken)
        {
            return await _context.CourseEvents
                .AsNoTracking()
                .AnyAsync(ce => ce.CourseId == courseId, cancellationToken);
        }
    }
}
