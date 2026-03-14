using Files.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using File = Files.Domain.Entities.File;

namespace Files.Infrastructure.Configurations;

public class FileJerseyConfiguration : IEntityTypeConfiguration<FileJersey>
{
    public void Configure(EntityTypeBuilder<FileJersey> builder)
    {
        builder.ToTable("Files_jerseys", "public");

        builder.HasKey(e => new { e.IdFile, e.IdJersey });

        builder.Property(e => e.IdFile).HasColumnName("id_file");
        builder.Property(e => e.IdJersey).HasColumnName("id_jersey");

        builder.HasOne(d => d.File)
            .WithMany(p => p.FileJerseys)
            .HasForeignKey(d => d.IdFile)
            .HasConstraintName("Files_jers_f");
    }
}

