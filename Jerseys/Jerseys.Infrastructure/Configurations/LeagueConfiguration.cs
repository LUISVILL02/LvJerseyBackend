using Jerseys.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jerseys.Infrastructure.Configurations;

public class LeagueConfiguration : IEntityTypeConfiguration<League>
{
    public void Configure(EntityTypeBuilder<League> builder)
    {
        builder.ToTable("League");

        builder.HasKey(e => e.IdLeague).HasName("league_pkey");

        builder.Property(e => e.IdLeague)
            .HasColumnName("id_league")
            .UseIdentityColumn();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(40)
            .HasColumnType("character varying(40)")
            .HasColumnName("name");

        builder.Property(e => e.Country)
            .IsRequired()
            .HasMaxLength(40)
            .HasColumnType("character varying(40)")
            .HasColumnName("country");

        // Relación con Club
        builder.HasMany(e => e.Clubs)
            .WithOne(c => c.League)
            .HasForeignKey(c => c.IdLeague)
            .HasConstraintName("fk_league");
    }
}

