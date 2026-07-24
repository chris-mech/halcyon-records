using HalcyonRecords.Api.Domain.Ids;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HalcyonRecords.Api.Infrastructure.Conversions;

public sealed class StockIdConverter()
    : ValueConverter<StockId, int>(id => id.Value, value => new StockId(value));
