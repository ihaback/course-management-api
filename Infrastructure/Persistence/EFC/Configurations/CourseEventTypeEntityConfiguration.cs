using Backend.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.EFC.Configurations;

public sealed class CourseEventTypeEntityConfiguration(bool isSqlite) : IEntityTypeConfiguration<CourseEventTypeEntity>
{
    public void Configure(EntityTypeBuilder<CourseEventTypeEntity> e)
    {

        e.ToTable("CourseEventTypes", t =>
        {
            t.HasCheckConstraint("CK_CourseEventTypes_TypeName_NotEmpty", "LTRIM(RTRIM([Name])) <> ''");
        });

        e.HasKey(x => x.Id).HasName("PK_CourseEventTypes_Id");

        e.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        e.Property(x => x.Name)
            .HasMaxLength(20)
            .IsRequired();

        if (isSqlite)
        {
            e.Property(x => x.Concurrency)
                .IsConcurrencyToken()
                .IsRequired(false);
        }
        else
        {
            e.Property(x => x.Concurrency)
                .IsRowVersion()
                .IsConcurrencyToken()
                .IsRequired();
        }

        e.HasIndex(x => x.Name)
            .IsUnique()
            .HasDatabaseName("IX_CourseEventTypes_Name");
    }
}

