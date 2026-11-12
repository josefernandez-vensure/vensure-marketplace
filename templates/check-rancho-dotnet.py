"""Prove the rancho-dotnet template's checks bite.

Seeds one violation per check into a copy of the template and confirms the build
or the architecture tests fail on it. Run it after changing a check.

Usage, from the repository root:  python -I templates/check-rancho-dotnet.py
"""
import functools
import os
import shutil
import subprocess
import sys
import tempfile
from pathlib import Path

print = functools.partial(print, flush=True)

SRC = Path(__file__).resolve().parent / "rancho-dotnet"
WORK = Path(tempfile.mkdtemp(prefix="rancho-dotnet-seeded-"))
CAT = "src/Modules/Catalog/"


def fresh():
    if WORK.exists():
        shutil.rmtree(WORK)
    shutil.copytree(SRC, WORK, ignore=shutil.ignore_patterns("bin", "obj"))


def edit(path, old, new):
    p = WORK / path
    s = p.read_text(encoding="utf-8")
    assert old in s, f"{old!r} not in {path}"
    p.write_text(s.replace(old, new, 1), encoding="utf-8")


def edit_glob(pattern, old, new):
    matches = list(WORK.glob(pattern))
    assert len(matches) == 1, f"{pattern} matches {matches}"
    edit(str(matches[0].relative_to(WORK)), old, new)


def write(path, text):
    p = WORK / path
    p.parent.mkdir(parents=True, exist_ok=True)
    p.write_text(text, encoding="utf-8")


def run(*args):
    r = subprocess.run(["dotnet", *args], cwd=WORK, capture_output=True, text=True, encoding="utf-8", errors="replace")
    return r.returncode, r.stdout + r.stderr


DOMAIN = CAT + "Catalog.Domain/Catalog.Domain.csproj"
UOW = "src/Platform/Platform.Infrastructure/Persistence/UnitOfWork.cs"
LIST = CAT + "Catalog.Infrastructure/Queries/ListProducts/ListProductsHandler.cs"
PRODUCT = CAT + "Catalog.Domain/Products/Product.cs"
HANDLER = CAT + "Catalog.Application/CreateProduct/CreateProductHandler.cs"
QH = CAT + "Catalog.Infrastructure/Queries/GetProduct/GetProductHandler.cs"
REPO = CAT + "Catalog.Infrastructure/Persistence/ProductRepository.cs"

