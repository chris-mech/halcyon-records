namespace HalcyonRecords.Api.Infrastructure.Options;

public sealed class ShopOptions
{
    public const string SectionName = "Shop";

    public string OrderNumberPrefix { get; init; } = "HR";
    public int OrderNumberPadding { get; init; } = 6;
}
