using Files.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using File = Files.Domain.Entities.File;

namespace Files.Infrastructure.Configurations;

public class FilePatchConfiguration : IEntityTypeConfiguration<FilePatch>
{
    public void Configure(EntityTypeBuilder<FilePatch> builder)
    {
        builder.ToTable("Files_patches", "public");

        builder.HasKey(e => new { e.IdFile, e.IdPatch });

        builder.Property(e => e.IdFile).HasColumnName("id_file");
        builder.Property(e => e.IdPatch).HasColumnName("id_patch");

        builder.HasOne(d => d.File)
            .WithMany(p => p.FilePatches)
            .HasForeignKey(d => d.IdFile)
            .HasConstraintName("Patch_Files_f");
            
        // Assuming Patch entity exists in DB but not in our Domain yet, or similar to Jersey
        // If Patch doesn't exist as a class, we map the column but can't configure HasOne w/o generic type.
        // Since we don't have Patch class in workspace, we skip HasOne(Patch).
        // The DB FK "Patch_Files_p" will handle integrity.
    }
}

