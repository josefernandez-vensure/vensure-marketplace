using System.Reflection;
using System.Text.Json.Nodes;
using Api.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Platform.Application.Abstractions;
using SharedKernel;

namespace Api.OpenApi;

/// <summary>
/// The OpenAPI document, which is the wire contract. It carries the data class of every property and parameter as <c>x-data-class</c>
/// and the bound of every page as <c>maxItems</c>; generating it fails on a value whose class can't be resolved.
/// </summary>
public static class ContractDocument
{
    public const string DataClassExtension = "x-data-class";

    public static IServiceCollection AddContractDocument(this IServiceCollection services) =>
        services.AddOpenApi(options =>
        {
            options.AddSchemaTransformer<DataClassTransformer>();
            options.AddSchemaTransformer<PageBoundTransformer>();

            // The document describes the contract, not the host that served it.
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Servers = [];
                return Task.CompletedTask;
            });
        });

    /// <summary>The wire name of a class, as <c>x-data-class</c> carries it.</summary>
    public static string WireName(DataClass dataClass) => dataClass.ToString().ToLowerInvariant();

    internal static void SetDataClass(OpenApiSchema schema, DataClass dataClass)
    {
        schema.Extensions ??= new Dictionary<string, IOpenApiExtension>();
        schema.Extensions[DataClassExtension] = new JsonNodeExtension(JsonValue.Create(WireName(dataClass)));
    }
}

/// <summary>Puts each property's and parameter's class on its schema, and refuses one it can't resolve.</summary>
internal sealed class DataClassTransformer : IOpenApiSchemaTransformer
{
    /// <summary>Framework schemas: their members carry declared problem types and field errors, never a classified value.</summary>
    private static readonly Dictionary<Type, DataClass> Framework = new()
    {
        [typeof(ProblemDetails)] = DataClass.Internal,
        [typeof(HttpValidationProblemDetails)] = DataClass.Internal,
    };

    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.JsonPropertyInfo is { } property)
        {
            ContractDocument.SetDataClass(schema, Require(OfProperty(property.DeclaringType, property.AttributeProvider, property.PropertyType), $"{property.DeclaringType.Name}.{property.Name}"));
        }
        else if (context.ParameterDescription is { } parameter && !IsBody(parameter))
        {
            ContractDocument.SetDataClass(schema, Require(OfParameter(parameter), parameter.Name));
        }
        else if (Of(context.JsonTypeInfo.Type) is { IsResolved: true, Class: { } typeClass })
        {
            // A component carries its type's class too, so a property that refers to it inherits it.
            ContractDocument.SetDataClass(schema, typeClass);
        }

        return Task.CompletedTask;
    }

    private static DataClassResolution Of(Type type) =>
        Framework.TryGetValue(type, out var framework) ? new(framework, []) : DataClassResolver.Resolve(type);

    private static DataClassResolution OfProperty(Type declaringType, ICustomAttributeProvider? member, Type propertyType)
    {
        if (Framework.Keys.FirstOrDefault(t => t.IsAssignableFrom(declaringType)) is { } framework)
        {
            return new(Framework[framework], []);
        }

        return member switch
        {
            PropertyInfo p => DataClassResolver.Resolve(p),
            FieldInfo f => DataClassResolver.Resolve(f),
            _ => DataClassResolver.Resolve(propertyType),
        };
    }

    private static DataClassResolution OfParameter(ApiParameterDescription parameter) =>
        parameter.ParameterDescriptor is IParameterInfoParameterDescriptor { ParameterInfo: { } info }
            ? DataClassResolver.Resolve(info)
            : DataClassResolver.Resolve(parameter.Type);

    private static bool IsBody(ApiParameterDescription parameter) =>
        parameter.Source == BindingSource.Body || parameter.Source == BindingSource.Form || parameter.Source == BindingSource.FormFile;

    private static DataClass Require(DataClassResolution resolution, string where) =>
        resolution.IsResolved
            ? resolution.Class!.Value
            : throw new InvalidOperationException(
                $"{where} has no data class, so it is Restricted and can't be published: {string.Join("; ", resolution.Unresolved)}. "
                + "Declare the class on the value's type, or on the member with [Public], [Internal], [Confidential], [Restricted] or [ClassOf<T>].");
}

/// <summary>A page's <c>items</c> carry <c>maxItems</c>: the largest maximum page size of the paged queries that return it.</summary>
internal sealed class PageBoundTransformer : IOpenApiSchemaTransformer
{
    private static readonly Lazy<Dictionary<Type, int>> Bounds = new(() =>
        EndpointRegistrar.Discover()
            .Select(d => d.Message)
            .Select(m => (Message: m, Item: m.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPagedQuery<>))?.GetGenericArguments()[0]))
            .Where(x => x.Item is not null)
            .GroupBy(x => x.Item!)
            .ToDictionary(g => g.Key, g => g.Max(x => (int)x.Message.GetProperty(nameof(IPagedQuery.MaxPageSize), BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!)));

    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.JsonPropertyInfo is { AttributeProvider: PropertyInfo { Name: nameof(Page<int>.Items) } } property
            && property.DeclaringType.IsGenericType
            && property.DeclaringType.GetGenericTypeDefinition() == typeof(Page<>))
        {
            var item = property.DeclaringType.GetGenericArguments()[0];
            schema.MaxItems = Bounds.Value.TryGetValue(item, out var bound)
                ? bound
                : throw new InvalidOperationException($"Page<{item.Name}> is in the document, but no paged query that returns it declares a maximum page size.");
        }

        return Task.CompletedTask;
    }
}
