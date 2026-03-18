using Jerseys.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jerseys.Infrastructure.Configurations;

public class SizeConfiguration : IEntityTypeConfiguration<Size>
{
    public void Configure(EntityTypeBuilder<Size> builder)
    {
        builder.ToTable("Sizes", "public");

        builder.HasKey(e => e.IdSize).HasName("Sizes_pkey");

        builder.Property(e => e.IdSize)
            .HasColumnName("id_size")
            .UseIdentityByDefaultColumn();

        builder.Property(e => e.NameSize)
            .HasColumnName("name_size")
            .HasColumnType("character varying");
    }
}
