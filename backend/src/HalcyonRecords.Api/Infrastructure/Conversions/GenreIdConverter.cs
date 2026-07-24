using HalcyonRecords.Api.Domain.Ids;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HalcyonRecords.Api.Infrastructure.Conversions;

public sealed class GenreIdConverter()
    : ValueConverter<GenreId, int>(id => id.Value, value => new GenreId(value));
