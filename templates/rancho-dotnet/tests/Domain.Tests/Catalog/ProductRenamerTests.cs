using Catalog.Domain.Products;
using Shouldly;

namespace Domain.Tests.Catalog;

public sealed class ProductRenamerTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 12, 9, 0, 0, TimeSpan.Zero);

    private readonly Products _products = new();

    [Fact]
    public async Task RenameAll_ReturnsRenamed_AndRenamesEveryProduct()
    {
        var lamp = _products.Add("Lamp");
        var desk = _products.Add("Desk");

        var outcome = await Renamer.RenameAll(Names((lamp, "Floor lamp"), (desk, "Writing desk")), TestContext.Current.CancellationToken);

        outcome.ShouldBe(RenameProductsOutcome.Renamed);
        lamp.Name.ShouldBe(new ProductName("Floor lamp"));
        desk.Name.ShouldBe(new ProductName("Writing desk"));
        _products.Saved.ShouldBe([lamp, desk], ignoreOrder: true);
    }

    [Fact]
    public async Task RenameAll_ReturnsProductNotFound_AndRenamesNone_WhenOneIsMissing()
    {
        var lamp = _products.Add("Lamp");
        var missing = Product.Create(ProductId.New(), new ProductName("Ghost"), Now);

        var outcome = await Renamer.RenameAll(Names((lamp, "Floor lamp"), (missing, "Spirit")), TestContext.Current.CancellationToken);

        outcome.ShouldBe(RenameProductsOutcome.ProductNotFound);
        lamp.Name.ShouldBe(new ProductName("Lamp"));
        _products.Saved.ShouldBeEmpty();
    }

    [Fact]
    public async Task RenameAll_ReturnsDiscontinued_AndRenamesNone_WhenOneIsDiscontinued()
    {
        var lamp = _products.Add("Lamp");
        var desk = _products.Add("Desk");
        desk.Discontinue();

        var outcome = await Renamer.RenameAll(Names((lamp, "Floor lamp"), (desk, "Writing desk")), TestContext.Current.CancellationToken);

        outcome.ShouldBe(RenameProductsOutcome.Discontinued);
        lamp.Name.ShouldBe(new ProductName("Lamp"));
        lamp.Version.ShouldBe(1);
        _products.Saved.ShouldBeEmpty();
    }

    private ProductRenamer Renamer => new(_products);

    private static Dictionary<ProductId, ProductName> Names(params (Product Product, string Name)[] names) =>
        names.ToDictionary(n => n.Product.Id, n => new ProductName(n.Name));

    /// <summary>The repository port, in memory: what it holds, and what the service staged.</summary>
    private sealed class Products : IProductRepository
    {
        private readonly List<Product> _held = [];

        public List<Product> Saved { get; } = [];

        public Product Add(string name)
        {
            var product = Product.Create(ProductId.New(), new ProductName(name), Now);
            _held.Add(product);
            return product;
        }

        public Task<Product?> Find(ProductId id, CancellationToken cancellationToken) =>
            Task.FromResult(_held.SingleOrDefault(p => p.Id == id));

        public Task<IReadOnlyList<Product>> FindAll(IReadOnlyCollection<ProductId> ids, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Product>>([.. _held.Where(p => ids.Contains(p.Id))]);

        public void Save(Product root) => Saved.Add(root);
    }
}
