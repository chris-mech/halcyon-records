using HalcyonRecords.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HalcyonRecords.Api.Infrastructure.Configurations;

public sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable(
            "CartItems",
            t => t.HasCheckConstraint("CK_CartItems_Quantity_Positive", "Quantity > 0")
        );

        builder.HasKey(ci => new { ci.CartId, ci.AlbumId });

        builder.HasOne(ci => ci.Cart).WithMany(c => c.CartItems).HasForeignKey(ci => ci.CartId);

        builder.HasOne(ci => ci.Album).WithMany().HasForeignKey(ci => ci.AlbumId);
    }
}
