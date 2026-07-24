using HalcyonRecords.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HalcyonRecords.Api.Infrastructure.Configurations;

public sealed class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        builder.ToTable(
            "Stocks",
            t =>
            {
                t.HasCheckConstraint("CK_Stocks_UnitsInStock_NotNegative", "UnitsInStock >= 0");
                t.HasCheckConstraint("CK_Stocks_PriceInPence_NotNegative", "PriceInPence >= 0");
                t.HasCheckConstraint(
                    "CK_Stocks_OriginalPriceInPence_NotNegative",
                    "OriginalPriceInPence IS NULL OR OriginalPriceInPence >= 0"
                );
                t.HasCheckConstraint(
                    "CK_Stocks_OriginalPriceInPence_GreaterThanPrice",
                    "OriginalPriceInPence IS NULL OR OriginalPriceInPence > PriceInPence"
                );
            }
        );
    }
}
