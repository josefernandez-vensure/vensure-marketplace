using Microsoft.Extensions.Compliance.Classification;

namespace SharedKernel;

/// <summary>The data classes, from least to most protected. A value with no class resolves to none, and is treated as <see cref="Restricted"/>.</summary>
public enum DataClass
{
    Public,
    Internal,

    /// <summary>Attributable to a person, who would expect to control it.</summary>
    Confidential,

    /// <summary>Regulated, or enough on its own to impersonate or defraud.</summary>
    Restricted,
}

/// <summary>The classes as Microsoft.Extensions.Compliance classifications, which the log redactors are keyed by.</summary>
public static class DataClasses
{
    public const string Taxonomy = "Rancho";

    public static DataClassification Public { get; } = new(Taxonomy, nameof(DataClass.Public));

    public static DataClassification Internal { get; } = new(Taxonomy, nameof(DataClass.Internal));

    public static DataClassification Confidential { get; } = new(Taxonomy, nameof(DataClass.Confidential));

    public static DataClassification Restricted { get; } = new(Taxonomy, nameof(DataClass.Restricted));

    public static DataClass? ToDataClass(DataClassification classification) =>
        classification.TaxonomyName == Taxonomy && Enum.TryParse<DataClass>(classification.Value, out var dataClass) ? dataClass : null;
}

/// <summary>Where a class may be declared: on a value's type, or on a member or parameter whose type carries none.</summary>
internal static class ClassTargets
{
    public const AttributeTargets Declarable =
        AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter;

    public const AttributeTargets Projection = AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter;
}

[AttributeUsage(ClassTargets.Declarable)]
public sealed class PublicAttribute() : DataClassificationAttribute(DataClasses.Public);

[AttributeUsage(ClassTargets.Declarable)]
public sealed class InternalAttribute() : DataClassificationAttribute(DataClasses.Internal);

[AttributeUsage(ClassTargets.Declarable)]
public sealed class ConfidentialAttribute() : DataClassificationAttribute(DataClasses.Confidential);

[AttributeUsage(ClassTargets.Declarable)]
public sealed class RestrictedAttribute() : DataClassificationAttribute(DataClasses.Restricted);

/// <summary>
/// A member projected from a classified value, such as a column read into a <c>string</c>, takes the class of the type it names:
/// the value's published twin in a module's Contracts. It never restates the class.
/// </summary>
[AttributeUsage(ClassTargets.Projection)]
public sealed class ClassOfAttribute<T> : Attribute, IClassOf
{
    public Type Source => typeof(T);
}

/// <summary>The non-generic face of <see cref="ClassOfAttribute{T}"/>, for readers that don't know <c>T</c>.</summary>
public interface IClassOf
{
    Type Source { get; }
}
