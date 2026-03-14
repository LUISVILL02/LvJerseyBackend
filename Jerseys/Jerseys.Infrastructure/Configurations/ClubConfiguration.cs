using Jerseys.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jerseys.Infrastructure.Configurations;

public class ClubConfiguration : IEntityTypeConfiguration<Club>
{
    public void Configure(EntityTypeBuilder<Club> builder)
    {
        builder.ToTable("Club");

        builder.HasKey(e => e.IdClub).HasName("club_pkey");

        builder.Property(e => e.IdClub)
            .HasColumnName("id_club")
            .UseIdentityColumn();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(40)
            .HasColumnType("character varying(40)")
            .HasColumnName("name");

        builder.Property(e => e.IdLeague)
            .IsRequired()
            .HasColumnName("id_league");

        // Relación con Jersey
        builder.HasMany(e => e.Jerseys)
            .WithOne(j => j.ClubNavigation)
            .HasForeignKey(j => j.IdClub)
            .HasConstraintName("fk_club");
    }
}

