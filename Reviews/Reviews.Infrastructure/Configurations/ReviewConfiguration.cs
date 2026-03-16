using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reviews.Domain.Entities;

namespace Reviews.Infrastructure.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews", "public");

        builder.HasKey(e => new { e.IdJersey, e.IdUser }).HasName("Reviews_pkey");

        builder.Property(e => e.IdJersey)
            .HasColumnName("id_jersey");

        builder.Property(e => e.IdUser)
            .HasColumnName("id_user");

        builder.Property(e => e.GeneralComment)
            .HasColumnName("general_comment")
            .HasColumnType("text");

        builder.Property(e => e.DateReview)
            .HasColumnName("date_review")
            .HasColumnType("timestamp");

        // Relación con Criterias
        builder.HasMany(e => e.Criterias)
            .WithOne(c => c.Review)
            .HasForeignKey(c => new { c.IdJersey, c.IdUser })
            .HasConstraintName("Criteria_review_fkey");
    }
}
