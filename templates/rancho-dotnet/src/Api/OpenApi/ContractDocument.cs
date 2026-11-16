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
/// The OpenAPI document, which is the wire contract. It carries the data class of every property and parameter as <c>x-data-class</c>,
/// the bound of every page as <c>maxItems</c>, every problem type the server declares as the top-level <c>x-problem-types</c>, the
/// <c>Idempotency-Key</c> header of every keyed operation, and the constraints the server holds a parameter to; generating it fails on a
/// value whose class can't be resolved.
/// </summary>
public static class ContractDocument
{
    public const string DataClassExtension = "x-data-class";

    public const string ProblemTypesExtension = "x-problem-types";

    public const string IdempotencyKeyHeader = "Idempotency-Key";

    /// <summary>A GUID on the wire is an id, and no id is the nil GUID: the server never finds one, and a validator refuses one it is given.</summary>
    public const string IdPattern = "^(?!0{8}-0{4}-0{4}-0{4}-0{12}$)[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$";

    public static IServiceCollection AddContractDocument(this IServiceCollection services) =>
        services.AddOpenApi(options =>
        {
            options.AddSchemaTransformer<DataClassTransformer>();
            options.AddSchemaTransformer<PageBoundTransformer>();
            options.AddSchemaTransformer<IdTransformer>();
            options.AddOperationTransformer<OperationContractTransformer>();
            options.AddDocumentTransformer<ProblemTypesTransformer>();

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

/// <summary>Every GUID, in a parameter or a property, refuses the nil GUID.</summary>
internal sealed class IdTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (schema.Format == "uuid")
        {
            schema.Pattern = ContractDocument.IdPattern;
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// What the registrar knows of each operation and the document would otherwise leave out: a keyed operation's required
/// <c>Idempotency-Key</c> header, and a paged query's page-size bounds.
/// </summary>
internal sealed class OperationContractTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.Description.ActionDescriptor.EndpointMetadata.OfType<DiscoveredEndpoint>().FirstOrDefault() is not { } endpoint)
        {
            return Task.CompletedTask;
        }

        operation.Parameters ??= [];
        if (endpoint.Operation.Idempotency == IdempotencyMechanism.Key
            && !operation.Parameters.Any(p => p.In == ParameterLocation.Header && p.Name == ContractDocument.IdempotencyKeyHeader))
        {
            var schema = new OpenApiSchema { Type = JsonSchemaType.String };
            ContractDocument.SetDataClass(schema, DataClass.Internal);
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = ContractDocument.IdempotencyKeyHeader,
                In = ParameterLocation.Header,
                Required = true,
                Description = "One per intent: every retry of the same intent sends the same key, and a replay returns the recorded result.",
                Schema = schema,
            });
        }

        if (typeof(IPagedQuery).IsAssignableFrom(endpoint.MessageType)
            && operation.Parameters.FirstOrDefault(p => p.In == ParameterLocation.Query && p.Name == "pageSize")?.Schema is OpenApiSchema pageSize)
        {
            var maximum = (int)endpoint.MessageType.GetProperty(nameof(IPagedQuery.MaxPageSize), BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;
            pageSize.Minimum = "1";
            pageSize.Maximum = maximum.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        return Task.CompletedTask;
    }
}

/// <summary>The top-level <c>x-problem-types</c>: every problem type the server declares, with its status, its title and whether a user may see that title.</summary>
internal sealed class ProblemTypesTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var types = new JsonArray([.. DeclaredProblems.All.Select(p => (JsonNode)new JsonObject
        {
            ["type"] = p.Uri,
            ["status"] = p.Status,
            ["title"] = p.Title,
            ["userFacing"] = p.UserFacing,
        })]);
        document.Extensions ??= new Dictionary<string, IOpenApiExtension>();
        document.Extensions[ContractDocument.ProblemTypesExtension] = new JsonNodeExtension(types);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Every problem type a production assembly declares as a public static field: the platform's and each module's. Two declarations of one
/// type URI must agree.
/// </summary>
public static class DeclaredProblems
{
    private static readonly Lazy<IReadOnlyList<ProblemType>> Declared = new(Find);

    public static IReadOnlyList<ProblemType> All => Declared.Value;

    private static IReadOnlyList<ProblemType> Find()
    {
        var found = ProductionAssemblies()
            .SelectMany(a => a.GetTypes())
            .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static))
            .Where(f => f.FieldType == typeof(ProblemType))
            .Select(f => (ProblemType)f.GetValue(null)!)
            .Distinct()
            .GroupBy(p => p.Uri)
            .ToList();

        var conflicts = found.Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        return conflicts.Count == 0
            ? [.. found.Select(g => g.Single()).OrderBy(p => p.Uri, StringComparer.Ordinal)]
            : throw new InvalidOperationException($"Problem types declared twice, differently: {string.Join(", ", conflicts)}.");
    }

    /// <summary>The host and every assembly it references that declares a ProjectKind (build/ProjectKinds.targets).</summary>
    private static IEnumerable<Assembly> ProductionAssemblies()
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var queue = new Queue<Assembly>([typeof(ContractDocument).Assembly]);
        while (queue.TryDequeue(out var assembly))
        {
            if (!seen.Add(assembly.GetName().Name!)
                || !assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Any(a => a.Key == "ProjectKind" && !string.IsNullOrEmpty(a.Value)))
            {
                continue;
            }

            yield return assembly;
            foreach (var reference in assembly.GetReferencedAssemblies().Where(r => !seen.Contains(r.Name!)))
            {
                queue.Enqueue(Assembly.Load(reference));
            }
        }
    }
}
