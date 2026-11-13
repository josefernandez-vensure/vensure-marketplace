using Microsoft.EntityFrameworkCore;
using Ordering.Domain.Orders;
using Platform.Application.Abstractions;

namespace Ordering.Infrastructure.Persistence;

/// <summary>Loads and saves orders within the acting principal's tenant: another tenant's order is never found.</summary>
internal sealed class OrderRepository(OrderingDbContext db, DispatchContext dispatch) : IOrderRepository
{
    public const string TenantColumn = "TenantId";

    public Task<Order?> Find(OrderId id, CancellationToken cancellationToken) =>
        dispatch.Tenant is { Value: var tenant }
            ? db.Orders.SingleOrDefaultAsync(o => o.Id == id && EF.Property<Guid>(o, TenantColumn) == tenant, cancellationToken)
            : Task.FromResult<Order?>(null);

    public void Save(Order root)
    {
        if (db.Entry(root).State == EntityState.Detached)
        {
            var tenant = dispatch.Tenant ?? throw new InvalidOperationException("An order is placed only within a tenant; the dispatch context has none.");
            db.Orders.Add(root).Property(TenantColumn).CurrentValue = tenant.Value;
        }
    }
}
