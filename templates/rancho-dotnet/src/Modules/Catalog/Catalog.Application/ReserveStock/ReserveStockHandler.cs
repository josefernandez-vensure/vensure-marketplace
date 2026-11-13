using Catalog.Contracts.IntegrationEvents;
using Catalog.Domain.Products;
using Platform.Application.Abstractions;

namespace Catalog.Application.ReserveStock;

/// <summary>
/// Every outcome is a reply: the process that asked hears <see cref="StockReservedV1"/> or <see cref="StockReservationFailedV1"/>.
/// A refusal is a reply, not a failed dispatch, so it commits with the reply and is never retried.
/// </summary>
public sealed class ReserveStockHandler(IProductRepository products, IOutbox outbox) : ICommandHandler<ReserveStock>
{
    public async ValueTask<Result> Handle(ReserveStock command, CancellationToken cancellationToken)
    {
        var product = await products.Find(new ProductId(command.ProductId.Value), cancellationToken);
        if (product is null)
        {
            outbox.Publish(new StockReservationFailedV1(command.ReservationId, command.ProductId));
            return Result.Success();
        }

        var outcome = product.ReserveStock(new ReservationId(command.ReservationId.Value), command.Quantity);
        products.Save(product);

        IIntegrationEvent reply = outcome switch
        {
            ReserveStockOutcome.Reserved or ReserveStockOutcome.AlreadyReserved =>
                new StockReservedV1(command.ReservationId, command.ProductId, command.Quantity),
            ReserveStockOutcome.InsufficientStock or ReserveStockOutcome.Discontinued =>
                new StockReservationFailedV1(command.ReservationId, command.ProductId),
        };
        outbox.Publish(reply);
        return Result.Success();
    }
}
