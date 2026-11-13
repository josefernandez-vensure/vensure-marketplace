"""Prove the rancho-dotnet template's checks bite.

Seeds one violation per check into a copy of the template and confirms the build
or the named tests fail on it. Run it after changing a check.

Usage, from the repository root:  python -I templates/check-rancho-dotnet.py

Each seed gets its own copy, and runs only the tests it expects to fail. The SQL
Server seeds share one server, started here in Docker, each run on a database of
its own (the fixture reads RANCHO_TEST_SQL). SEEDS=<name>,<name> reruns only
those seeds; WORKERS=<n> runs that many seeds at once (default 6).
"""
import concurrent.futures
import functools
import os
import secrets
import shutil
import subprocess
import sys
import tempfile
import threading
import time
from pathlib import Path

print = functools.partial(print, flush=True)

SRC = Path(__file__).resolve().parent / "rancho-dotnet"
CAT = "src/Modules/Catalog/"
WORKERS = int(os.environ.get("WORKERS", "6"))
_local = threading.local()


def work():
    """This thread's copy of the template: each seed edits and builds its own."""
    return _local.work


def fresh():
    _local.work = Path(tempfile.mkdtemp(prefix="rancho-dotnet-seeded-"))
    shutil.copytree(SRC, _local.work, dirs_exist_ok=True, ignore=shutil.ignore_patterns("bin", "obj", "TestResults", ".vs"))


def done():
    shutil.rmtree(_local.work, ignore_errors=True)


def edit(path, old, new):
    p = work() / path
    s = p.read_text(encoding="utf-8")
    assert old in s, f"{old!r} not in {path}"
    p.write_text(s.replace(old, new, 1), encoding="utf-8")


def edit_glob(pattern, old, new):
    matches = list(work().glob(pattern))
    assert len(matches) == 1, f"{pattern} matches {matches}"
    edit(str(matches[0].relative_to(work())), old, new)


def write(path, text):
    p = work() / path
    p.parent.mkdir(parents=True, exist_ok=True)
    p.write_text(text, encoding="utf-8")


def run(*args):
    r = subprocess.run(["dotnet", *args], cwd=work(), capture_output=True, text=True, encoding="utf-8", errors="replace",
                       env={**os.environ, "DOTNET_CLI_TELEMETRY_OPTOUT": "1"})
    return r.returncode, r.stdout + r.stderr


def build(project="Starter.slnx"):
    """The whole solution, or only the test project a seed runs and what it references."""
    return run("build", project, "-nologo")


def test(project, names):
    """Only the tests the seed expects to fail. A name that matches no test runs nothing, so it reads as not caught."""
    filters = [arg for name in names for arg in ("--filter-method", f"*.{name}")]
    return run("test", "--project", project, "--no-build", *filters)


