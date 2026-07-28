namespace HalcyonRecords.SeedDataGenerator.Core.MusicBrainz;

public sealed class MusicBrainzOptions
{
    public const string SectionName = "MusicBrainz";

    public string BaseAddress { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
}
