# Upgrading to rancho 5.0

For every project that binds `rancho-dotnet` or `rancho-react`. Upgrade the three plugins together: `rancho` 5.0.0, `rancho-dotnet` 4.0.0 and `rancho-react` 3.0.0.

**What changed:**
- **Two files per tree:** an agent now reads two files, the core law and the stack law its tree binds, about 3,500 words instead of 9,000–15,000.
- **Checks replace prose:** the structure is enforced by the checks in this repository's `templates/`, and each tree's `CLAUDE.md` names the code to copy.
- **Retired:** roles, the contract law, annexes, inventories, rule ids, `LAW-EXCEPTIONS.md` and `ENFORCEMENT-STATUS.md`.

## Steps

1. **Bind every tree.**
   - Put one line in the `CLAUDE.md` at the tree's root: `Stack law: /rancho-dotnet:law`, `Stack law: /rancho-react:law`, or `Stack law: none - <reason>` for a tree no stack law governs.
   - A single-tree repository binds its root `CLAUDE.md`.
   - An unbound tree makes the agent stop: that is core §1, not a fault.
2. **List the reference code.**
   - Add a `## Reference` section to each tree's `CLAUDE.md`. For each kind of code, it names the implementation to follow: in .NET a command, a versioned write, a paged query, a reaction and an integration test; in React a query, a form, an edit form of a versioned resource, a route and a test.
   - Pick the best existing example of each kind, and fix it first if it breaks the law.
   - `templates/rancho-dotnet/CLAUDE.md` and `templates/rancho-react/CLAUDE.md` show the form.
3. **Adopt the template's checks.**
   - .NET:
     - `build/ProjectKinds.targets` and `build/BannedSymbols*.txt`;
     - the `.editorconfig` analyzers as errors;
     - the architecture, host and integration tests;
     - `build/check-migrations.sh` and `build/check-contracts.sh` in CI.
   - React:
     - `eslint.config.js` with its `eslint/` rules;
     - the strict `tsconfig.json` and `scripts/check.mjs`;
     - the tests;
     - `npm run verify`.
   - Keep any check of your own that the template lacks, renamed for what it checks rather than by rule id.
   - A failing check is fixed in the code, never by editing the check.
4. **Retire the 4.x records.**
   - Fold each `LAW-EXCEPTIONS.md` entry:
     - a check suppression becomes a suppression comment giving its reason, beside the code;
     - a `Restricted` field admitted at a boundary becomes an exception declared at that boundary, with a comment giving its reason, approved in review;
     - a disagreement with the law becomes an issue on `vensure-marketplace`.
   - Then delete `LAW-EXCEPTIONS.md` and `ENFORCEMENT-STATUS.md`.
   - Rule ids in comments (`BE-…`, `FE-…`, `ROOT-…`) are history: replace them with the reason when you next touch the code.
5. **Meet core §4 and §5.** Check the server against them; phase 7's re-runs found these gaps in an existing project:
   - **Concurrency:** a version in the request body (`expectedVersion`, `409` when stale) moves to a strong `ETag` sent back as `If-Match`, with `428` when it's missing and `412` when it's stale. The React law's edit-form rules depend on this.
   - **The OpenAPI document:**
     - `x-data-class` on every property and every parameter's schema;
     - a top-level `x-problem-types`;
     - `Idempotency-Key` declared on each keyed operation.
   - **Responses:** `Cache-Control: no-store` for `Confidential` and `Restricted` data, and `private` for a representation that depends on the principal.
   - **Tracing:** the correlation id is the W3C trace id, not a GUID of its own, and every problem carries it as `traceId`.
   - **Audit:** an audit trail of failed authorizations, executed commands and `Restricted` reads.

   Open an issue in your own repository for each gap until its migration lands. Meanwhile core §1 holds: say so in each change that touches it, and never extend the old convention to a new resource.
6. **One service principal per module.** Background work runs as the service principal of the module that owns it (.NET §5). Split a shared system principal per module, so each holds only its own module's grants.
7. **The React stack.**
   - `rancho-react` 3.0.0 is written for React Router, npm and Prismatic (`@prismhr/components-react`) in the PrismOne shell.
   - A tree on another router, package manager or UI library migrates before it binds 3.0.0.
   - Until it does, it stays on the 4.x law (see below).
8. **What 5.0 doesn't cover yet: stop and ask.**
   - .NET: caching, calls to AI models, real-time push and file uploads.
   - React: real-time push, and running outside the shell.
   - Code a 4.x annex governed stays as it is, but new work in those areas stops and asks.

## Staying on 4.x for now

The 4.x law is frozen under the `law-archive-4.1` tag. Its `marketplace.json` has the same name as the current marketplace, and Claude Code registers only one marketplace per name. So 4.x is served as a release channel: the `law-4.x` branch, which is the tag with its marketplace renamed `vensure-marketplace-4x`. A project that stays on 4.x commits the following to its `.claude/settings.json`:

```json
{
  "extraKnownMarketplaces": {
    "vensure-marketplace-4x": {
      "source": { "source": "github", "repo": "josefernandez-vensure/vensure-marketplace", "ref": "law-4.x" }
    }
  },
  "enabledPlugins": {
    "rancho@vensure-marketplace": false,
    "rancho-react@vensure-marketplace": false,
    "rancho@vensure-marketplace-4x": true,
    "rancho-react@vensure-marketplace-4x": true
  }
}
```

The 5.0 plugins are turned off in that project, so it never loads two `rancho` plugins. Try it on one machine before rolling it out. Leave the channel as soon as the migration lands; it gets no fixes.

## Done when

- Every tree is bound and has a `## Reference` section.
- The template's checks pass.
- No `LAW-EXCEPTIONS.md` or `ENFORCEMENT-STATUS.md` remains.
- Each gap from step 5 is either fixed or has an issue.
- A fresh session reads only `/rancho:law` and the bound law before writing code.
