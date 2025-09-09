using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Users.Infrastructure.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.HasKey(e => e.IdAddress).HasName("Addresses_pkey");

        builder.Property(e => e.IdAddress)
            .HasColumnName("id_address");

        builder.Property(e => e.Address1)
            .HasColumnType("character varying")
            .HasColumnName("address");

        builder.Property(e => e.IdUser)
            .HasColumnName("id_user");

        builder.Property(e => e.Neighborhood)
            .HasColumnType("character varying")
            .HasColumnName("neighborhood");

        builder.HasOne(d => d.IdUserNavigation)
            .WithMany(p => p.Adresses)
            .HasForeignKey(d => d.IdUser)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("users_addresses");
    }
}