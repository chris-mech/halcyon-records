using HalcyonRecords.Api.Domain.Ids;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HalcyonRecords.Api.Infrastructure.Conversions;

public sealed class CartIdConverter()
    : ValueConverter<CartId, int>(id => id.Value, value => new CartId(value));
