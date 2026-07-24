using HalcyonRecords.Api.Domain.Ids;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HalcyonRecords.Api.Infrastructure.Conversions;

public sealed class ArtistIdConverter()
    : ValueConverter<ArtistId, int>(id => id.Value, value => new ArtistId(value));
