using Microsoft.EntityFrameworkCore;
using Ordering.Application;
using Ordering.Application.GetOrder;
using Ordering.Infrastructure.Persistence;
using Platform.Application.Abstractions;

namespace Ordering.Infrastructure.Queries.GetOrder;

/// <summary>Public because the dispatcher's generated code names it. It reads with SQL straight into the DTO, within the principal's tenant.</summary>
public sealed class GetOrderHandler(OrderingDbContext db, DispatchContext dispatch) : IQueryHandler<Application.GetOrder.GetOrder, OrderResponse>
{
    public async ValueTask<Result<OrderResponse>> Handle(Application.GetOrder.GetOrder query, CancellationToken cancellationToken)
    {
        var id = query.OrderId.Value;
        var tenant = dispatch.Tenant?.Value;
        var row = await db.Database
            .SqlQuery<OrderRow>($"SELECT Id, ProductId, Quantity, Status, Version FROM ordering.Orders WHERE Id = {id} AND TenantId = {tenant}")
            .SingleOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return OrderingProblems.OrderNotFound.With();
        }

        return new OrderResponse(new Contracts.OrderId(row.Id), new Catalog.Contracts.ProductId(row.ProductId), row.Quantity, row.Status, $"\"{row.Version}\"");
    }

    private sealed record OrderRow(Guid Id, Guid ProductId, int Quantity, string Status, long Version);
}
