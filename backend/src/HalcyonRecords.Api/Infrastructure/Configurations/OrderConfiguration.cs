using HalcyonRecords.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HalcyonRecords.Api.Infrastructure.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(o => o.Id).ValueGeneratedOnAdd();

        builder.Property(o => o.OrderNumber).HasMaxLength(20);
        builder.HasIndex(o => o.OrderNumber).IsUnique();

        builder.HasIndex(o => o.IdempotencyKey).IsUnique();

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
