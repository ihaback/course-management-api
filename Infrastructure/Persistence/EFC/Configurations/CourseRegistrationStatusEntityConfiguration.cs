using Backend.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.EFC.Configurations;

public sealed class CourseRegistrationStatusEntityConfiguration(bool isSqlite) : IEntityTypeConfiguration<CourseRegistrationStatusEntity>
{
    public void Configure(EntityTypeBuilder<CourseRegistrationStatusEntity> e)
    {

        e.ToTable("CourseRegistrationStatuses");

        e.HasKey(x => x.Id).HasName("PK_CourseRegistrationStatuses_Id");

        e.Property(x => x.Id)
            .ValueGeneratedNever();

        e.Property(x => x.Name)
            .HasMaxLength(50)
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
            .HasDatabaseName("IX_CourseRegistrationStatuses_Name");

    }
}
