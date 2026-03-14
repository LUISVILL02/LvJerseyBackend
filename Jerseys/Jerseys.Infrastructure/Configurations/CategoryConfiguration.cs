using Jerseys.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jerseys.Infrastructure.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Category");

        builder.HasKey(c => c.IdCategory).HasName("Category_pkey");

        builder.Property(c => c.IdCategory).HasColumnName("id_category");

        builder.Property(c => c.Name)
            .IsRequired()
            .HasColumnType("varchar")
            .HasColumnName("name");

        builder.HasMany(c => c.CategoriesJerseys)
            .WithOne(cj => cj.Category)
            .HasForeignKey(cj => cj.IdCategory)
            .HasConstraintName("Categ_jer_ca");
    }
}

