using ErrorOr;
using HalcyonRecords.Api.Common.Sqids;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Domain.Ids;
using HalcyonRecords.Api.Infrastructure;
using HalcyonRecords.Shared;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HalcyonRecords.Api.Features.Carts.SyncCart;

public sealed class SyncCartHandler(ApplicationDbContext dbContext, AlbumSqidEncoder albumSqids)
    : IRequestHandler<SyncCartCommand, ErrorOr<Success>>
{
    private const int MaxAttempts = 3;

    public async Task<ErrorOr<Success>> Handle(
        SyncCartCommand command,
        CancellationToken cancellationToken
    )
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(
            u => u.PublicId == command.PublicId,
            cancellationToken
        );
        if (user is null)
        {
            return DomainErrors.Auth.UserNotFound();
        }

        var decoded = command
            .Items.Select(i => (AlbumId: albumSqids.Decode(i.AlbumSqid), i.Quantity))
            .Where(x => x.AlbumId is not null)
            .Select(x => (AlbumId: new AlbumId(x.AlbumId!.Value), x.Quantity))
            .ToList();

        var candidateIds = decoded.Select(x => x.AlbumId).Distinct().ToList();
        var validIds = await dbContext
            .Albums.Where(a => candidateIds.Contains(a.Id))
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);
        var validIdSet = validIds.ToHashSet();

        var incoming = decoded
            .Where(x => validIdSet.Contains(x.AlbumId))
            .GroupBy(x => x.AlbumId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        for (var attempt = 1; ; attempt++)
        {
            var cart = await dbContext
                .Carts.Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == user.Id, cancellationToken);

            if (cart is null)
            {
                cart = new Cart { UserId = user.Id };
                dbContext.Carts.Add(cart);
            }

            var itemsToRemove = cart
                .CartItems.Where(item => !incoming.ContainsKey(item.AlbumId))
                .ToList();
            foreach (var itemToRemove in itemsToRemove)
            {
                cart.CartItems.Remove(itemToRemove);
            }

            foreach (var (albumId, quantity) in incoming)
            {
                var existingItem = cart.CartItems.FirstOrDefault(ci => ci.AlbumId == albumId);
                if (existingItem is not null)
                {
                    existingItem.Quantity = quantity;
                }
                else
                {
                    cart.CartItems.Add(new CartItem { AlbumId = albumId, Quantity = quantity });
                }
            }

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                return Result.Success;
            }
            catch (DbUpdateException ex) when (IsDuplicateKeyViolation(ex) && attempt < MaxAttempts)
            {
                dbContext.ChangeTracker.Clear();
            }
        }
    }

    private static bool IsDuplicateKeyViolation(DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 2601 or 2627 };
}