# Build-time seeds: (name, expected text in build output, edits)
BUILD = [
    ("Domain references Platform.Infrastructure", "may not reference Platform.Infrastructure",
     lambda: edit(DOMAIN, '<ProjectReference Include="..\\..\\..\\Shared\\SharedKernel\\SharedKernel.csproj" />',
                  '<ProjectReference Include="..\\..\\..\\Shared\\SharedKernel\\SharedKernel.csproj" /><ProjectReference Include="..\\..\\..\\Platform\\Platform.Infrastructure\\Platform.Infrastructure.csproj" />')),
    ("Application takes EF Core", "may not reference package Microsoft.EntityFrameworkCore",
     lambda: edit(CAT + "Catalog.Application/Catalog.Application.csproj", "</Project>",
                  '<ItemGroup><PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.15" /></ItemGroup></Project>')),
    ("A test stand-in provider", "is denied in every project",
     lambda: edit("tests/Architecture.Tests/Architecture.Tests.csproj", "</Project>",
                  '<ItemGroup><PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="10.0.15" /></ItemGroup></Project>')),
    ("A project with no kind", "<ProjectKind> is ''",
     lambda: edit(DOMAIN, "<ProjectKind>Domain</ProjectKind>", "")),
    ("Domain reads the clock", "RS0030",
     lambda: edit(PRODUCT, "public void Discontinue()", "public static DateTime Clock() => DateTime.UtcNow;\n\n    public void Discontinue()")),
    ("Blocking on a task in an async method", "VSTHRD103",
     lambda: edit(HANDLER, "var id =", "_ = Task.FromResult(1).Result;\n        var id =")),
    ("async void", "VSTHRD100",
     lambda: edit(HANDLER, "    public async ValueTask", "    public async void Fire() => await Task.Yield();\n\n    public async ValueTask")),
    ("Swallowing Exception", "CA1031",
     lambda: write(CAT + "Catalog.Infrastructure/External/Swallow.cs", "namespace Catalog.Infrastructure.External;\n\ninternal static class Swallow\n{\n    public static async Task Run(Func<Task> work)\n    {\n        try\n        {\n            await work();\n        }\n        catch (Exception)\n        {\n        }\n    }\n}\n")),
    ("Logging without [LoggerMessage]", "CA1848",
     lambda: write("src/Api/Seed.cs", "namespace Api;\n\ninternal static class Seed\n{\n    public static void Log(Microsoft.Extensions.Logging.ILogger logger) => Microsoft.Extensions.Logging.LoggerExtensions.LogInformation(logger, \"hello\");\n}\n")),
    ("Namespace does not match folder", "IDE0130",
     lambda: write(CAT + "Catalog.Domain/Products/Misplaced.cs", "namespace Catalog.Domain.Elsewhere;\n\npublic sealed record Misplaced(int Value);\n")),
    ("Raw SQL", "EF1003",
     lambda: write(CAT + "Catalog.Infrastructure/Queries/GetProduct/Raw.cs", "using Catalog.Infrastructure.Persistence;\nusing Microsoft.EntityFrameworkCore;\n\nnamespace Catalog.Infrastructure.Queries.GetProduct;\n\ninternal static class Raw\n{\n    public static IQueryable<int> Count(CatalogDbContext db, string table) => db.Database.SqlQueryRaw<int>(\"SELECT COUNT(*) AS Value FROM \" + table);\n}\n")),
    ("Waiting on a task with a timeout", "VSTHRD103",
     lambda: edit(HANDLER, "var id =", "_ = Task.Delay(1).Wait(TimeSpan.FromSeconds(1));\n        var id =")),
    ("Blocking on a task in a synchronous method", "VSTHRD002",
     lambda: write(CAT + "Catalog.Application/CreateProduct/Sync.cs", "namespace Catalog.Application.CreateProduct;" + chr(10) + chr(10) + "public static class Sync" + chr(10) + "{" + chr(10) + "    public static int Get(Task<int> task) => task.Result;" + chr(10) + "}" + chr(10))),
    ("A message without an operation", "CS0535",
     lambda: edit(CAT + "Catalog.Application/CreateProduct/CreateProduct.cs", "public static Operation Operation { get; } =", "public static Operation Declared { get; } =")),
    ("An outcome the translation forgets", "CS8509",
     lambda: edit(CAT + "Catalog.Domain/Products/RenameOutcome.cs", "    Discontinued,", "    Discontinued,\n    Archived,")),
    ("Application reads an environment variable", "RS0030",
     lambda: write(CAT + "Catalog.Application/CreateProduct/Env.cs", "namespace Catalog.Application.CreateProduct;\n\npublic static class Env\n{\n    public static string? Get() => Environment.GetEnvironmentVariable(\"X\");\n}\n")),
    ("Application locates a service", "RS0030",
     lambda: write(CAT + "Catalog.Application/CreateProduct/Locate.cs", "namespace Catalog.Application.CreateProduct;\n\npublic static class Locate\n{\n    public static object? Get(IServiceProvider services) => services.GetService(typeof(string));\n}\n")),
    ("A scope begun with a template", "RS0030",
     lambda: write("src/Api/Scope.cs", "namespace Api;\n\ninternal static class Scope\n{\n    public static IDisposable? Begin(Microsoft.Extensions.Logging.ILogger logger) => Microsoft.Extensions.Logging.LoggerExtensions.BeginScope(logger, \"Order {Id}\", 1);\n}\n")),
]