DOMAIN = CAT + "Catalog.Domain/Catalog.Domain.csproj"
UOW = "src/Platform/Platform.Infrastructure/Persistence/UnitOfWork.cs"
LIST = CAT + "Catalog.Infrastructure/Queries/ListProducts/ListProductsHandler.cs"
PRODUCT = CAT + "Catalog.Domain/Products/Product.cs"
HANDLER = CAT + "Catalog.Application/CreateProduct/CreateProductHandler.cs"
QH = CAT + "Catalog.Infrastructure/Queries/GetProduct/GetProductHandler.cs"
REPO = CAT + "Catalog.Infrastructure/Persistence/ProductRepository.cs"
ORD = "src/Modules/Ordering/"
DISPATCHER = "src/Platform/Platform.Infrastructure/Messaging/OutboxDispatcher.cs"
MESSAGING = "src/Platform/Platform.Infrastructure/PlatformMessaging.cs"

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
    ("A reaction that maps to a query", "CS0311",
     lambda: write(ORD + "Ordering.Application/GetOrder/OnStockReserved.cs", "using Catalog.Contracts.IntegrationEvents;\nusing Ordering.Contracts;\nusing Platform.Application.Abstractions;\n\nnamespace Ordering.Application.GetOrder;\n\npublic sealed class OnStockReserved : IReaction<StockReservedV1, GetOrder>\n{\n    public static GetOrder Map(StockReservedV1 message) => new(new OrderId(message.ReservationId.Value));\n}\n")),
    ("A module builds a service principal", "CS0122",
     lambda: write(ORD + "Ordering.Infrastructure/External/Impersonate.cs", "namespace Ordering.Infrastructure.External;\n\ninternal static class Impersonate\n{\n    public static System.Security.Claims.ClaimsIdentity Catalog() => new Platform.Infrastructure.Messaging.ServiceIdentity(\"Catalog\");\n}\n")),
    ("A module enters a dispatch scope", "CS0122",
     lambda: write(ORD + "Ordering.Infrastructure/External/Enter.cs", "namespace Ordering.Infrastructure.External;\n\ninternal static class Enter\n{\n    public static object Scope() => new Platform.Infrastructure.Messaging.DispatchScope();\n}\n")),
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
     lambda: edit(CAT + "Catalog.Application/GetProduct/GetProduct.cs", "string Etag)", "string Etag, Catalog.Domain.Products.ProductName? Display = null)")),
    ("Handlers_do_not_log",
     lambda: (edit(QH, "public sealed class GetProductHandler(CatalogDbContext db, DispatchContext dispatch)", "public sealed class GetProductHandler(CatalogDbContext db, DispatchContext dispatch, Microsoft.Extensions.Logging.ILogger<GetProductHandler> logger)"),
              edit(QH, "        var id = query.ProductId.Value;", "        _ = logger;\n        var id = query.ProductId.Value;"))),
    ("Application_and_domain_read_no_raw_claims",
     lambda: write(CAT + "Catalog.Application/CreateProduct/Who.cs", "namespace Catalog.Application.CreateProduct;\n\npublic static class Who\n{\n    public static string? Name(System.Security.Claims.ClaimsPrincipal user) => user.Identity?.Name;\n}\n")),
    ("Published_messages_carry_their_version_in_their_name",
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
    ("Published_messages_leave_only_through_the_outbox",
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
     lambda: edit(LIST, ".AsNoTracking().Where(r => r.TenantId == tenant);", ".AsNoTracking().Where(r => r.TenantId == tenant).OrderBy(r => r.Name).AsQueryable();")),
    ("Only_the_codec_seals_and_opens_cursors",
     lambda: write(CAT + "Catalog.Infrastructure/Queries/ListProducts/Peek.cs", "using Platform.Application.Abstractions;\n\nnamespace Catalog.Infrastructure.Queries.ListProducts;\n\ninternal static class Peek\n{\n    public static PageCursor Make() => new(\"x\", Guid.Empty);\n}\n")),
    ("No_static_mutable_state",
     lambda: write(CAT + "Catalog.Infrastructure/Persistence/Counter.cs", "namespace Catalog.Infrastructure.Persistence;\n\ninternal static class Counter\n{\n    public static int Hits;\n\n    public static void Hit() => Hits++;\n}\n")),
    ("Class_attributes_are_defined_only_in_the_shared_kernel",
     lambda: write(CAT + "Catalog.Domain/Products/SensitiveAttribute.cs", "using Microsoft.Extensions.Compliance.Classification;\n\nnamespace Catalog.Domain.Products;\n\npublic sealed class SensitiveAttribute() : DataClassificationAttribute(new DataClassification(\"Rancho\", \"Restricted\"));\n")),
    ("A_contracts_twin_declares_the_class_of_its_domain_type",
     lambda: edit(CAT + "Catalog.Contracts/ProductName.cs", "[Public]", "[Internal]")),
    ("Everything_that_leaves_through_a_response_an_event_or_the_outbox_resolves_to_a_class",
     lambda: edit(CAT + "Catalog.Application/ListProducts/ListProducts.cs", "[ClassOf<ProductName>] string Name)", "[ClassOf<ProductName>] string Name, int Rank = 0)")),
    ("A_member_declares_a_class_only_where_its_type_carries_none",
     lambda: edit(CAT + "Catalog.Contracts/IntegrationEvents/ProductCreatedV1.cs", "(ProductId ProductId,", "([Internal] ProductId ProductId,")),
    ("Every_logging_parameter_declares_the_class_of_its_value",
     lambda: edit("src/Platform/Platform.Application/Behaviors/DispatchTelemetry.cs", "[Internal] string operation, [Internal] double elapsedMs)", "[Internal] string operation, double elapsedMs)")),
    ("Logging_methods_are_generated_by_the_redacting_generator",
     lambda: edit("src/Platform/Platform.Application/Platform.Application.csproj", '<PackageReference Include="Microsoft.Extensions.Telemetry.Abstractions" Version="10.10.0" />', "")),
    ("A_confidential_or_restricted_type_converts_implicitly_to_no_string",
     lambda: write(CAT + "Catalog.Domain/Products/SupplierEmail.cs", "using SharedKernel;\n\nnamespace Catalog.Domain.Products;\n\n[Confidential]\npublic sealed record SupplierEmail(string Value)\n{\n    public static implicit operator string(SupplierEmail email) => email.Value;\n\n    public override string ToString() => \"***\";\n}\n")),
    ("A_confidential_or_restricted_type_redacts_in_ToString",
     lambda: write(CAT + "Catalog.Domain/Products/TaxNumber.cs", "using SharedKernel;\n\nnamespace Catalog.Domain.Products;\n\n[Restricted]\npublic sealed record TaxNumber(string Value);\n")),
    ("Events_and_outbox_payloads_carry_nothing_restricted",
     lambda: write(CAT + "Catalog.Contracts/IntegrationEvents/ProductTaxedV1.cs", "using Platform.Application.Abstractions;\nusing SharedKernel;\n\nnamespace Catalog.Contracts.IntegrationEvents;\n\npublic sealed record ProductTaxedV1(ProductId ProductId, [Restricted] string TaxNumber) : IIntegrationEvent;\n")),
    ("No_committed_configuration_file_holds_a_secret",
     lambda: edit("src/Api/appsettings.Development.json", '  "Logging": {', '  "Smtp": {\n    "Password": "hunter2"\n  },\n  "Logging": {')),
    ("Domain_outcomes_are_enums",
     lambda: write(CAT + "Catalog.Domain/Products/ArchiveOutcome.cs", "namespace Catalog.Domain.Products;\n\npublic sealed record ArchiveOutcome(bool Archived);\n")),
    ("Reactions_live_in_their_modules_application",
     lambda: write("src/Api/Reactions/OnStockReserved.cs", "using Catalog.Contracts.IntegrationEvents;\nusing Ordering.Application.ConfirmOrder;\nusing Ordering.Contracts;\nusing Platform.Application.Abstractions;\n\nnamespace Api.Reactions;\n\npublic sealed class OnStockReserved : IReaction<StockReservedV1, ConfirmOrder>\n{\n    public static ConfirmOrder Map(StockReservedV1 message) => new(new OrderId(message.ReservationId.Value));\n}\n")),
    ("A_reaction_maps_to_a_command_of_its_own_module",
     lambda: (write(CAT + "Catalog.Contracts/PurgeProducts.cs", "using Platform.Application.Abstractions;\n\nnamespace Catalog.Contracts;\n\npublic sealed record PurgeProducts(ProductId ProductId) : ICommand\n{\n    public static Operation Operation { get; } = Operation.Command(\"catalog.purge-products\", \"catalog.react\", IdempotencyMechanism.Key);\n}\n"),
              write(CAT + "Catalog.Application/PurgeProducts/PurgeProductsHandler.cs", "using Platform.Application.Abstractions;\n\nnamespace Catalog.Application.PurgeProducts;\n\npublic sealed class PurgeProductsHandler : ICommandHandler<Contracts.PurgeProducts>\n{\n    public ValueTask<Result> Handle(Contracts.PurgeProducts command, CancellationToken cancellationToken) => ValueTask.FromResult(Result.Success());\n}\n"),
              write(ORD + "Ordering.Application/ConfirmOrder/OnReservedPurge.cs", "using Catalog.Contracts;\nusing Catalog.Contracts.IntegrationEvents;\nusing Platform.Application.Abstractions;\n\nnamespace Ordering.Application.ConfirmOrder;\n\npublic sealed class OnReservedPurge : IReaction<StockReservedV1, PurgeProducts>\n{\n    public static PurgeProducts Map(StockReservedV1 message) => new(message.ProductId);\n}\n"))),
    ("A_reaction_answers_only_a_message_its_module_may_hear",
     lambda: write(ORD + "Ordering.Application/ConfirmOrder/OnReserveStock.cs", "using Catalog.Contracts.CommandMessages;\nusing Ordering.Contracts;\nusing Platform.Application.Abstractions;\n\nnamespace Ordering.Application.ConfirmOrder;\n\npublic sealed class OnReserveStock : IReaction<ReserveStockV1, ConfirmOrder>\n{\n    public static ConfirmOrder Map(ReserveStockV1 message) => new(new OrderId(message.ReservationId.Value));\n}\n")),
    ("A_reaction_command_is_keyed_transactional_and_unversioned",
     lambda: edit(CAT + "Catalog.Application/ReleaseStock/ReleaseStock.cs", "IdempotencyMechanism.Key", "IdempotencyMechanism.Natural")),
]

