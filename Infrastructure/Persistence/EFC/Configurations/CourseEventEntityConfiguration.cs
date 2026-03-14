using Backend.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.EFC.Configurations;

public sealed class CourseEventEntityConfiguration(bool isSqlite) : IEntityTypeConfiguration<CourseEventEntity>
{
    public void Configure(EntityTypeBuilder<CourseEventEntity> e)
    {

        e.ToTable("CourseEvents", t =>
        {
            t.HasCheckConstraint("CK_CourseEvents_Price", "[Price] >= 0");
            t.HasCheckConstraint("CK_CourseEvents_Seats", "[Seats] > 0");
        });

        e.HasKey(x => x.Id).HasName("PK_CourseEvents_Id");

        e.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("(NEWSEQUENTIALID())", "DF_CourseEvents_Id");

        e.Property(x => x.EventDate)
            .HasPrecision(0)
            .IsRequired();

        e.Property(x => x.Price)
            .HasColumnType("money")
            .IsRequired();

        e.Property(x => x.Seats)
            .IsRequired();

        e.Property(x => x.VenueTypeId)
            .HasDefaultValue(1)
            .IsRequired();

        if (isSqlite)
        {
            e.Property(x => x.Concurrency)
                .IsConcurrencyToken()
                .IsRequired(false);

            e.Property(x => x.CreatedAtUtc)
                .HasPrecision(0)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            e.Property(x => x.ModifiedAtUtc)
                .HasPrecision(0)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();
        }
        else
        {
            e.Property(x => x.Concurrency)
                .IsRowVersion()
                .IsConcurrencyToken()
                .IsRequired();

            e.Property(x => x.CreatedAtUtc)
                .HasPrecision(0)
                .HasDefaultValueSql("(SYSUTCDATETIME())", "DF_CourseEvents_CreatedAtUtc")
                .ValueGeneratedOnAdd();

            e.Property(x => x.ModifiedAtUtc)
                .HasPrecision(0)
                .HasDefaultValueSql("(SYSUTCDATETIME())", "DF_CourseEvents_ModifiedAtUtc")
                .ValueGeneratedOnAddOrUpdate();
        }

        e.HasIndex(x => new { x.CourseId, x.EventDate })
            .HasDatabaseName("IX_CourseEvents_CourseId_EventDate");

        e.HasOne(ce => ce.CourseEventType)
            .WithMany(cet => cet.CourseEvents)
            .HasForeignKey(ce => ce.CourseEventTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(ce => ce.VenueType)
            .WithMany()
            .HasForeignKey(ce => ce.VenueTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasMany(ce => ce.Instructors)
            .WithMany(i => i.CourseEvents)
            .UsingEntity<CourseEventInstructorEntity>(
                cei => cei.HasOne(ci => ci.Instructor).WithMany().HasForeignKey(ci => ci.InstructorId).OnDelete(DeleteBehavior.Cascade),
                cei => cei.HasOne(ci => ci.CourseEvent).WithMany().HasForeignKey(ci => ci.CourseEventId).OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey(ci => new { ci.CourseEventId, ci.InstructorId });
                    j.ToTable("CourseEventInstructors");
                });

        e.HasMany(ce => ce.InPlaceLocations)
            .WithMany(ipl => ipl.CourseEvents)
            .UsingEntity<InPlaceEventLocationEntity>(
                ipl => ipl.HasOne(ip => ip.InPlaceLocation).WithMany().HasForeignKey(ip => ip.InPlaceLocationId).OnDelete(DeleteBehavior.Cascade),
                ce => ce.HasOne(ip => ip.CourseEvent).WithMany().HasForeignKey(ip => ip.CourseEventId).OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey(ip => new { ip.CourseEventId, ip.InPlaceLocationId });
                    j.ToTable("InPlaceEventLocations");
                });
    }
}