# Test-time seeds: all applied at once; (test name, edits)
TEST = [
    ("Module_application_uses_no_data_access",
     lambda: write(CAT + "Catalog.Application/CreateProduct/Isolation.cs", "namespace Catalog.Application.CreateProduct;\n\npublic static class Isolation\n{\n    public static System.Data.IsolationLevel Level() => System.Data.IsolationLevel.Serializable;\n}\n")),
    ("Api_names_only_registration_entry_points_of_infrastructure",
     lambda: (write(CAT + "Catalog.Infrastructure/External/Client.cs", "namespace Catalog.Infrastructure.External;\n\npublic sealed class Client\n{\n    public int Value => 1;\n}\n"),
              write("src/Api/UsesClient.cs", "namespace Api;\n\ninternal static class UsesClient\n{\n    public static int Get() => new Catalog.Infrastructure.External.Client().Value;\n}\n"))),
    ("Each_module_infrastructure_has_one_registration_entry_point",
     lambda: edit(CAT + "Catalog.Infrastructure/CatalogModule.cs", "    public static IServiceCollection AddCatalogModule",
                  "    public static IServiceCollection AddCatalogExtras(this IServiceCollection services) => services;\n\n    public static IServiceCollection AddCatalogModule")),
    ("Internals_are_visible_only_to_tests",
     lambda: write(CAT + "Catalog.Domain/AssemblyInfo.cs", "[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(\"Api\")]\n")),
    ("Aggregate_state_is_private",
     lambda: edit(PRODUCT, "    public bool IsDiscontinued", "    public string Sku { get; set; } = string.Empty;\n\n    public bool IsDiscontinued")),
    ("Domain_does_no_io",
     lambda: edit(PRODUCT, "    public void Discontinue()", "    public static bool Exists(string path) => System.IO.File.Exists(path);\n\n    public void Discontinue()")),
    ("Domain_events_stay_in_their_module",
     lambda: write("src/Api/OnCreated.cs", "namespace Api;\n\ninternal static class OnCreated\n{\n    public static string Describe(Catalog.Domain.Products.ProductCreated e) => e.ToString();\n}\n")),
    ("Ids_are_strongly_typed",
     lambda: edit(PRODUCT, "    public bool IsDiscontinued", "    public Guid LegacyId => Id.Value;\n\n    public bool IsDiscontinued")),
    ("Domain_types_carry_no_mapping_attributes",
     lambda: edit(PRODUCT, "    public DateTimeOffset CreatedAt", "    [System.ComponentModel.DataAnnotations.Required]\n    public DateTimeOffset CreatedAt")),
    ("An_aggregate_holds_other_aggregates_by_id",
     lambda: (write(CAT + "Catalog.Domain/Categories/Category.cs", "using SharedKernel;\n\nnamespace Catalog.Domain.Categories;\n\npublic readonly record struct CategoryId(Guid Value);\n\npublic sealed class Category : AggregateRoot<CategoryId>\n{\n    private Category() { }\n}\n"),
              edit(PRODUCT, "    public bool IsDiscontinued", "    public Categories.Category? Category { get; private set; }\n\n    public bool IsDiscontinued"))),
    ("Repositories_expose_no_IQueryable",
     lambda: (edit(CAT + "Catalog.Domain/Products/IProductRepository.cs", "public interface IProductRepository : IRepository<Product, ProductId>;",
                   "public interface IProductRepository : IRepository<Product, ProductId>\n{\n    IQueryable<Product> Query();\n}"),
              edit(REPO, "    public void Save", "    public IQueryable<Product> Query() => db.Products;\n\n    public void Save"))),
    ("Messages_and_handlers_live_where_they_belong",
     lambda: write(CAT + "Catalog.Application/Ping/Ping.cs", "using Platform.Application.Abstractions;\n\nnamespace Catalog.Application.Ping;\n\npublic sealed record Ping : IQuery<int>\n{\n    public static Operation Operation { get; } = Operation.Query(\"catalog.ping\", CatalogPolicies.Read);\n}\n\npublic sealed class PingHandler : IQueryHandler<Ping, int>\n{\n    public ValueTask<Result<int>> Handle(Ping query, CancellationToken cancellationToken) => ValueTask.FromResult<Result<int>>(1);\n}\n")),
    ("A_query_never_writes",
     lambda: edit(QH, "        var id = query.ProductId.Value;", "        await db.SaveChangesAsync(cancellationToken);\n        var id = query.ProductId.Value;")),
    ("A_query_never_loads_an_aggregate",
     lambda: edit(QH, "        var id = query.ProductId.Value;", "        _ = await db.Products.CountAsync(cancellationToken);\n        var id = query.ProductId.Value;")),
    ("Messages_and_responses_carry_no_domain_types",
     lambda: edit(CAT + "Catalog.Application/GetProduct/GetProduct.cs", "string ETag)", "string ETag, Catalog.Domain.Products.ProductName? Display = null)")),
    ("Handlers_do_not_log",
     lambda: (edit(QH, "public sealed class GetProductHandler(CatalogDbContext db)", "public sealed class GetProductHandler(CatalogDbContext db, Microsoft.Extensions.Logging.ILogger<GetProductHandler> logger)"),
              edit(QH, "        var id = query.ProductId.Value;", "        _ = logger;\n        var id = query.ProductId.Value;"))),
    ("Application_and_domain_read_no_raw_claims",
     lambda: write(CAT + "Catalog.Application/CreateProduct/Who.cs", "namespace Catalog.Application.CreateProduct;\n\npublic static class Who\n{\n    public static string? Name(System.Security.Claims.ClaimsPrincipal user) => user.Identity?.Name;\n}\n")),
    ("Integration_events_carry_their_version_in_their_name",
     lambda: write(CAT + "Catalog.Contracts/IntegrationEvents/ProductRenamed.cs", "using Platform.Application.Abstractions;\n\nnamespace Catalog.Contracts.IntegrationEvents;\n\npublic sealed record ProductRenamed(ProductId ProductId) : IIntegrationEvent;\n")),
    ("Infrastructure_is_organized_by_technical_concern",
     lambda: write(CAT + "Catalog.Infrastructure/Helpers/Slug.cs", "namespace Catalog.Infrastructure.Helpers;\n\ninternal static class Slug\n{\n    public static string Of(string s) => s.ToUpperInvariant();\n}\n")),
    ("Application_is_sliced_by_feature_and_domain_by_aggregate",
     lambda: write(CAT + "Catalog.Application/Handlers/Noop.cs", "namespace Catalog.Application.Handlers;" + chr(10) + chr(10) + "public static class Noop" + chr(10) + "{" + chr(10) + "    public static int Zero() => 0;" + chr(10) + "}" + chr(10))),
    ("Only_the_composition_root_reads_configuration_or_the_environment",
     lambda: write(CAT + "Catalog.Infrastructure/External/Settings.cs", "namespace Catalog.Infrastructure.External;\n\ninternal sealed class Settings(Microsoft.Extensions.Configuration.IConfiguration configuration)\n{\n    public string? Value => configuration[\"x\"];\n}\n")),
    ("Only_the_composition_root_resolves_services",
     lambda: write(CAT + "Catalog.Infrastructure/External/Locator.cs", "namespace Catalog.Infrastructure.External;\n\ninternal sealed class Locator(IServiceProvider services)\n{\n    public object? Get(Type t) => services.GetService(t);\n}\n")),
    ("No_dynamic",
     lambda: write(CAT + "Catalog.Infrastructure/External/Dyn.cs", "namespace Catalog.Infrastructure.External;\n\ninternal static class Dyn\n{\n    public static object Add(dynamic d) => d + 1;\n}\n")),
    ("Every_message_declares_a_valid_operation",
     lambda: edit(CAT + "Catalog.Application/RenameProduct/RenameProduct.cs", "\"catalog.rename-product\"", "\"catalog.create-product\"")),
    ("Commands_return_at_most_an_id_an_outcome_and_a_version",
     lambda: write(CAT + "Catalog.Application/ArchiveProduct/ArchiveProduct.cs", "using Platform.Application.Abstractions;\n\nnamespace Catalog.Application.ArchiveProduct;\n\npublic sealed record ArchiveProduct : ICommand<string>\n{\n    public static Operation Operation { get; } = Operation.Command(\"catalog.archive-product\", CatalogPolicies.Write, IdempotencyMechanism.Natural);\n}\n\npublic sealed class ArchiveProductHandler : ICommandHandler<ArchiveProduct, string>\n{\n    public ValueTask<Result<string>> Handle(ArchiveProduct command, CancellationToken cancellationToken) => ValueTask.FromResult<Result<string>>(\"archived\");\n}\n")),
    ("Only_the_registrar_maps_a_route_to_a_handler",
     lambda: write("src/Api/Extra.cs", "namespace Api;\n\ninternal static class Extra\n{\n    public static void Map(IEndpointRouteBuilder app) => app.MapGet(\"/extra\", () => \"extra\");\n}\n")),
    ("Endpoints_reference_only_their_messages_and_dtos",
     lambda: edit("src/Api/Endpoints/Catalog/GetProductEndpoint.cs", "var result = await sender.Send(", "_ = global::Catalog.Application.CatalogProblems.ProductNotFound;\n        var result = await sender.Send(")),
    ("Infrastructure_is_internal_unless_the_dispatcher_names_it",
     lambda: write(CAT + "Catalog.Infrastructure/Persistence/Helper.cs", "namespace Catalog.Infrastructure.Persistence;\n\npublic sealed class Helper\n{\n    public int Value => 1;\n}\n")),
    ("Migrations_reference_no_application_code",
     lambda: edit_glob(CAT + "Catalog.Infrastructure/Persistence/Migrations/*_Initial.cs", "protected override void Up(MigrationBuilder migrationBuilder)\n        {\n", "protected override void Up(MigrationBuilder migrationBuilder)\n        {\n            _ = new Catalog.Domain.Products.ProductId(Guid.Empty);\n")),
    ("Only_the_pipeline_touches_the_unit_of_work",
     lambda: write(CAT + "Catalog.Application/CreateProduct/Peek.cs", "using Platform.Application.Abstractions;\n\nnamespace Catalog.Application.CreateProduct;\n\npublic static class Peek\n{\n    public static bool Dirty(IUnitOfWorkFactory units) => units.For(typeof(Peek)).HasChanges;\n}\n")),
    ("Integration_events_leave_only_through_the_outbox",
     lambda: write(CAT + "Catalog.Application/CreateProduct/IEventSender.cs", "using Catalog.Contracts.IntegrationEvents;\n\nnamespace Catalog.Application.CreateProduct;\n\npublic interface IEventSender\n{\n    void Send(ProductCreatedV1 integrationEvent);\n}\n")),
    ("Idempotency_records_live_in_the_database_with_the_change",
     lambda: edit(UOW, "internal sealed class UnitOfWork(ModuleDbContext db, DispatchContext dispatch, TimeProvider time) : IUnitOfWork\n{\n", "internal sealed class UnitOfWork(ModuleDbContext db, DispatchContext dispatch, TimeProvider time) : IUnitOfWork\n{\n    private readonly IDbContextFactory<ModuleDbContext>? _other = null;\n\n    public bool HasOther => _other is not null;\n\n")),
    ("A_query_returns_a_collection_only_as_a_page",
     lambda: (write(CAT + "Catalog.Application/ListNames/ListNames.cs", "using Platform.Application.Abstractions;\n\nnamespace Catalog.Application.ListNames;\n\npublic sealed record ListNames : IQuery<IReadOnlyList<string>>\n{\n    public static Operation Operation { get; } = Operation.Query(\"catalog.list-names\", CatalogPolicies.Read);\n}\n"),
              write(CAT + "Catalog.Infrastructure/Queries/ListNames/ListNamesHandler.cs", "using Platform.Application.Abstractions;\n\nnamespace Catalog.Infrastructure.Queries.ListNames;\n\npublic sealed class ListNamesHandler : IQueryHandler<Application.ListNames.ListNames, IReadOnlyList<string>>\n{\n    public ValueTask<Result<IReadOnlyList<string>>> Handle(Application.ListNames.ListNames query, CancellationToken cancellationToken) =>\n        ValueTask.FromResult(Result<IReadOnlyList<string>>.Success([]));\n}\n"))),
    ("Page_is_the_only_page_shape",
     lambda: write(CAT + "Catalog.Application/ListProducts/ProductPage.cs", "namespace Catalog.Application.ListProducts;\n\npublic sealed record ProductPage(IReadOnlyList<ProductSummary> Items, string? NextCursor);\n")),
    ("Every_paged_query_declares_a_maximum_within_the_ceiling",
     lambda: edit(CAT + "Catalog.Application/ListProducts/ListProducts.cs", "public static int MaxPageSize => 50;", "public static int MaxPageSize => 500;")),
    ("A_paged_query_orders_only_through_its_keyset",
     lambda: edit(LIST, "var rows = db.Set<ProductSummaryRow>().AsNoTracking();", "var rows = db.Set<ProductSummaryRow>().AsNoTracking().OrderBy(r => r.Name).AsQueryable();")),
    ("Only_the_codec_seals_and_opens_cursors",
     lambda: write(CAT + "Catalog.Infrastructure/Queries/ListProducts/Peek.cs", "using Platform.Application.Abstractions;\n\nnamespace Catalog.Infrastructure.Queries.ListProducts;\n\ninternal static class Peek\n{\n    public static PageCursor Make() => new(\"x\", Guid.Empty);\n}\n")),
    ("No_static_mutable_state",
     lambda: write(CAT + "Catalog.Infrastructure/Persistence/Counter.cs", "namespace Catalog.Infrastructure.Persistence;\n\ninternal static class Counter\n{\n    public static int Hits;\n\n    public static void Hit() => Hits++;\n}\n")),
]