PROGRAM = "src/Api/Program.cs"
UOW = "src/Platform/Platform.Infrastructure/Persistence/UnitOfWork.cs"
BEHAVIOR = "src/Platform/Platform.Application/Behaviors/UnitOfWorkBehavior.cs"
PRODUCT_CONFIG = CAT + "Catalog.Infrastructure/Persistence/ProductConfiguration.cs"
HTTP = "src/Api/Http/HttpSetup.cs"
OPENAPI = "src/Api/OpenApi/ContractDocument.cs"
TRACING = "src/Api/Telemetry/TraceBoundary.cs"

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
    (["The_committed_openapi_document_is_the_one_the_host_generates"],
     lambda: edit("src/Api/Endpoints/Catalog/RenameProductEndpoint.cs", "RenameProductResponse([Internal] string Etag)", "RenameProductResponse([Internal] string Etag, [Internal] int Attempts = 0)")),
    (["Every_property_and_parameter_schema_carries_its_data_class"],
     lambda: edit("src/Api/Endpoints/Catalog/ReceiveStockEndpoint.cs", "ReceiveStockResponse([Internal] string Etag)", "ReceiveStockResponse([Internal] string Etag, long Version = 0)")),
    (["Every_property_and_parameter_schema_carries_its_data_class"],
     lambda: edit(OPENAPI, "ContractDocument.SetDataClass(schema, Require(OfParameter(parameter), parameter.Name));", "_ = OfParameter(parameter);")),
    (["Every_array_a_success_returns_is_bounded_and_a_page_by_its_maximum_page_size"],
     lambda: edit(OPENAPI, "            options.AddSchemaTransformer<PageBoundTransformer>();\n", "")),
    (["A_classified_logging_parameter_is_redacted_in_the_providers_output"],
     lambda: edit(PROGRAM, "builder.Logging.EnableRedaction();\n", "")),
    (["A_classified_logging_parameter_is_redacted_in_the_providers_output"],
     lambda: edit(PROGRAM, ".SetFallbackRedactor<ErasingRedactor>()", ".SetFallbackRedactor<NullRedactor>()")),
    (["Every_rate_limit_refuses_a_request_beyond_it_with_problem_details_and_retry_after"],
     lambda: edit(HTTP, "context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);", "_ = retryAfter;")),
    (["Every_rate_limit_refuses_a_request_beyond_it_with_problem_details_and_retry_after"],
     lambda: edit(PROGRAM, "app.UseRateLimiter();\napp.UseAuthorization();", "app.UseAuthorization();\napp.UseRateLimiter();")),
    (["A_callers_traceparent_starts_a_new_trace_linked_to_the_callers"],
     lambda: edit(TRACING, "        builder.Services.AddSingleton<DistributedContextPropagator>(new NewTracePropagator());\n", "")),
    (["A_callers_traceparent_starts_a_new_trace_linked_to_the_callers"],
     lambda: edit(TRACING, "        Sdk.SetDefaultTextMapPropagator(new TraceContextPropagator());\n", "")),
    (["A_callers_traceparent_starts_a_new_trace_linked_to_the_callers"],
     lambda: edit(PROGRAM, "app.UseTraceBoundary();\n", "")),
    (["Every_module_context_connects_to_the_one_database"],
     lambda: edit(CAT + "Catalog.Infrastructure/CatalogModule.cs", "        services.AddModuleDbContext<CatalogDbContext>(CatalogDbContext.SchemaName);\n", "        services.AddModuleDbContext<CatalogDbContext>(CatalogDbContext.SchemaName);\n        services.AddDbContext<CatalogDbContext>(options => Microsoft.EntityFrameworkCore.SqlServerDbContextOptionsExtensions.UseSqlServer(options, \"Server=localhost;Database=Catalog\"));\n")),
    (["A_reaction_commands_policy_admits_its_modules_service_principal_alone"],
     lambda: edit(CAT + "Catalog.Infrastructure/CatalogModule.cs", "policy => policy.RequireServicePrincipal(CatalogDbContext.ModuleName)", "policy => policy.RequireAuthenticatedUser().RequireClaim(\"permissions\", CatalogPolicies.React)")),
    (["A_reaction_commands_policy_admits_its_modules_service_principal_alone"],
     lambda: edit(ORD + "Ordering.Infrastructure/OrderingModule.cs", "policy.RequireServicePrincipal(OrderingDbContext.ModuleName)", "policy.RequireServicePrincipal(\"Catalog\")")),
    (["A_modules_service_principal_holds_no_policy_but_its_reactions"],
     lambda: edit(ORD + "Ordering.Infrastructure/OrderingModule.cs", "policy => policy.RequireAuthenticatedUser().RequireClaim(\"tenant\").RequireClaim(\"permissions\", OrderingPolicies.Read)", "policy => policy.RequireAuthenticatedUser()")),
    (["Every_reaction_in_the_solution_is_registered_with_the_outbox_dispatcher"],
     lambda: edit(ORD + "Ordering.Infrastructure/OrderingModule.cs", "        services.AddReactions(typeof(OrderingPolicies).Assembly);\n", "")),
    (["No_route_dispatches_a_reaction_command"],
     lambda: write("src/Api/Endpoints/Ordering/ConfirmOrderEndpoint.cs", "using Mediator;\nusing Microsoft.AspNetCore.Http.HttpResults;\nusing Ordering.Application.ConfirmOrder;\nusing Ordering.Contracts;\nusing SharedKernel;\n\nnamespace Api.Endpoints.Ordering;\n\npublic sealed class ConfirmOrderEndpoint : IEndpoint<ConfirmOrder>\n{\n    public static string Method => HttpMethods.Post;\n\n    public static string Pattern => \"/ordering/orders/{id:guid}/confirm\";\n\n    public static Delegate Handler => Handle;\n\n    private static async Task<Results<NoContent, ProblemHttpResult>> Handle([ClassOf<OrderId>] Guid id, ISender sender, CancellationToken cancellationToken) =>\n        (await sender.Send(new ConfirmOrder(new OrderId(id)), cancellationToken)).ToNoContent();\n}\n")),
    (["Every_options_class_the_solution_binds_has_a_validator"],
     lambda: edit(MESSAGING, "            .Validate(o => o.PollInterval > TimeSpan.Zero && o.BatchSize is > 0 and <= 1000, \"Outbox delivery needs a positive poll interval and a batch size of 1 to 1000.\")\n", "")),
    (["A_missing_database_connection_string_stops_startup", "Every_options_class_the_solution_binds_has_a_validator"],
     lambda: edit("src/Platform/Platform.Infrastructure/PlatformPersistence.cs", "            .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), $\"ConnectionStrings:{ConnectionStringName} is missing; every module's schema lives in that one database.\")\n", "")),
]


