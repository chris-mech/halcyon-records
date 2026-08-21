using HalcyonRecords.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HalcyonRecords.Api.Infrastructure.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable(
            "OrderItems",
            t => t.HasCheckConstraint("CK_OrderItems_Quantity_Positive", "Quantity > 0")
        );

        builder.HasKey(oi => new { oi.OrderId, oi.AlbumId });

        builder
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(oi => oi.Album)
            .WithMany()
            .HasForeignKey(oi => oi.AlbumId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