PROGRAM = "src/Api/Program.cs"
UOW = "src/Platform/Platform.Infrastructure/Persistence/UnitOfWork.cs"
BEHAVIOR = "src/Platform/Platform.Application/Behaviors/UnitOfWorkBehavior.cs"
PRODUCT_CONFIG = CAT + "Catalog.Infrastructure/Persistence/ProductConfiguration.cs"
HTTP = "src/Api/Http/HttpSetup.cs"

# API seeds, each run alone so that one seed cannot make another seed's test fail: (expected failing tests, edits)
API = [
    (["Every_route_is_a_discovered_endpoint_or_a_declared_framework_endpoint"],
     lambda: edit(PROGRAM, "app.MapDiscoveredEndpoints();", "app.MapGet(\"/ping\", () => \"pong\").AllowAnonymous();\napp.MapDiscoveredEndpoints();")),
    (["Every_endpoint_carries_its_operations_policy_and_rate_limit"],
     lambda: edit("src/Api/Endpoints/EndpointRegistrar.cs", "            .RequireRateLimiting(operation.RateLimit)\n", "")),
    (["The_fallback_policy_denies_everyone"],
     lambda: edit(HTTP, "RequireAssertion(_ => false)", "RequireAuthenticatedUser()")),
    (["Every_operation_policy_is_registered"],
     lambda: edit(CAT + "Catalog.Infrastructure/CatalogModule.cs", "options.AddPolicy(CatalogPolicies.Write,", "options.AddPolicy(\"catalog.misnamed\",")),
    (["An_invalid_message_is_refused_before_its_handler_with_field_errors_by_request_path"],
     lambda: edit(PROGRAM, "        typeof(ValidationBehavior<,>),\n", "")),
    (["Every_options_class_the_solution_binds_has_a_validator", "A_missing_or_unusable_limit_stops_startup"],
     lambda: edit(HTTP, "            .Validate(o => o.PermitLimit > 0 && o.Window > TimeSpan.Zero, \"Rate limits need a positive permit limit and window; a missing limit denies.\")\n", "")),
]


