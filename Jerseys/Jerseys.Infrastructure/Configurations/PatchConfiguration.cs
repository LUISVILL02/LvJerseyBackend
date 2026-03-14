using Jerseys.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jerseys.Infrastructure.Configurations;

public class PatchConfiguration : IEntityTypeConfiguration<Patch>
{
    public void Configure(EntityTypeBuilder<Patch> builder)
    {
        builder.ToTable("Patches");

        builder.HasKey(e => e.IdPatch).HasName("Patches_pkey");

        builder.Property(e => e.IdPatch)
            .HasColumnName("id_patch")
            .UseIdentityColumn();

        builder.Property(e => e.NamePatch)
            .HasColumnType("character varying")
            .HasColumnName("name_patch");

        builder.Property(e => e.Season)
            .HasColumnType("character varying")
            .HasColumnName("season");
    }
}
