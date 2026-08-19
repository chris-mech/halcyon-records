using HalcyonRecords.Api.Domain.Ids;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HalcyonRecords.Api.Infrastructure.Conversions;

public sealed class OrderIdConverter()
    : ValueConverter<OrderId, int>(id => id.Value, value => new OrderId(value));
