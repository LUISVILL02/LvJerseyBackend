using Files.Domain.Enums;
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

        builder.Property(e => e.ProcessingStatus)
            .HasColumnName("processing_status")
            .HasDefaultValue(FileProcessingStatus.Pending)
            .HasConversion<int>();

        builder.Property(e => e.ContainerType)
            .HasColumnName("container_type")
            .HasMaxLength(50);

        builder.Property(e => e.TempFilePath)
            .HasColumnName("temp_file_path")
            .HasMaxLength(500);

        builder.Property(e => e.ContentType)
            .HasColumnName("content_type")
            .HasMaxLength(100);

        builder.Property(e => e.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(1000);

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");

        // Índice para búsquedas por jersey
        builder.HasIndex(e => e.IdJersey)
            .HasDatabaseName("IX_Files_IdJersey");

        // Índice para el procesamiento de archivos pendientes
        builder.HasIndex(e => e.ProcessingStatus)
            .HasDatabaseName("IX_Files_ProcessingStatus");
    }
}
