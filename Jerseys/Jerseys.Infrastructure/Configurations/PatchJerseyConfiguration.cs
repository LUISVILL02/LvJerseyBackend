using Jerseys.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jerseys.Infrastructure.Configurations;

public class PatchJerseyConfiguration : IEntityTypeConfiguration<PatchJersey>
{
    public void Configure(EntityTypeBuilder<PatchJersey> builder)
    {
        builder.ToTable("Patches_jerseys");

        // Clave primaria compuesta
        builder.HasKey(pj => new { pj.IdJersey, pj.IdPatch })
            .HasName("Patches_jerseys_pkey");

        builder.Property(pj => pj.IdJersey)
            .HasColumnName("id_jersey");

        builder.Property(pj => pj.IdPatch)
            .HasColumnName("id_patch");

        // Relaci con Patch
        builder.HasOne(pj => pj.Patch)
            .WithMany(p => p.PatchJerseys)
            .HasForeignKey(pj => pj.IdPatch)
            .HasConstraintName("Patch_jer_p");

        // Relaci con Jersey
        builder.HasOne(pj => pj.Jersey)
            .WithMany(j => j.PatchJerseys)
            .HasForeignKey(pj => pj.IdJersey)
            .HasConstraintName("Patch_jer_j");
    }
}
