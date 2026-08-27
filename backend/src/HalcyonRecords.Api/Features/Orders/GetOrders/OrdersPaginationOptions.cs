namespace HalcyonRecords.Api.Features.Orders.GetOrders;

public sealed class OrdersPaginationOptions
{
    public const string SectionName = "OrdersPagination";

    public int MinPage { get; init; } = 1;

    public int MinPageSize { get; init; } = 1;

    public int MaxPageSize { get; init; } = 25;

    public int DefaultPageSize { get; init; } = 10;
}
