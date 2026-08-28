namespace HalcyonRecords.Api.Infrastructure.Search;

public sealed class ReindexOptions
{
    public const string SectionName = "Reindex";

    public string? TriggerKey { get; init; }
}
