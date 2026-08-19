using ErrorOr;
using HalcyonRecords.Api.Common.Contracts;
using HalcyonRecords.Api.Common.Sqids;
using HalcyonRecords.Api.Infrastructure;
using HalcyonRecords.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HalcyonRecords.Api.Features.Orders.GetOrders;

public sealed class GetOrdersHandler(ApplicationDbContext dbContext, AlbumSqidEncoder albumSqids)
    : IRequestHandler<GetOrdersQuery, ErrorOr<PagedResult<OrderSummaryResponse>>>
{
    public async Task<ErrorOr<PagedResult<OrderSummaryResponse>>> Handle(
        GetOrdersQuery query,
        CancellationToken cancellationToken
    )
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(
            u => u.PublicId == query.PublicId,
            cancellationToken
        );
        if (user is null)
        {
            return DomainErrors.Auth.UserNotFound();
        }

        var orders = dbContext
            .Orders.Where(o => o.UserId == user.Id)
            .OrderByDescending(o => o.PlacedAt)
            .ThenBy(o => o.Id);

        var totalCount = await orders.CountAsync(cancellationToken);

        var page = await orders
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(o => new
            {
                o.OrderNumber,
                o.PlacedAt,
                o.Status,
                o.TotalInPence,
                Items = o.OrderItems.Select(oi => new
                {
                    oi.AlbumId,
                    oi.Album.Title,
                    oi.Album.ImageUrl,
                }),
            })
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        var items = page.Select(o => new OrderSummaryResponse(
                o.OrderNumber,
                o.PlacedAt,
                o.Status.ToString(),
                o.TotalInPence,
                o.Items.Select(i => new OrderSummaryItemResponse(
                        albumSqids.Encode(i.AlbumId.Value),
                        i.Title,
                        Slugifier.Slugify(i.Title),
                        i.ImageUrl
                    ))
                    .ToList()
            ))
            .ToList();

        return new PagedResult<OrderSummaryResponse>(items, query.Page, query.PageSize, totalCount);
    }
}