MODEL = "tests/Architecture.Tests"
SQL = "tests/Catalog.IntegrationTests"
UNIT = "tests/Platform.Tests"
DOMAIN_TESTS = "tests/Domain.Tests"

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
     lambda: edit(CAT + "Catalog.Domain/Products/ProductCreated.cs", "(ProductId ProductId, [Internal] DateTimeOffset At)", "(ProductId ProductId, [Internal] DateTimeOffset At, Product? Snapshot = null)")),
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
    (SQL, ["A_write_that_loses_a_race_is_a_failed_precondition_not_an_exception"],
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

    (MODEL, ["Published_contracts_keep_their_shape"],
     lambda: edit(CAT + "Catalog.Contracts/IntegrationEvents/ProductCreatedV1.cs", "DateTimeOffset At)", "DateTimeOffset OccurredAt)")),
    (MODEL, ["Published_contracts_keep_their_shape"],
     lambda: write(CAT + "Catalog.Contracts/IntegrationEvents/ProductDiscontinuedV1.cs", "using Platform.Application.Abstractions;\n\nnamespace Catalog.Contracts.IntegrationEvents;\n\npublic sealed record ProductDiscontinuedV1(ProductId ProductId) : IIntegrationEvent;\n")),
    (MODEL, ["No_committed_configuration_file_holds_a_secret"],
     lambda: edit("src/Api/appsettings.json", "Integrated Security=true;", "User Id=catalog;Password=Pa55word;")),

    (DOMAIN_TESTS, ["Every_domain_method_constructor_and_outcome_has_its_test"],
     lambda: edit(PRODUCT, "    public void Discontinue()", "    public bool IsInStock() => StockOnHand > 0;\n\n    public void Discontinue()")),
    (DOMAIN_TESTS, ["Every_domain_method_constructor_and_outcome_has_its_test"],
     lambda: (edit(CAT + "Catalog.Domain/Products/RenameOutcome.cs", "    Discontinued,", "    Discontinued,\n    Reserved,"),
              edit(CAT + "Catalog.Application/RenameProduct/RenameProductHandler.cs", "            RenameOutcome.Discontinued => CatalogProblems.ProductDiscontinued.With(),", "            RenameOutcome.Discontinued => CatalogProblems.ProductDiscontinued.With(),\n            RenameOutcome.Reserved => CatalogProblems.ProductDiscontinued.With(),"))),
    (SQL, ["RenameProduct_ReturnsPreconditionRequired_WhenIfMatchIsMissingOrAny", "ReceiveStock_ReturnsPreconditionRequired_WhenIfMatchIsMissingOrAny", "CancelOrder_ReturnsPreconditionRequired_WhenIfMatchIsMissingOrAny"],
     lambda: edit(PROGRAM, "        typeof(PreconditionBehavior<,>),\n", "")),
    (SQL, ["RenameProduct_ReturnsPreconditionFailed_WhenIfMatchIsStale", "ReceiveStock_ReturnsPreconditionFailed_WhenIfMatchIsStale", "CancelOrder_ReturnsPreconditionFailed_WhenIfMatchIsStale"],
     lambda: edit(BEHAVIOR, "if (unit.LoadedVersions.Any(v => !versioned.ExpectedVersion.Matches(v)))", "if (unit.LoadedVersions.Any(v => !versioned.ExpectedVersion.Matches(v)) && versioned.ExpectedVersion.Unmatchable)")),
    (SQL, ["CreateProduct_ReturnsPreconditionFailed_WhenCreateOnlyAndTheIdExists"],
     lambda: edit(CAT + "Catalog.Application/CreateProduct/CreateProductHandler.cs", "return command.CreateOnly ? PlatformProblems.PreconditionFailed.With() : command.ProductId;", "return command.ProductId;")),
    (SQL, ["RenameProduct_ReturnsNotFound_WhenTheProductIsAnotherTenants", "ReceiveStock_ReturnsNotFound_WhenTheProductIsAnotherTenants"],
     lambda: edit(REPO, "p => p.Id == id && EF.Property<Guid>(p, TenantColumn) == tenant", "p => p.Id == id")),
    (SQL, ["GetProduct_ReturnsNotFound_WhenTheProductIsAnotherTenants"],
     lambda: edit(QH, " AND TenantId = {tenant}", "")),
    (SQL, ["ListProducts_ReturnsNoProduct_OfAnotherTenant"],
     lambda: edit(LIST, ".AsNoTracking().Where(r => r.TenantId == tenant);", ".AsNoTracking();")),
    (SQL, ["Every_operation_refuses_a_principal_its_policy_does_not_admit"],
     lambda: edit(CAT + "Catalog.Infrastructure/CatalogModule.cs", '.RequireClaim("tenant").RequireClaim("permissions", CatalogPolicies.Write));', '.RequireClaim("tenant"));')),
    (SQL, ["A_principal_with_the_permission_but_no_tenant_is_refused"],
     lambda: edit(CAT + "Catalog.Infrastructure/CatalogModule.cs", 'policy.RequireAuthenticatedUser().RequireClaim("tenant").RequireClaim("permissions", CatalogPolicies.Read)', 'policy.RequireAuthenticatedUser().RequireClaim("permissions", CatalogPolicies.Read)')),
    (SQL, ["Every_step_of_a_paged_query_emits_a_span_nested_in_pipeline_order"],
     lambda: edit("src/Platform/Platform.Application/Behaviors/PagingBehavior.cs", '        using var activity = DispatchTelemetry.StartStep("page", GetType());\n', "")),
    (SQL, ["Every_step_of_a_command_emits_a_span_nested_in_pipeline_order", "Every_step_of_a_paged_query_emits_a_span_nested_in_pipeline_order"],
     lambda: edit("src/Platform/Platform.Application/Behaviors/AuthorizationBehavior.cs", '        using var activity = DispatchTelemetry.StartStep("authorize", GetType());\n', "")),
    (UNIT, ["A_write_to_a_loaded_aggregate_without_a_precondition_fails_the_dispatch"],
     lambda: edit(BEHAVIOR, "else if (unit.ChangesLoadedAggregates && context.Causation is null)", "else if (unit.ChangesLoadedAggregates && message is null)")),

    (SQL, ["Delivery_restores_the_writing_transactions_trace_context"],
     lambda: edit(DISPATCHER, "StartActivity(DispatchTelemetry.DeliverSpan, ActivityKind.Consumer, writer)", "StartActivity(DispatchTelemetry.DeliverSpan, ActivityKind.Consumer)")),
    (SQL, ["Delivery_restores_the_writing_transactions_trace_context"],
     lambda: edit(DISPATCHER, "delivery.TraceId.ToHexString()", "ActivityTraceId.CreateRandom().ToHexString()")),
    (SQL, ["A_redelivered_event_changes_nothing"],
     lambda: edit(DISPATCHER, "idempotencyKey: message.Id.ToString(),", "idempotencyKey: Guid.NewGuid().ToString(),")),
    (SQL, ["A_reaction_passes_the_initiating_user_and_its_cause_to_the_messages_it_writes"],
     lambda: edit(UOW, "InitiatingPrincipalId = dispatch.Causation?.InitiatingPrincipalId ?? dispatch.PrincipalId,", "InitiatingPrincipalId = dispatch.PrincipalId,")),
    (SQL, ["A_reaction_passes_the_initiating_user_and_its_cause_to_the_messages_it_writes"],
     lambda: edit(UOW, "        CausationId = dispatch.Causation?.MessageId,\n", "")),
    (SQL, ["A_reaction_runs_as_its_modules_service_principal_on_behalf_of_the_initiating_user"],
     lambda: edit("src/Platform/Platform.Application/Behaviors/Audit.cs", "context.Causation?.InitiatingPrincipalId, context.Causation?.MessageId);", "null, context.Causation?.MessageId);")),
    (SQL, ["A_saga_step_that_fails_after_a_compensable_one_runs_its_compensations"],
     lambda: edit(ORD + "Ordering.Application/ConfirmOrder/ConfirmOrderHandler.cs", "ConfirmOutcome.Abandoned => Compensate(order),", "ConfirmOutcome.Abandoned => order.Id == default ? Compensate(order) : Result.Success(),")),
    (SQL, ["A_message_whose_reaction_fails_stays_undelivered_for_the_next_pass"],
     lambda: edit(DISPATCHER, "DispatchTelemetry.ReactionFailed(logger, message.Id, reaction.Name, result.Problem!.Type.Uri);\n            return false;", "DispatchTelemetry.ReactionFailed(logger, message.Id, reaction.Name, result.Problem!.Type.Uri);\n            return true;")),
    (SQL, ["The_outbox_is_delivered_without_being_asked"],
     lambda: edit(MESSAGING, "        services.AddHostedService<OutboxDelivery>();\n", "")),
    (SQL, ["An_order_whose_stock_is_reserved_is_confirmed"],
     lambda: edit(UOW, "        TenantId = dispatch.Tenant?.Value,\n", "")),
    (SQL, ["An_order_whose_stock_is_reserved_is_confirmed"],
     lambda: edit(BEHAVIOR, "else if (unit.ChangesLoadedAggregates && context.Causation is null)", "else if (unit.ChangesLoadedAggregates)")),
    (SQL, ["GetOrder_ReturnsNotFound_WhenTheOrderIsAnotherTenants"],
     lambda: edit(ORD + "Ordering.Infrastructure/Queries/GetOrder/GetOrderHandler.cs", " AND TenantId = {tenant}", "")),
    (SQL, ["CancelOrder_ReturnsNotFound_WhenTheOrderIsAnotherTenants"],
     lambda: edit(ORD + "Ordering.Infrastructure/Persistence/OrderRepository.cs", "o => o.Id == id && EF.Property<Guid>(o, TenantColumn) == tenant", "o => o.Id == id")),
    (UNIT, ["A_reaction_that_loses_a_race_is_not_recorded_so_its_retry_runs_again"],
     lambda: edit(BEHAVIOR, "        if (attempt is not null && !lostRace)\n", "        if (attempt is not null && (lostRace || !lostRace))\n")),

    (MODEL, ["Published_messages_carry_their_version_in_their_name"],
     lambda: write(CAT + "Catalog.Contracts/CommandMessages/RestockProduct.cs", "using Platform.Application.Abstractions;\n\nnamespace Catalog.Contracts.CommandMessages;\n\npublic sealed record RestockProduct(ProductId ProductId) : ICommandMessage;\n")),
    (MODEL, ["Published_messages_leave_only_through_the_outbox"],
     lambda: write(ORD + "Ordering.Application/PlaceOrder/IStockClient.cs", "using Catalog.Contracts.CommandMessages;\n\nnamespace Ordering.Application.PlaceOrder;\n\npublic interface IStockClient\n{\n    void Reserve(ReserveStockV1 commandMessage);\n}\n")),
    (MODEL, ["Events_and_outbox_payloads_carry_nothing_restricted"],
     lambda: write(CAT + "Catalog.Contracts/CommandMessages/AuditSupplierV1.cs", "using Platform.Application.Abstractions;\nusing SharedKernel;\n\nnamespace Catalog.Contracts.CommandMessages;\n\npublic sealed record AuditSupplierV1(ProductId ProductId, [Restricted] string TaxNumber) : ICommandMessage;\n")),
]


