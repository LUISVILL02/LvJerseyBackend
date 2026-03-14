using Jerseys.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jerseys.Infrastructure.Configurations;

public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.ToTable("Favorites");

        builder.HasKey(f => new { f.idJersey, f.IdUser });
        
        builder.Property(e => e.idJersey)
            .IsRequired()
            .HasColumnName("id_jersey");
        builder.Property(e => e.IdUser)
            .IsRequired()
            .HasColumnName("id_user");
    }
}