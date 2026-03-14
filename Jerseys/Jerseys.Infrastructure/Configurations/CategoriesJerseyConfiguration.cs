using Jerseys.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jerseys.Infrastructure.Configurations;

public class CategoriesJerseyConfiguration : IEntityTypeConfiguration<CategoriesJersey>
{
    public void Configure(EntityTypeBuilder<CategoriesJersey> builder)
    {
        builder.ToTable("Categories_jerseys");

        // Clave primaria compuesta
        builder.HasKey(cj => new { cj.IdCategory, cj.IdJersey })
            .HasName("Categories_jerseys_pkey");

        builder.Property(cj => cj.IdCategory)
            .HasColumnName("id_category");

        builder.Property(cj => cj.IdJersey)
            .HasColumnName("id_jersey");

        // Relación con Category
        builder.HasOne(cj => cj.Category)
            .WithMany(c => c.CategoriesJerseys)
            .HasForeignKey(cj => cj.IdCategory)
            .HasConstraintName("Categ_jer_ca");

        // Relación con Jersey
        builder.HasOne(cj => cj.Jersey)
            .WithMany(j => j.CategoriesJerseys)
            .HasForeignKey(cj => cj.IdJersey)
            .HasConstraintName("Categ_jer_j");
    }
}
