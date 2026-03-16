using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reviews.Domain.Entities;

namespace Reviews.Infrastructure.Configurations;

public class CriteriaConfiguration : IEntityTypeConfiguration<Criteria>
{
    public void Configure(EntityTypeBuilder<Criteria> builder)
    {
        builder.ToTable("Criterias", "public");

        builder.HasKey(e => e.IdCriteria).HasName("Criterias_pkey");

        builder.Property(e => e.IdCriteria)
            .HasColumnName("id_criteria")
            .UseIdentityByDefaultColumn();

        builder.Property(e => e.CriteriaType)
            .HasColumnName("criteria_type")
            .HasColumnType("character varying")
            .IsRequired();

        builder.Property(e => e.Value)
            .HasColumnName("value")
            .HasColumnType("decimal")
            .IsRequired();

        builder.Property(e => e.Comment)
            .HasColumnName("comment")
            .HasColumnType("character varying");

        builder.Property(e => e.IdJersey)
            .HasColumnName("id_jersey");

        builder.Property(e => e.IdUser)
            .HasColumnName("id_usuario");

        // FK a Review (compuesta)
        builder.HasOne(e => e.Review)
            .WithMany(r => r.Criterias)
            .HasForeignKey(e => new { e.IdJersey, e.IdUser })
            .HasConstraintName("Criteria_review_fkey");
    }
}
