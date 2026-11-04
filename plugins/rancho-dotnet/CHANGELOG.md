# Changelog

## 1.1.0

- Amendment A-09, from a consuming project's compliance audit of 2026-10-30
  (items 1 to 4, 6, 9 and 10), with six conflicts the owners' review found
  and resolved in the same change. Record in `references/enforcement.md`.
  Carries the core law's 2.1.0 (A-08).
- **What each project does to upgrade:**
  - Reference `<Module>.Application` from `Api`, drop any transitive or
    `InternalsVisibleTo` route to it, and give each module's Infrastructure
    one `Add<Module>Module(IServiceCollection, IConfiguration)` that builds
    and registers its own clients, including its `DbContext`.
  - Pin Mediator 3.x. Make Infrastructure types `internal`, except
    registration entry points, concrete query handlers, which the generated
    registration names, and the types their constructors take.
  - Move an external authority's client into one Infrastructure adapter
    behind a port; nothing else references its types.
  - Move command and query message ids, and Response DTO ids, to the
    module's `Contracts` ids; a message references no `Domain` type.
  - Rewrite query handlers to read through Infrastructure read types or
    parameterized SQL into the DTO, never a `Domain` type; test each SQL
    handler against real SQL Server, and remove any raw-string SQL call.
  - Give each command that does not persist a client-supplied id, and move
    duplicating external effects, such as email, behind the outbox.
- §2: `Api` references `<Module>.Application` and names only Infrastructure
  registration entry points (BE-105, BE-106). Infrastructure types are
  `internal` and `InternalsVisibleTo` names test assemblies only (BE-34,
  BE-107). The composition root is defined as `Api` plus the registration
  entry points it calls, and BE-52, BE-95 and BE-98 are scoped to it. The package column admits `Microsoft.Extensions.*.Abstractions`,
  options and its configuration extensions, and the SDK an adapter wraps, and
  BE-01 now checks packages.
- §3: messages and Response DTOs reference no `Domain` type and carry
  `Contracts` ids (BE-109). BE-08 narrows to `Domain` and the messages.
- §4: every concrete query handler is in Infrastructure by convention, and
  references no `Domain` type (BE-108). It reads through Infrastructure read
  types or parameterized SQL, which applies the module's query filters itself
  and is tested against a real database (BE-112); raw-string SQL APIs are
  banned (BE-113). A command declares whether it persists; one that does not
  opens no unit of work, uses neither a key nor the outbox, and is idempotent
  naturally or by a client-supplied id, which an agent's invocation carries
  (BE-110, BE-111, BE-31). An effect a repeat would duplicate goes through the
  outbox. A conflict at commit becomes a `Result` conflict in the transaction
  behavior.
- §5: the adapter that receives a concurrency or unique-key exception
  translates it. §7: a class is declared on the domain type and its
  `Contracts` twin, which must agree, and a property whose type has no class
  declares one (BE-54); encryption of `Restricted` columns may live in the
  driver, and is transparent to every read path; the class is emitted on
  request properties and parameters (BE-83).
- §6: an external authority is reached through one Infrastructure adapter,
  which declares its SDK and translates its codes and failures (BE-102).
  §8: tool parameters are wire values from `Contracts`.
- §13: an endpoint may reference the message it dispatches and its DTOs' own
  types (BE-82). §15 admits a declared fault-isolation boundary and
  `IServiceScopeFactory` in a hosted service. BE-43's check skips
  compiler-generated types.
- Infrastructure types the project maps or binds - entities, read types,
  options classes - are an adapter kind (BE-34). The registration entry point
  sits at the project root (BE-35). An agent's client-supplied id has a
  required test (BE-114).
- Rows go from 104 to 114, and marks from 90 to 95 (the summary had counted
  91). The role carries 15 core rules, with ROOT-20.

## 1.0.0

- Split out of rancho 1.7.0, where this stack law was
  `skills/law/references/backend.md`. Released with rancho 2.0.0, whose core
  law it carries; amendment A-07 in the core inventory records why.
- Opens with the sections every stack law carries under core §2: its role, a
  core map from each core section to the section here that carries it, and
  its gaps. Then the stack law itself, §1–§15, numbered as before. The rules
  are unchanged: references to the root law now read core, and §7 says the
  class reaches every client, where it said the frontend.
- §16 Verification holds the `dotnet build` and `dotnet test` gate the core
  law's §10 used to state.
- The repository map the core law's §2 used to draw for `backend/` is now this
  stack law's **Layout**, relative to the tree root.
- `references/enforcement.md` holds this stack law's rows, the core rules its
  role carries under **Core rules carried**, and the records that concern this
  stack law alone.