MODEL = "tests/Architecture.Tests"
SQL = "tests/Catalog.IntegrationTests"
UNIT = "tests/Platform.Tests"

ISOLATED = [
    (MODEL, ["Each_module_has_one_context_on_its_own_schema"],
     lambda: edit(PRODUCT_CONFIG, 'builder.ToTable("Products");', 'builder.ToTable("Products", "dbo");')),
    (MODEL, ["Mapped_domain_types_have_no_public_setter_or_parameterless_constructor"],
     lambda: edit(PRODUCT, "public sealed class Product : AggregateRoot<ProductId>\n{\n", "public sealed class Product : AggregateRoot<ProductId>\n{\n    public Product()\n        : this(default, new ProductName(\"x\"), default)\n    {\n    }\n\n")),
    (MODEL, ["No_navigation_leads_from_one_aggregate_to_another"],
     lambda: (write(CAT + "Catalog.Domain/Categories/Category.cs", "using SharedKernel;\n\nnamespace Catalog.Domain.Categories;\n\npublic readonly record struct CategoryId(Guid Value);\n\npublic sealed class Category : AggregateRoot<CategoryId>\n{\n    private Category()\n    {\n    }\n}\n"),
              write(CAT + "Catalog.Infrastructure/Persistence/CategoryConfiguration.cs", "using Catalog.Domain.Categories;\nusing Microsoft.EntityFrameworkCore;\nusing Microsoft.EntityFrameworkCore.Metadata.Builders;\n\nnamespace Catalog.Infrastructure.Persistence;\n\ninternal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>\n{\n    public void Configure(EntityTypeBuilder<Category> builder)\n    {\n        builder.HasKey(c => c.Id);\n        builder.Property(c => c.Id).HasConversion(id => id.Value, value => new CategoryId(value));\n        builder.Ignore(c => c.DomainEvents);\n    }\n}\n"),
              edit(PRODUCT, "    public int StockOnHand", "    public Categories.Category? Category { get; private set; }\n\n    public int StockOnHand"))),
    (MODEL, ["Domain_events_carry_ids_and_values_never_entities"],
     lambda: edit(CAT + "Catalog.Domain/Products/ProductCreated.cs", "(ProductId ProductId, DateTimeOffset At)", "(ProductId ProductId, DateTimeOffset At, Product? Snapshot = null)")),
    (MODEL, ["Read_types_are_keyless"],
     lambda: write(CAT + "Catalog.Infrastructure/Queries/GetProduct/ProductListRow.cs", "using Microsoft.EntityFrameworkCore;\nusing Microsoft.EntityFrameworkCore.Metadata.Builders;\n\nnamespace Catalog.Infrastructure.Queries.GetProduct;\n\ninternal sealed class ProductListRow\n{\n    public Guid Id { get; set; }\n\n    public string Name { get; set; } = string.Empty;\n}\n\ninternal sealed class ProductListRowConfiguration : IEntityTypeConfiguration<ProductListRow>\n{\n    public void Configure(EntityTypeBuilder<ProductListRow> builder)\n    {\n        builder.ToView(\"ProductList\");\n        builder.HasKey(r => r.Id);\n    }\n}\n")),
    (MODEL, ["Every_module_model_carries_the_pipelines_tables"],
     lambda: edit("src/Platform/Platform.Infrastructure/Persistence/ModuleDbContext.cs", "r.OperationId, r.Key }).IsUnique();", "r.OperationId, r.Key });")),
    (MODEL, ["Every_model_change_has_its_migration"],
     lambda: edit(CAT + "Catalog.Domain/Products/ProductName.cs", "public const int MaxLength = 200;", "public const int MaxLength = 150;")),

    (SQL, ["A_keyed_command_without_a_key_is_refused"],
     lambda: edit(CAT + "Catalog.Application/ReceiveStock/ReceiveStock.cs", "IdempotencyMechanism.Key", "IdempotencyMechanism.Natural")),
    (SQL, ["A_replay_returns_the_recorded_result_without_running_the_handler_again", "The_same_key_with_a_different_request_is_refused", "Concurrent_duplicates_resolve_as_one_execution_and_replays"],
     lambda: edit(UOW, "r => r.PrincipalId == attempt.PrincipalId && r.OperationId", "r => r.Id < 0 && r.PrincipalId == attempt.PrincipalId && r.OperationId")),
    (SQL, ["A_key_belongs_to_its_principal"],
     lambda: edit(BEHAVIOR, "attempt = new AttemptIdentity(context.PrincipalId, operation.Id, key);", "attempt = new AttemptIdentity(\"anyone\", operation.Id, key);")),
    (SQL, ["A_success_commits_the_change_its_outbox_rows_and_its_audit_record_together"],
     lambda: edit(UOW, "        StageDomainEvents();\n        try", "        try")),
    (UNIT, ["A_failure_discards_what_the_handler_staged_before_it_is_recorded"],
     lambda: edit(BEHAVIOR, "so a replay returns it.\n        unit.Discard();\n", "so a replay returns it.\n")),
    (SQL, ["A_stale_write_is_a_conflict_not_an_exception"],
     lambda: edit(UOW, "        catch (DbUpdateConcurrencyException)\n        {\n            return CommitOutcome.Conflict;\n        }\n", "")),
    (SQL, ["A_principal_without_the_policy_is_refused_at_the_transport_and_recorded"],
     lambda: edit("src/Api/Http/HttpSetup.cs", "        services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationMiddlewareResultHandler, AuditingAuthorizationResultHandler>();\n", "")),
    (SQL, ["The_authorization_behavior_refuses_and_records_without_any_transport_filter"],
     lambda: edit(PROGRAM, "        typeof(AuthorizationBehavior<,>),\n", "")),
    (SQL, ["The_authorization_behavior_refuses_and_records_without_any_transport_filter"],
     lambda: edit("src/Platform/Platform.Application/Behaviors/AuthorizationBehavior.cs", "        unit.StageAudit(Audit.Entry(message, TMessage.Operation, context, time, AuditOutcome.Denied, problem));\n", "        _ = time;\n")),
    (SQL, ["A_query_that_reads_with_sql_projects_what_was_committed"],
     lambda: edit(QH, "SELECT Id, Name, IsDiscontinued, StockOnHand, Version FROM", "SELECT Id, Name, IsDiscontinued, 0 AS StockOnHand, Version FROM")),
    (SQL, ["A_query_for_a_missing_record_answers_its_declared_problem"],
     lambda: edit(QH, "return CatalogProblems.ProductNotFound.With();", "return PlatformProblems.NotFound.With();")),

    (SQL, ["Paging_skips_and_repeats_no_row_across_page_boundaries"],
     lambda: edit("src/Platform/Platform.Application/Abstractions/Keyset.cs", "Expression.GreaterThan(Compare(id, idValue), Expression.Constant(0))", "Expression.GreaterThanOrEqual(Compare(id, idValue), Expression.Constant(0))")),
    (SQL, ["A_page_size_above_the_maximum_is_refused_never_clamped", "A_cursor_the_query_did_not_issue_is_refused"],
     lambda: edit(PROGRAM, "        typeof(PagingBehavior<,>),\n", "")),
    (UNIT, ["A_command_that_does_not_persist_fails_the_dispatch_if_it_stages_a_change"],
     lambda: edit(BEHAVIOR, "        if (unit.HasChanges)\n", "        if (unit.HasChanges && message is null)\n")),
    (UNIT, ["A_command_that_does_not_persist_is_still_recorded_in_the_audit_trail"],
     lambda: edit(BEHAVIOR, "        unit.StageAudit(Audit.Entry(message, TMessage.Operation, context, time, result.IsSuccess ? AuditOutcome.Succeeded : AuditOutcome.Failed, result.Problem));\n", "")),
]


