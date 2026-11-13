using Platform.Application.Abstractions;

namespace Catalog.Application;

public static class CatalogProblems
{
    public static readonly ProblemType ProductNotFound = new("/problems/catalog/product-not-found", 404, "The product does not exist.", UserFacing: true);
    public static readonly ProblemType ProductDiscontinued = new("/problems/catalog/product-discontinued", 409, "The product is discontinued.", UserFacing: true);

    public static IReadOnlyList<ProblemType> All { get; } = [ProductNotFound, ProductDiscontinued];
}
