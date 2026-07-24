using HalcyonRecords.Api.Domain.Ids;

namespace HalcyonRecords.Api.Domain;

public class Stock
{
    public StockId Id { get; set; }
    public AlbumId AlbumId { get; set; }
    public Album Album { get; set; } = default!;
    public int UnitsInStock { get; set; }
    public int PriceInPence { get; set; }
    public int? OriginalPriceInPence { get; set; }
}
