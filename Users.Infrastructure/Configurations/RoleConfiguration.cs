using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Users.Infrastructure.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(e => e.IdRol).HasName("Roles_pkey");

        builder.Property(e => e.IdRol)
            .HasColumnName("id_rol");

        builder.Property(e => e.Name)
            .HasColumnType("character varying")
            .HasColumnName("name");
    }
}