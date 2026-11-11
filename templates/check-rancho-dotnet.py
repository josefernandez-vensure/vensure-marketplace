"""Prove the rancho-dotnet template's checks bite.

Seeds one violation per check into a copy of the template and confirms the build
or the architecture tests fail on it. Run it after changing a check.

Usage, from the repository root:  python -I templates/check-rancho-dotnet.py
"""
import shutil
import subprocess
import sys
import tempfile
from pathlib import Path

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


def write(path, text):
    p = WORK / path
    p.parent.mkdir(parents=True, exist_ok=True)
    p.write_text(text, encoding="utf-8")


def run(*args):
    r = subprocess.run(["dotnet", *args], cwd=WORK, capture_output=True, text=True, encoding="utf-8", errors="replace")
    return r.returncode, r.stdout + r.stderr


DOMAIN = CAT + "Catalog.Domain/Catalog.Domain.csproj"
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
     lambda: edit(HANDLER, "var product =", "_ = Task.FromResult(1).Result;\n        var product =")),
    ("async void", "VSTHRD100",
     lambda: edit(HANDLER, "    public async ValueTask", "    public async void Fire() => await Task.Yield();\n\n    public async ValueTask")),
    ("Swallowing Exception", "CA1031",
     lambda: edit(REPO, "await db.SaveChangesAsync(cancellationToken);",
                  "try { await db.SaveChangesAsync(cancellationToken); } catch (Exception) { }")),
    ("Logging without [LoggerMessage]", "CA1848",
     lambda: write("src/Api/Seed.cs", "namespace Api;\n\ninternal static class Seed\n{\n    public static void Log(Microsoft.Extensions.Logging.ILogger logger) => Microsoft.Extensions.Logging.LoggerExtensions.LogInformation(logger, \"hello\");\n}\n")),
    ("Namespace does not match folder", "IDE0130",
     lambda: write(CAT + "Catalog.Domain/Products/Misplaced.cs", "namespace Catalog.Domain.Elsewhere;\n\npublic sealed record Misplaced(int Value);\n")),
    ("Raw SQL", "EF1003",
     lambda: write(CAT + "Catalog.Infrastructure/Queries/GetProduct/Raw.cs", "using Catalog.Infrastructure.Persistence;\nusing Microsoft.EntityFrameworkCore;\n\nnamespace Catalog.Infrastructure.Queries.GetProduct;\n\ninternal static class Raw\n{\n    public static IQueryable<int> Count(CatalogDbContext db, string table) => db.Database.SqlQueryRaw<int>(\"SELECT COUNT(*) AS Value FROM \" + table);\n}\n")),
    ("Waiting on a task with a timeout", "VSTHRD103",
     lambda: edit(HANDLER, "var product =", "_ = Task.Delay(1).Wait(TimeSpan.FromSeconds(1));\n        var product =")),
    ("Blocking on a task in a synchronous method", "VSTHRD002",
     lambda: write(CAT + "Catalog.Application/CreateProduct/Sync.cs", "namespace Catalog.Application.CreateProduct;" + chr(10) + chr(10) + "public static class Sync" + chr(10) + "{" + chr(10) + "    public static int Get(Task<int> task) => task.Result;" + chr(10) + "}" + chr(10))),
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
              edit(REPO, "    public async Task Save", "    public IQueryable<Product> Query() => db.Products;\n\n    public async Task Save"))),
    ("Messages_and_handlers_live_where_they_belong",
     lambda: write(CAT + "Catalog.Application/GetProduct/MisplacedHandler.cs", "using Mediator;\n\nnamespace Catalog.Application.GetProduct;\n\npublic sealed class MisplacedHandler : IQueryHandler<GetProduct, ProductResponse?>\n{\n    public ValueTask<ProductResponse?> Handle(GetProduct query, CancellationToken cancellationToken) => ValueTask.FromResult<ProductResponse?>(null);\n}\n")),
    ("A_query_never_writes",
     lambda: edit(QH, "        var id = query.ProductId.Value;", "        await db.SaveChangesAsync(cancellationToken);\n        var id = query.ProductId.Value;")),
    ("A_query_never_loads_an_aggregate",
     lambda: edit(QH, "        var id = query.ProductId.Value;", "        _ = await db.Products.CountAsync(cancellationToken);\n        var id = query.ProductId.Value;")),
    ("Messages_and_responses_carry_no_domain_types",
     lambda: edit(CAT + "Catalog.Application/GetProduct/ProductResponse.cs", "string ETag)", "string ETag, Catalog.Domain.Products.ProductName? Display = null)")),
    ("Handlers_do_not_log",
     lambda: (edit(QH, "internal sealed class GetProductHandler(CatalogDbContext db)", "internal sealed class GetProductHandler(CatalogDbContext db, Microsoft.Extensions.Logging.ILogger<GetProductHandler> logger)"),
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
    ("No_static_mutable_state",
     lambda: write(CAT + "Catalog.Infrastructure/Persistence/Counter.cs", "namespace Catalog.Infrastructure.Persistence;\n\ninternal static class Counter\n{\n    public static int Hits;\n\n    public static void Hit() => Hits++;\n}\n")),
]

failures = []

for name, expected, seed in BUILD:
    fresh()
    seed()
    code, out = run("build", "Starter.slnx", "-nologo")
    ok = code != 0 and expected in out
    print(f"{'FAILS AS EXPECTED' if ok else 'NOT CAUGHT':18} build  {name}")
    if not ok:
        failures.append(name)
        print("\n".join(l for l in out.splitlines() if "error" in l)[:2000])

fresh()
for _, seed in TEST:
    seed()
code, out = run("build", "Starter.slnx", "-nologo")
if code != 0:
    print("Test seeds do not compile:\n" + "\n".join(l for l in out.splitlines() if "error" in l)[:4000])
    sys.exit(1)
code, out = run("test", "--solution", "Starter.slnx", "--no-build")
for name, _ in TEST:
    ok = f"{name} " in out and any(name in l and "failed" in l.lower() for l in out.splitlines())
    caught = f".{name}" in out or f" {name}" in out
    print(f"{'FAILS AS EXPECTED' if caught else 'NOT CAUGHT':18} test   {name}")
    if not caught:
        failures.append(name)

shutil.rmtree(WORK, ignore_errors=True)
print(f"\n{len(BUILD) + len(TEST) - len(failures)} of {len(BUILD) + len(TEST)} seeds caught")
sys.exit(1 if failures else 0)
