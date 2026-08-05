namespace HalcyonRecords.SeedDataGenerator.Core.Wikidata;

public sealed record WikidataEntitiesResponse(Dictionary<string, WikidataEntity>? Entities);

public sealed record WikidataEntity(Dictionary<string, WikidataSitelink>? Sitelinks);

public sealed record WikidataSitelink(string? Title);
