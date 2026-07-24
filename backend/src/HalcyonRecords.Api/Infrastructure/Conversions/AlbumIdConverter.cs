using HalcyonRecords.Api.Domain.Ids;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HalcyonRecords.Api.Infrastructure.Conversions;

public sealed class AlbumIdConverter()
    : ValueConverter<AlbumId, int>(id => id.Value, value => new AlbumId(value));
