using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Domain.Entities;

namespace Orders.Infrastructure.Configurations;

public class ItemsOrderConfiguration : IEntityTypeConfiguration<ItemsOrder>
{
    public void Configure(EntityTypeBuilder<ItemsOrder> builder)
    {
        builder.ToTable("Items_order", "public");
        builder.HasKey(e => e.IdItemOrder).HasName("Items_order_pkey");
        builder.Property(e => e.IdItemOrder).HasColumnName("id_item_order").ValueGeneratedOnAdd();
        builder.Property(e => e.Quantity).HasColumnName("cuantity");
        builder.Property(e => e.Personalitation).HasColumnName("personalitation");
        builder.Property(e => e.Size).HasColumnName("size");
        builder.Property(e => e.IdJersey).HasColumnName("id_jersey");
        builder.Property(e => e.IdOrder).HasColumnName("id_order");
    }
}
