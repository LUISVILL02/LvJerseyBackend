using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Domain.Entities;

namespace Orders.Infrastructure.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Order", "public");
        builder.HasKey(e => e.IdOrder).HasName("Order_pkey");
        builder.Property(e => e.IdOrder).HasColumnName("id_order").ValueGeneratedOnAdd();
        builder.Property(e => e.TotalPrice).HasColumnName("total_price");
        builder.Property(e => e.TotalItems).HasColumnName("total_items");
        builder.Property(e => e.TotalWeight).HasColumnName("total_weight");
        builder.Property(e => e.Email).HasColumnType("character varying").HasColumnName("email");
        builder.Property(e => e.IdUser).HasColumnName("id_user");
        builder.Property(e => e.AddressId).HasColumnType("character varying").HasColumnName("address_id");
        builder.Property(e => e.ShippingCost).HasColumnName("shipping_cost");
        builder.Property(e => e.Status).HasColumnType("character varying").HasColumnName("status");
        builder.Property(e => e.PaymentMethod).HasColumnType("character varying").HasColumnName("payment_method");
        builder.Property(e => e.PaymentStatus).HasColumnType("character varying").HasColumnName("payment_status");
        builder.Property(e => e.PaymentAccountId).HasColumnType("character varying").HasColumnName("payment_account_id");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
    }
}
