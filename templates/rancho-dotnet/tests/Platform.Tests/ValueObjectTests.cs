using SharedKernel;
using Shouldly;

namespace Platform.Tests;

/// <summary>The value-object base every module's values derive from: equal by their components and their type.</summary>
public sealed class ValueObjectTests
{
    [Fact]
    public void Values_with_equal_components_are_equal()
    {
        var a = new Money(5, "EUR");
        var b = new Money(5, "EUR");

        a.Equals(b).ShouldBeTrue();
        (a == b).ShouldBeTrue();
        (a != b).ShouldBeFalse();
        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Fact]
    public void Values_that_differ_in_a_component_are_not_equal()
    {
        (new Money(5, "EUR") == new Money(5, "USD")).ShouldBeFalse();
        (new Money(5, "EUR") == new Money(6, "EUR")).ShouldBeFalse();
    }

    [Fact]
    public void Values_of_different_types_are_not_equal_even_with_equal_components()
    {
        new Money(5, "EUR").Equals(new Price(5, "EUR")).ShouldBeFalse();
    }

    [Fact]
    public void A_value_is_never_equal_to_null()
    {
        var a = new Money(5, "EUR");

        a.Equals(null).ShouldBeFalse();
        (a == null).ShouldBeFalse();
        (null == a).ShouldBeFalse();
        ((Money?)null == null).ShouldBeTrue();
    }

    private class Money(decimal amount, string currency) : ValueObject
    {
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return amount;
            yield return currency;
        }
    }

    private sealed class Price(decimal amount, string currency) : Money(amount, currency);
}
