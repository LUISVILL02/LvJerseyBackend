using Jerseys.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jerseys.Infrastructure.Configurations;

public class SizeJerseyConfiguration : IEntityTypeConfiguration<SizeJersey>
{
    public void Configure(EntityTypeBuilder<SizeJersey> builder)
    {
        builder.ToTable("Size_jerseys", "public");

        builder.HasKey(e => new { e.IdJersey, e.IdSize }).HasName("Size_jerseys_pkey");

        builder.Property(e => e.IdJersey)
            .HasColumnName("id_jersey");

        builder.Property(e => e.IdSize)
            .HasColumnName("id_size");

        // FK a Jersey
        builder.HasOne(e => e.Jersey)
            .WithMany(j => j.SizeJerseys)
            .HasForeignKey(e => e.IdJersey)
            .HasConstraintName("Size_jersey_j");

        // FK a Size
        builder.HasOne(e => e.Size)
            .WithMany(s => s.SizeJerseys)
            .HasForeignKey(e => e.IdSize)
            .HasConstraintName("Size_jersey_s");
    }
}
