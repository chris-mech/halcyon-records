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
            t =>
            {
                t.HasCheckConstraint("CK_Albums_Title_NotEmpty", "LEN(Title) > 0");
                t.HasCheckConstraint("CK_Albums_UnitsInStock_NotNegative", "UnitsInStock >= 0");
                t.HasCheckConstraint(
                    "CK_Albums_RestockUnitsInStock_NotNegative",
                    "RestockUnitsInStock >= 0"
                );
                t.HasCheckConstraint("CK_Albums_PriceInPence_NotNegative", "PriceInPence >= 0");
                t.HasCheckConstraint(
                    "CK_Albums_OriginalPriceInPence_GreaterThanPrice",
                    "OriginalPriceInPence IS NULL OR OriginalPriceInPence > PriceInPence"
                );
            }
        );

        builder.Property(a => a.Id).ValueGeneratedOnAdd();

        builder.Property(a => a.Title).HasMaxLength(1000);
        builder.Property(a => a.Label).HasMaxLength(200);

        builder.Property(a => a.ImageUrl).HasMaxLength(500);
    }
}
