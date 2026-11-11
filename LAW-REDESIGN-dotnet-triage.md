# Phase 1 triage: the .NET inventory, row by row

Working file for the law redesign (`LAW-REDESIGN.md`). It is deleted in phase 6; the PR keeps the record. It covers the 136 active rows of `rancho-dotnet`'s `enforcement.md` at rancho 4.1.0; the 12 retired rows are left out.

The checks land in `templates/rancho-dotnet/`. The law they serve is `rancho-modular-dotnet` (open decision 1, taken as recommended).

## Increment 1: structural checks on a skeleton solution (done)

These need only projects and types, not a running platform. Every check below was shown to fail on a seeded violation: `python -I templates/check-rancho-dotnet.py` reports 40 of 40 seeds caught.

| Mechanism | Rows |
|---|---|
| MSBuild, `build/ProjectKinds.targets`: each project's kind, and the references and packages that kind allows | BE-01 (project and package half). BE-39, narrowed: the in-memory and SQLite EF providers are denied. |
| `build/BannedSymbols*.txt` per project kind | BE-32 (ambient transactions), BE-57 (scope templates, Serilog's static logger), BE-95 and BE-98 (banned half), BE-130 (banned half), BE-132 (clock); `Thread.CurrentPrincipal` and `ClaimsPrincipal.Current` from BE-01 |
| `.editorconfig`: built-in analyzers as errors | BE-35 (IDE0130), BE-57 (CA1848, CA2254), BE-113 (EF1002/EF1003, EF Core's own analyzer), BE-133 (VSTHRD100/101), BE-134 (VSTHRD002/103), BE-135 (CA1031) |
| Generic constraint, by construction | BE-78: `IRepository<TRoot, TId>` requires `TRoot : IAggregateRoot` |
| `tests/Architecture.Tests` (26 tests) | BE-01 (type half), BE-03 (namespaces, since IDE0130 ties them to folders), BE-04, BE-06, BE-07, BE-08, BE-12, BE-15, BE-35 (namespaces), BE-38 (attribute half), BE-40, BE-53, BE-79 (type half), BE-84, BE-93, BE-95 and BE-98 (Infrastructure half), BE-105, BE-106, BE-107, BE-108 (type half), BE-109, BE-129, BE-130 (Infrastructure half), BE-131 |

Where the new check differs from the old row:

- **BE-01 packages:** Infrastructure kinds may take any package except ASP.NET Core beyond Authorization and Data Protection, because modular law §2 admits "SDKs of the systems they adapt", which no allowlist can name. Application may reference `SharedKernel` directly, which modular law's four facts allow.
- **BE-113:** EF1003 refuses raw SQL built by concatenation or interpolation, which is the injection risk itself. A raw-SQL call with a constant string is no longer refused.
- **BE-135:** CA1031 has no fault-isolation marker. A deliberate boundary suppresses it with its reason.
- **BE-08:** a strongly-typed id is recognised structurally, as a type whose only instance state is one `Guid`.
- **Untested backstops:** `Only_Api_uses_AspNetCore…` and `A_module_reaches_another_module…` have no seed, because the build-time reference table makes those violations impossible to compile. They guard against anything arriving transitively.

Moved to increment 2, because they need EF migrations: BE-87 (a committed migration isn't edited) and BE-88 (a migration references no application code).

## Increment 2: checks that need the platform

These test `Result`, the operation declaration, the pipeline, the page and cursor, the outbox, the endpoint abstraction, the EF model or the built host, so the platform is built first. The required behaviour tests become the template's own reference tests, which modular law §11 already lists.

| Kind | Rows |
|---|---|
| Build checks | ROOT-09, ROOT-11, ROOT-12, ROOT-13, ROOT-16, ROOT-18, ROOT-19, BE-87, BE-88, BE-10, BE-11, BE-19 to BE-28, BE-30, BE-33, BE-34, BE-37, BE-38 (EF half), BE-50, BE-51, BE-54, BE-55, BE-56, BE-61, BE-70, BE-73, BE-79 (EF half), BE-80, BE-81, BE-82, BE-83, BE-85, BE-86, BE-90, BE-91, BE-96, BE-97, BE-108 (EF half), BE-110, BE-119 |
| Reference behaviour tests | BE-29, BE-89, BE-92, BE-111, BE-112, BE-115 to BE-118, BE-120, BE-122, BE-124 to BE-128, BE-137, BE-138, BE-139, BE-141 |

## Deferred: features modular law §12 doesn't cover yet

The rule for these is "stop and ask". Each row comes back as a check when the first project needs the feature, using the archived annex as source material.

| Feature | Rows |
|---|---|
| AI agents | BE-31, BE-58, BE-59, BE-62 to BE-69, BE-71, BE-72, BE-103, BE-114, BE-123 |
| Caching | BE-44 to BE-49, BE-60, BE-140 |
| Real-time | BE-74, BE-75, BE-121 |
| External authorities (modular §8 keeps the principle) | BE-99, BE-100, BE-101, BE-102, BE-104 |

## Dropped: the check costs more than the mistake

| Row | Reason |
|---|---|
| BE-17 `<Feature>Reader` naming | Naming. The reference code shows it. |
| BE-36 Queries folders mirror Application folders | Naming. The reference code shows it. |
| BE-94 Metric tag keys from a closed set | It needs a custom analyzer, and a wrong tag is cheap to fix |
| BE-136 No `#region` | Cosmetic |
| BE-09 Domain events in the past tense | Already dropped in 4.1.0 |