def failed(out, name):
    return any(line.startswith("failed ") and f".{name}" in line for line in out.splitlines())


ONLY = [n for n in os.environ.get("SEEDS", "").split(",") if n]


def wanted(*names):
    return not ONLY or any(n in ONLY for n in names)


failures = []

for name, expected, seed in BUILD:
    if not wanted(name):
        continue
    fresh()
    seed()
    code, out = run("build", "Starter.slnx", "-nologo")
    ok = code != 0 and expected in out
    print(f"{'FAILS AS EXPECTED' if ok else 'NOT CAUGHT':18} build  {name}")
    if not ok:
        failures.append(name)
        print("\n".join(l for l in out.splitlines() if "error" in l)[:2000])

TEST = [(n, s) for n, s in TEST if wanted(n)]
fresh()
for _, seed in TEST:
    seed()
code, out = run("build", "Starter.slnx", "-nologo")
if code != 0:
    print("Test seeds do not compile:\n" + "\n".join(l for l in out.splitlines() if "error" in l)[:4000])
    sys.exit(1)
code, out = run("test", "--project", "tests/Architecture.Tests", "--no-build")
for name, _ in TEST:
    caught = failed(out, name)
    print(f"{'FAILS AS EXPECTED' if caught else 'NOT CAUGHT':18} test   {name}")
    if not caught:
        failures.append(name)

