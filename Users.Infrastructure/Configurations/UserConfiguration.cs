using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Users.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(e => e.IdUser).HasName("Users_pkey");

        builder.HasIndex(e => e.Email).IsUnique().HasDatabaseName("Users_email_key");
        builder.HasIndex(e => e.Nikname).IsUnique().HasDatabaseName("Users_nikname_key");

        builder.Property(e => e.IdUser).HasColumnName("id_user");
        builder.Property(e => e.City).HasColumnType("character varying").HasColumnName("city");
        builder.Property(e => e.Country).HasColumnType("character varying").HasColumnName("country");
        builder.Property(e => e.Email).HasColumnType("character varying").HasColumnName("email");
        builder.Property(e => e.IdRol).HasDefaultValue(1).HasColumnName("id_rol");
        builder.Property(e => e.LastName).HasColumnType("character varying").HasColumnName("last_name");
        builder.Property(e => e.Names).HasColumnType("character varying").HasColumnName("names");
        builder.Property(e => e.Nikname).HasColumnType("character varying").HasColumnName("nikname");
        builder.Property(e => e.Password).HasColumnType("character varying").HasColumnName("password");
        builder.Property(e => e.PhoneNumber).HasColumnType("character varying").HasColumnName("phone_number");
        builder.Property(e => e.PostalCode).HasColumnName("postal_code");
        builder.Property(e => e.State).HasColumnType("character varying").HasColumnName("state");

        builder.HasOne(d => d.IdRolNavigation)
            .WithMany(p => p.Users)
            .HasForeignKey(d => d.IdRol)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("users_roles");
    }
}