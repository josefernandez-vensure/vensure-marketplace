using Platform.Application.Abstractions;

namespace Ordering.Application;

public static class OrderingProblems
{
    public static readonly ProblemType OrderNotFound = new("/problems/ordering/order-not-found", 404, "The order does not exist.", UserFacing: true);
    public static readonly ProblemType OrderAlreadyConfirmed = new("/problems/ordering/order-already-confirmed", 409, "The order is confirmed and can no longer be cancelled.", UserFacing: true);

    public static IReadOnlyList<ProblemType> All { get; } = [OrderNotFound, OrderAlreadyConfirmed];
}
