using SharedKernel;

namespace Ordering.Domain.Orders;

public interface IOrderRepository : IRepository<Order, OrderId>;