def failed(out, name):
    return any(line.startswith("failed ") and f".{name}" in line for line in out.splitlines())


ONLY = [n for n in os.environ.get("SEEDS", "").split(",") if n]


def wanted(*names):
    return not ONLY or any(n in ONLY for n in names)


def label(project):
    return project.split("/")[-1][:6].lower()


def build_seed(name, expected, seed):
    fresh()
    try:
        seed()
        code, out = build()
        ok = code != 0 and expected in out
        detail = "" if ok else "\n" + "\n".join(l for l in out.splitlines() if "error" in l)[:2000]
        return [(name, ok, "build ", detail)]
    finally:
        done()


def project_seed(kind, project, names, seed):
    fresh()
    try:
        seed()
        code, out = build(project)
        if code != 0:
            return [(names[0], False, kind, "  SEED BROKEN\n" + "\n".join(l for l in out.splitlines() if " error " in l)[:2000])]
        code, out = test(project, names)
        return [(name, failed(out, name), kind, "") for name in names]
    finally:
        done()


def start_sql_server():
    """One SQL Server for every seed that needs one; each test run makes and drops a database of its own on it."""
    password = "Aa1!" + secrets.token_urlsafe(16)
    container = subprocess.run(
        ["docker", "run", "-d", "-e", "ACCEPT_EULA=Y", "-e", f"MSSQL_SA_PASSWORD={password}", "-p", "127.0.0.1::1433",
         "mcr.microsoft.com/mssql/server:2022-latest"],
        capture_output=True, text=True, check=True).stdout.strip()
    port = subprocess.run(["docker", "port", container, "1433"], capture_output=True, text=True, check=True).stdout.split(":")[-1].strip()
    for _ in range(120):
        ready = subprocess.run(
            ["docker", "exec", container, "/opt/mssql-tools18/bin/sqlcmd", "-C", "-S", "localhost", "-U", "sa", "-P", password, "-Q", "SELECT 1"],
            capture_output=True, text=True)
        if ready.returncode == 0:
            break
        time.sleep(1)
    else:
        subprocess.run(["docker", "rm", "-f", container], capture_output=True)
        sys.exit("SQL Server did not start")
    os.environ["RANCHO_TEST_SQL"] = f"Server=127.0.0.1,{port};User Id=sa;Password={password};TrustServerCertificate=true"
    return container