for names, seed in API:
    if not wanted(*names):
        continue
    fresh()
    seed()
    code, out = run("build", "Starter.slnx", "-nologo")
    if code != 0:
        print(f"{'SEED BROKEN':18} api    {names[0]}\n" + "\n".join(l for l in out.splitlines() if " error " in l)[:2000])
        failures.append(names[0])
        continue
    code, out = run("test", "--project", "tests/Api.Tests", "--no-build")
    for name in names:
        caught = failed(out, name)
        print(f"{'FAILS AS EXPECTED' if caught else 'NOT CAUGHT':18} api    {name}")
        if not caught:
            failures.append(name)

for project, names, seed in ISOLATED:
    if not wanted(*names):
        continue
    fresh()
    seed()
    code, out = run("build", "Starter.slnx", "-nologo")
    if code != 0:
        print(f"{'SEED BROKEN':18} {project.split('/')[-1][:6].lower():6} {names[0]}\n" + "\n".join(l for l in out.splitlines() if " error " in l)[:2000])
        failures.append(names[0])
        continue
    code, out = run("test", "--project", project, "--no-build")
    for name in names:
        caught = failed(out, name)
        print(f"{'FAILS AS EXPECTED' if caught else 'NOT CAUGHT':18} {project.split('/')[-1][:6].lower():6} {name}")
        if not caught:
            failures.append(name)

total = (sum(1 for n, _, _ in BUILD if wanted(n)) + len(TEST) + sum(len(n) for n, _ in API if wanted(*n))
         + sum(len(n) for _, n, _ in ISOLATED if wanted(*n)))
shutil.rmtree(WORK, ignore_errors=True)
print(f"\n{total - len(failures)} of {total} seeds caught")
sys.exit(1 if failures else 0)
