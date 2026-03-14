using Jerseys.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jerseys.Infrastructure.Configurations;

public class JerseyConfiguration : IEntityTypeConfiguration<Jersey>
{
    public void Configure(EntityTypeBuilder<Jersey> builder)
    {
        builder.ToTable("Jersey");

        builder.HasKey(e => e.IdJersey).HasName("Jersey_pkey");

        builder.Property(e => e.IdJersey).HasColumnName("id_jersey");
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasColumnType("text")
            .HasColumnName("name");

        builder.Property(e => e.Weight)
            .IsRequired()
            .HasColumnType("numeric")
            .HasColumnName("weight");

        // Valores esperados: Player, Fan, Retro
        builder.Property(e => e.Type)
            .HasColumnType("character varying")
            .HasColumnName("type")
            .HasComment("Player, Fan, Retro");

        builder.Property(e => e.Brand)
            .HasColumnType("character varying")
            .HasColumnName("brand");

        builder.Property(e => e.Season)
            .HasColumnType("character varying")
            .HasColumnName("season");

        builder.Property(e => e.Price)
            .HasColumnType("numeric")
            .HasColumnName("price");

        builder.Property(e => e.Sex)
            .HasColumnType("character varying")
            .HasColumnName("sex");

        builder.Property(e => e.Stock)
            .HasColumnName("stock");

        builder.Property(e => e.IdClub)
            .IsRequired()
            .HasColumnName("id_club");

        builder.Property(e => e.ClubName)
            .HasColumnType("character varying")
            .HasColumnName("club");

        builder.HasMany(j => j.FavoriteJerseys)
            .WithOne(f => f.Jersey)
            .HasForeignKey(j => j.idJersey)
            .HasConstraintName("Favorite_jerseys");

        builder.HasOne(j => j.ClubNavigation)
            .WithMany(c => c.Jerseys)
            .HasForeignKey(j => j.IdClub)
            .HasConstraintName("Jersey_club_fkey");
    }
}