began = time.monotonic()
failures = []


def report(results, seconds):
    for name, ok, kind, detail in results:
        print(f"{'FAILS AS EXPECTED' if ok else 'NOT CAUGHT':18} {kind:6} {name}  ({seconds:.0f}s){detail}")
        if not ok:
            failures.append(name)


# The architecture seeds go in together: one copy, one build, one test run.
TEST = [(n, s) for n, s in TEST if wanted(n)]
if TEST:
    clock = time.monotonic()
    fresh()
    try:
        for _, seed in TEST:
            seed()
        code, out = build()
        if code != 0:
            print("Test seeds do not compile:\n" + "\n".join(l for l in out.splitlines() if "error" in l)[:4000])
            sys.exit(1)
        code, out = test("tests/Architecture.Tests", [n for n, _ in TEST])
        report([(name, failed(out, name), "test  ", "") for name, _ in TEST], time.monotonic() - clock)
    finally:
        done()

# Every other seed alone, WORKERS at a time.
jobs = ([(build_seed, (name, expected, seed)) for name, expected, seed in BUILD if wanted(name)]
        + [(project_seed, ("api", "tests/Api.Tests", names, seed)) for names, seed in API if wanted(*names)]
        + [(project_seed, (label(project), project, names, seed)) for project, names, seed in ISOLATED if wanted(*names)])

container = start_sql_server() if any(project == SQL and wanted(*names) for project, names, _ in ISOLATED) else None
try:
    def timed(job):
        function, args = job
        clock = time.monotonic()
        return function(*args), time.monotonic() - clock

    with concurrent.futures.ThreadPoolExecutor(max_workers=WORKERS) as pool:
        for results, seconds in pool.map(timed, jobs):
            report(results, seconds)
finally:
    if container:
        subprocess.run(["docker", "rm", "-f", container], capture_output=True)

total = (sum(1 for n, _, _ in BUILD if wanted(n)) + len(TEST) + sum(len(n) for n, _ in API if wanted(*n))
         + sum(len(n) for _, n, _ in ISOLATED if wanted(*n)))
print(f"\n{total - len(failures)} of {total} seeds caught in {(time.monotonic() - began) / 60:.1f} minutes")
sys.exit(1 if failures else 0)
