using HalcyonRecords.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HalcyonRecords.Api.Infrastructure.Configurations;

public sealed class AlbumConfiguration : IEntityTypeConfiguration<Album>
{
    public void Configure(EntityTypeBuilder<Album> builder)
    {
        builder.ToTable(
            "Albums",
            t => t.HasCheckConstraint("CK_Albums_Title_NotEmpty", "LEN(Title) > 0")
        );

        builder.Property(a => a.Title).HasMaxLength(1000);
        builder.Property(a => a.Label).HasMaxLength(200);

        builder.Property(a => a.Slug).HasMaxLength(200);
        builder.HasIndex(a => a.Slug).IsUnique();

        builder.Property(a => a.ImageUrl).HasMaxLength(500);

        builder
            .HasOne(a => a.Stock)
            .WithOne(s => s.Album)
            .HasForeignKey<Stock>(s => s.AlbumId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
