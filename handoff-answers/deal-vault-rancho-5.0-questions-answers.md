# Answers · Questions for the rancho 5.0 law's owners

To the Deal Vault team, who sent "Hand-off · Questions for the rancho 5.0 law's owners". From the owners of the engineering law (`vensure-marketplace`).

**Read against:** `rancho` 5.0.0, `rancho-dotnet` 4.2.1, `rancho-react` 3.0.1, and the templates in `vensure-marketplace` as of `e16f604` (2026-11-24). These are the versions you bound to.

**Releases:** the answers are in `rancho` 5.0.1, `rancho-dotnet` 4.3.0 and `rancho-react` 3.1.0. Each plugin's `CHANGELOG.md` gives the upgrade steps. Release: https://github.com/josefernandez-vensure/vensure-marketplace/releases/tag/rancho-5.0.1

| # | Item | Answer | What changed | What you do |
| --- | --- | --- | --- | --- |
| Q-1 | Per-row failure isolation | **Allowed, at a delivery boundary.** | .NET §4 names the delivery boundary | Copy the template's two catches |
| Q-2 | Replay after `If-Match` | **No conflict in substance.** A true replay always passes the header-shape check. | Core §5 names the version comparison | Nothing; keep the template's order |
| Q-3 | ESLint 9 or 10 | **ESLint 10.** | React §2 and the template are on ESLint 10 | Stay on 10 |
| Q-4 | File uploads | **Acknowledged.** We will tell you when upload rules are added. | Nothing yet | Check two points against core §2 and §3 |
| Q-5 | Syncfusion licence key | **A client key, not a secret.** | React §8 names it; the template README says where to register it | Keep R2-38 |
| Q-6 | No tenant | **Allowed. Declare it.** | The template gains `[assembly: NoTenantScope]`; .NET §9 names it | Declare it in `Api`, naming R2-12 |

---

## Q-1 · Per-row failure isolation when only an adapter may catch

**Answer: allowed, at a delivery boundary.** `rancho-dotnet` 4.3.0, §4, adds:

> The one other catch is a delivery boundary in `Infrastructure`: one iteration of a hosted service's loop, or the delivery of one message. It catches any exception but cancellation, logs it, translates nothing, and leaves the item for the next pass, so one failure stops neither the host nor the items behind it.

The template already had both. Copy them:

- `src/Platform/Platform.Infrastructure/Messaging/OutboxDispatcher.cs`, `React`. It handles one reaction to one message. It returns `false`, so the message stays undelivered for the next pass. The dispatcher then moves on to the next message.
- `src/Platform/Platform.Infrastructure/Messaging/OutboxDelivery.cs`, `ExecuteAsync`. It handles one pass of the hosted service's loop, then waits for the next tick.

Both catch `Exception when (exception is not OperationCanceledException)`. Each carries a CA1031 suppression whose reason states what the boundary does.

**Why the law changed.** You read 4.2.1 correctly: its §4 left no compliant way to write these loops, and the template broke its own law. Under core §1, the planned comments of your audit session 1.2, arguing that §4 doesn't apply, would have been a defect. Now the suppression's reason is a fact §4 states. The `law-reviewer` agent reports any other catch as a finding.

**A gap we know of.** The template has no attempt limit and no dead-letter. A message that always throws is retried, and logged, on every pass. It never blocks the messages behind it. If you need a limit, ask: the law has no rule for one yet.

## Q-2 · A replay is answered after the precondition check

**Answer: no conflict in substance. Keep the template's order.** `rancho` 5.0.1, core §5, now reads:

> A replay returns the original response without executing again, and is answered before the version its `If-Match` names is compared with the resource's.

`PreconditionBehavior` reads only the request. It answers `428` when `If-Match` is missing or `*`, and `412` when the tag can never match. It reads no state. A replay repeats the original request, headers included. So a replay passes `PreconditionBehavior` exactly when the original did, and reaches the attempt lookup in `UnitOfWorkBehavior`. The comparison with the resource's current version, the one that would wrongly refuse a replay, runs after that lookup.

**Your narrow case is not a replay.** `UnitOfWorkBehavior.Fingerprint` hashes the whole command, and the command carries `ExpectedVersion`. A request with the same key but a missing or different `If-Match` therefore has a different fingerprint. Even with the lookup moved first, it could never get the original response. It would get `422`, a key reused with another payload, instead of `428` or `412`. Either way the request is refused. The shape check answers first, as validation answers `400` before the key is looked up.

