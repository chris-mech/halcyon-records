using HalcyonRecords.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HalcyonRecords.Api.Infrastructure.Configurations;

public sealed class DecadeConfiguration : IEntityTypeConfiguration<Decade>
{
    public void Configure(EntityTypeBuilder<Decade> builder)
    {
        builder.ToTable("Decades");

        builder.HasKey(d => d.Slug);
        builder.Property(d => d.Slug).HasMaxLength(50);

        builder.Property(d => d.Label).HasMaxLength(50);
        builder.Property(d => d.ImageUrl).HasMaxLength(500);
    }
}
