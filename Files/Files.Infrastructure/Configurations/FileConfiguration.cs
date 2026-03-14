using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using File = Files.Domain.Entities.File;

namespace Files.Infrastructure.Configurations;

public class FileConfiguration : IEntityTypeConfiguration<File>
{
    public void Configure(EntityTypeBuilder<File> builder)
    {
        builder.ToTable("Files", "public");

        builder.HasKey(e => e.IdFile).HasName("Files_pkey");

        builder.Property(e => e.IdFile)
            .HasColumnName("id_file")
            .UseIdentityAlwaysColumn();

        builder.Property(e => e.Url)
            .HasColumnName("url")
            .IsRequired();

        builder.Property(e => e.Name)
            .HasColumnName("name");

        builder.Property(e => e.Format)
            .HasColumnName("format")
            .HasColumnType("character varying[]");

        builder.Property(e => e.IdJersey)
            .HasColumnName("id_jersey");

        builder.Property(e => e.IdUser)
            .HasColumnName("id_usuario");

        // Assuming relationship to Reviews is managed by DB FK, 
        // strictly speaking we cannot configure EF relationship without Review entity.
    }
}
