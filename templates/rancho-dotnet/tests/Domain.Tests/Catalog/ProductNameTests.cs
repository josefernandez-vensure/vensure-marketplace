using Catalog.Domain.Products;
using Shouldly;

namespace Domain.Tests.Catalog;

public sealed class ProductNameTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void New_Throws_WhenTheNameIsBlank(string? value) =>
        Should.Throw<ArgumentException>(() => new ProductName(value!));

    [Fact]
    public void New_Throws_WhenTheNameIsLongerThanTheMaximum() =>
        Should.Throw<ArgumentOutOfRangeException>(() => new ProductName(new string('a', ProductName.MaxLength + 1)));

    [Fact]
    public void New_AcceptsTheMaximumLength_AfterTrimming() =>
        new ProductName($"  {new string('a', ProductName.MaxLength)}  ").Value.Length.ShouldBe(ProductName.MaxLength);
}
