using Backend.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.EFC.Configurations;

public sealed class InstructorEntityConfiguration(bool isSqlite) : IEntityTypeConfiguration<InstructorEntity>
{
    public void Configure(EntityTypeBuilder<InstructorEntity> e)
    {

        e.ToTable("Instructors", t =>
        {
            t.HasCheckConstraint("CK_Instructors_Name_NotEmpty", "LTRIM(RTRIM([Name])) <> ''");
        });

        e.HasKey(x => x.Id).HasName("PK_Instructors_Id");

        e.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("(NEWSEQUENTIALID())", "DF_Instructors_Id");

        e.Property(x => x.Name)
            .HasMaxLength(50)
            .IsRequired();

        e.Property(x => x.InstructorRoleId)
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

        e.HasOne(x => x.InstructorRole)
            .WithMany(r => r.Instructors)
            .HasForeignKey(x => x.InstructorRoleId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasIndex(x => x.Name)
            .HasDatabaseName("IX_Instructors_Name");
    }
}