## Q-3 · ESLint major version

**Answer: ESLint 10. Don't downgrade.**

The law named ESLint 9 too, not only the template. `rancho-react` 3.1.0 changes §2 *Stack* to "ESLint 10, type-aware". It also moves the template to `eslint` `^10.15.0` and `@eslint/js` `^10.0.1`. npm already marks ESLint 9.39.5 as out of support.

What we verified on the template, under ESLint 10:

- `npm run verify` passes.
- A file of seeded violations is caught by every family of rules: the core rules, `boundaries`, `react-hooks`, `eslint-comments`, and the template's own and type-aware rules.
- `templates/check-rancho-react.py` catches all 77 of 77 seeds.

When you adopt the template's `eslint.config.js` (audit 5.5), take `@eslint/js` `^10` with it.

## Q-4 · File uploads have no rules

**Acknowledged.** Thank you for your design. We will tell you when upload rules are added to `rancho-dotnet`, so you can check your design against them.

Two core rules bind your uploads already, because core rules apply even where .NET §10 is silent:

- **Core §2** reads: "An uploaded file is untrusted until the server has proven its type from its content, stored it under a name it chose where it can't execute, and seen a malware scan pass." Your summary covers the scan and the Excel, CSV and PDF allowlist. Make sure R2-40 also states two things:
  - the type is proven from the file's content, not from its extension or its `Content-Type`;
  - the server chooses the stored name, never the client's file name.
- **Core §3:** a class belongs to the data. Classing every file's contents `Confidential` holds only if no file can hold `Restricted` data. A deal's spreadsheets can hold government identifiers, bank details or dates of birth, and those are `Restricted`. The class decides whether the contents may be cached, sent to a model or exported, and whether they are encrypted at rest. Confirm the class in R2-7, or raise it.

## Q-5 · Is a Syncfusion licence key a secret?

**Answer: no. It is a client key, as R2-38 reads it.** It grants no access to any data or system; it only removes a banner, and Syncfusion requires it in the browser. `rancho-react` 3.1.0, §8, now reads:

> The bundle holds no secret. A client id may be in it, and so may a licence key its vendor requires in the browser, such as Syncfusion's; neither is committed.

Your practice meets this: CI injects the key at build time from an organization secret, and the key is never committed.

**The template.** The template has no grid, so it registers no key. Its README (*Setup*) now says what a project that adds the grid does:

- Register the key once, in `src/app/main.tsx`, from a build-time variable such as `VITE_SYNCFUSION_LICENSE_KEY` declared in `src/env.d.ts`.
- Add `'!@syncfusion/ej2-base'` to `UI_CLASS_PACKAGES` in `eslint.config.js`. Its `registerLicense` import otherwise counts as a presentation package.

## Q-6 · Tenant checks when access is global

**Answer: allowed. Declare it.** The law always allowed a tree with no tenant:

- core §2 requires a per-record check "unless the policy grants every record", and R2-12's policy does;
- .NET §7 applies "the principal's scope, such as its tenant", and a tree with no scope has none to apply.

The template's checks assumed a tenant. That was the template's gap, not yours. Don't add a tenant column you never filter by.

**The change.** `rancho-dotnet` 4.3.0 adds `NoTenantScopeAttribute`, in `src/Platform/Platform.Application/Abstractions/NoTenantScope.cs`. Declare it once, in `Api`:

```csharp
[assembly: Platform.Application.Abstractions.NoTenantScope("DECISIONS.md R2-12", "Every signed-in user may read every record; roles restrict only actions.")]
```

With the declaration:

- `Every_read_applies_the_principals_tenant` and `Every_aggregate_root_is_stored_with_its_tenant` are skipped, and the test output shows the declaration's reason.
- A new test, `Only_the_composition_root_declares_no_tenant_scope_and_names_why`, refuses the declaration anywhere but `Api`, or without a decision and a reason.
- .NET §9 now says such a tree "has no tenant to test", so the "another tenant's record" test is not required.

**Upgrade.** Take the attribute, `Solution.NoTenantScope` and `Solution.SkipWithoutTenantScope` (`tests/Architecture.Tests/Solution.cs`), the new test in `InfrastructureTests`, and the skip at the top of the two tenant tests. Then add the declaration. There is no interim step to take: don't remove the tenant tests.

**When R2-12 ends.** If access stops being global, remove the declaration. The tenant column, the scoped reads and §9's tests then come back together, and the checks enforce them again.
