using Backend.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.EFC.Configurations;

public sealed class InstructorRoleEntityConfiguration(bool isSqlite) : IEntityTypeConfiguration<InstructorRoleEntity>
{
    public void Configure(EntityTypeBuilder<InstructorRoleEntity> e)
    {

        e.ToTable("InstructorRoles", t =>
        {
            t.HasCheckConstraint(
                "CK_InstructorRoles_RoleName_NotEmpty",
                isSqlite
                    ? "LTRIM(RTRIM([Name])) <> ''"
                    : "LEN([Name]) > 0");
        });

        e.HasKey(x => x.Id).HasName("PK_InstructorRoles_Id");

        e.Property(x => x.Id)
            .ValueGeneratedOnAdd();

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
            .HasDatabaseName("IX_InstructorRoles_Name");
    }
}
