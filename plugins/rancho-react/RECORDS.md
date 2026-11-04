# Records - React stack law

> Records that say a record is "in the core inventory" were written before 3.0 stage 1. The core records now live in `plugins/rancho/RECORDS.md`.

What the React stack law and its inventory no longer carry, because a coding agent does not need it to apply a rule: the reasoning behind wording that would otherwise look arbitrary, and the history of how the law came to read as it does. Nothing here is a rule. Every rule is stated in `skills/law/SKILL.md` or its annex, `skills/law/references/annex-realtime.md`.

## Rationale moved from SKILL.md in 3.0 stage 1

Moved word for word, grouped by the section of `skills/law/SKILL.md` it came from.

### §2 Structure - Feature names

After "Where a feature covers the same concept as a server module, it SHOULD reuse that module's name.":

Shared vocabulary is worth having. A build check that crosses into the other tree to enforce it is not, and a naming convention is not worth coupling two trees over.

### §3 API Types

After FE-34's bullet:

Without the class in the types, every data-boundary rule on this side is a hope rather than a check.

### §4 Server State - TanStack Query

After FE-21's mark:

The page you were handed is not the data set, and treating it as one produces a UI that is quietly wrong at scale.

### §9 Authentication

After "MUST NOT run a sign-in flow of its own.":

A second flow is a second session: sign-out in one would leave the other signed in.

After FE-30's mark:

A refresh token is a long-lived credential; reusing one found inside another token creates a credential-handling path nothing else in this section governs.

After the `403` rule:

Retrying a `403` is how a UI turns a clear denial into a hang.

After "and MUST clear the TanStack Query cache and every store.":

A previous user's data surviving a sign-out on a shared workstation is a disclosure, and shared workstations are the normal case in this domain.

After "which MUST end the provider session.":

Clearing local state while a provider session survives means the next sign-in completes silently as the previous user - the disclosure the rule above exists to prevent, reintroduced one step later.

### §11 Data Boundaries

After FE-16's mark:

They land in browser history, server access logs, and `Referer` headers sent to third parties.

## Records

Moved word for word from `skills/law/references/enforcement.md` in 3.0 stage 1. Paths in them are as written there, so `../SKILL.md` is this stack law, `skills/law/SKILL.md`. A record "in the core inventory" is now in `rancho`'s `RECORDS.md`, and one in `rancho-dotnet`'s inventory is now in that plugin's `RECORDS.md`.

### Rewordings applied

Three of the fifteen rewordings concern this stack law alone, and are kept here. R-01 to R-03 are in the core inventory, R-04 to R-12 in `rancho-dotnet`'s. Each entry records the rule, why the original wording resisted checking, and what replaced it. Records written before 2.0.0 name this file's stack law `frontend.md`, and call the core law the root; P-03, A-03, A-04 and A-05 are in the core inventory.

**R-13 · FE-10 · "Every list query MUST be paginated."**
The client cannot verify this; it consumes whatever the generated client exposes. *Applied:* the frontend ⚙ is dropped, the law now says outright that enforcement is server-side (BE-19), and the genuinely client-side half became a marked rule of its own - *no loop that exhausts `fetchNextPage` to assemble a whole collection*, FE-21.

**R-14 · FE-14 · "Inline `style` only for genuinely dynamic values."**
"Genuinely dynamic" is judgment. *Applied:* split. `ci`/`eslint` enforces the checkable half - no stylesheet import outside the global stylesheet and its layout partial (A-06) - and a static object literal passed to `style` becomes a lint error. Anything computed passes, which is the intent.

**R-15 · FE-16 · "Classified values MUST NOT appear in URLs or route/search params."**
The frontend has no classification metadata today, so nothing can decide which values are classified: the mark was fiction. *Applied:* the ⚙ is removed and the law states the condition on which it returns - classification carried into the OpenAPI document and the generated types, P-03 in the core inventory. The rule itself is untouched and still MUST. This is the one rewording that made a rule *less* enforced rather than more, because the alternative was leaving a mark that no check could ever honour.

### Amendments

A rewording keeps a rule and fixes how it is stated. An amendment changes what the law requires, under core §11 - the law first, the codebase after. A-06 and A-10 concern this stack law alone and are kept here. A-09 is in `rancho-dotnet`'s inventory; every other amendment is in the core inventory.

**A-06 · frontend §1, §2, §6, §7, FE-12, FE-14 · The law named a component kit and a styling system instead of a shape.**
Frontend §1 named Shadcn/ui and Tailwind, §2 gave `shared/ui/` to Shadcn primitives, §6 routed every form through Shadcn's `<Form>`, and §7 made Tailwind the only styling. The organization's applications are required to build on its own design system: a component library, itself built on a commercial component suite, published as a package with its own stylesheet, theme and icon set. An application on it could not comply with FE-12 or FE-14 whatever its own code did, because the `<Form>` and `shared/ui/` they name do not exist, and `../SKILL.md` §11 forbids working around the rule in one repository. What the rules protected still held: one form library with one schema per form, styling the codebase cannot drift from, and no stylesheet arriving from anywhere. Only their wording assumed a kit. Copying a kit into `shared/ui/` as editable source was also the opposite of what a design system needs. A local copy is a fork that drifts from the design system, and a wrapper layer in `shared/` is where that fork would grow back.
*Applied:* §1 names the UI library and the component library it is built on as a class, recorded in `DECISIONS.md`, as A-03 and A-05 did for the identity provider and library. §1 also gains FE-32: a UI-class package - one whose purpose is presentation, such as a component, styling, icon, animation or class-name package - exists only if the UI library depends on it, and application code imports no UI-class package but the UI library and its component library. The rule and its check are scoped to that class: the library's dependency closure also holds packages the application uses in its own right, such as its runtime, and a ban on the whole closure would forbid them. §2 removes `shared/ui/`. §7 replaces the editable-primitives rule with FE-31: features and `app/` use the library directly, `shared/` imports neither library, and a wrapper, adapter or re-export is forbidden. §7's styling rule and FE-14 become the library's stylesheet entry, its component library's theme and its icon stylesheet, imported once by the global stylesheet, plus one layout partial for layout the library has no component for. That partial uses only the library's own tokens and mixins, and never targets a library component; this is the pattern the organization's own application template uses. §6 and FE-12 keep React Hook Form + Zod, bind a library input through `Controller`, and forbid the library's own validator, so every form still has exactly one schema. Two clauses no build check reaches are recorded as reached by review in their rows: the `Controller` binding under FE-12, and a wrapper written inside a feature under FE-31. The packages each check reads are listed in the project's lint configuration, as FE-24's are, so no rule names a product (A-04). Nothing is weakened for an application whose UI library ships as editable source: it records that library in `DECISIONS.md` and uses it directly, and FE-31 forbids the wrapper layer `shared/ui/` used to invite. Frontend rows go from 30 to 32.

**A-10 · §3, §9, §13, FE-16, FE-17, FE-22, FE-23, FE-29 · The client could not handle unclassified request data, and a hosted application was told to end a session it does not own.**
Two items of a consuming project's compliance audit of 2026-10-30, 5 and 8, reach this stack law; A-08 in the core inventory carries the core half of item 5. *Classification:* the generated types carried the class of response properties only, so a value the user typed - a search term - had no class, was `Restricted` by the core rule, and could not reach a URL without a self-asserted brand. *Applied:* the server now emits the class of request properties and parameters too (BE-83), and the generated types carry each class as a type, such as a brand per class. Type-aware lints at the calls that cross each boundary - URL, storage, telemetry - read it; the telemetry lint supports FE-17's required test. A draft that had the generator emit per-boundary field-name sets was replaced before release: a field name does not say where a value came from, and following a value to its destination is beyond what ESLint can do, so those checks could not have been built as claimed. User input takes its brand only by being parsed through a schema the generator emits, such as a Zod schema whose output carries the brand; a TypeScript type cannot produce a runtime schema, so the schemas are generated, and a brand the application asserts for itself is forbidden (FE-34). The brand's own gap - a value with no brand, such as one rebuilt with string operations or client-only state, passes the lint - is recorded in FE-16, and the telemetry lint gains its own row, FE-33. *Sign-out:* §9 told every application to call the provider's end-session endpoint, while forbidding an application in a host shell a sign-in flow of its own - such an application does not own the provider session. *Applied:* it clears its own caches and stores and delegates to the shell's sign-out, which MUST end the provider session. The guarantee stands: the next sign-in prompts. Delegation changes who ends the session, not whether it ends. The review before release found three more things a hosted application cannot own in a shared session, and gives them to the shell the same way: the refresh after a `401` (applications refreshing one shared session would race), the choice of session store, and the request for a grant. Frontend rows go from 32 to 34, with FE-33 and FE-34.

### Build order

Sequenced by value per unit of effort, not by document order.

**First - the cheap ones that fail loudly.** FE-01 through FE-04, FE-11 through FE-13, FE-15, FE-18, FE-19 are almost all off-the-shelf ESLint configuration. **Enforced in about a day**, none requiring a decision.

**Second - the classification types.** FE-22 asserts the generated types carry the class the server stack law emits into the OpenAPI document (ROOT-18). That is what makes FE-16 and FE-23 enforceable on the client at all, so build it before either.

### Former status legend

The inventory's own status legend before 3.0 stage 1, word for word:

| Status | Meaning |
|---|---|
| `build` | Enforceable as worded. A violation can be made to fail the build. |
| `required-test` | No build check can reach it. The ⚙ is stripped and the obligation is named in a Testing section, which is where it is now caught. See [The badge problem](../rancho/RECORDS.md#the-badge-problem). |
| `dropped` | The ⚙ was removed. The rule still stands - it is enforced by review, not by the build. |
| `reword` | Not decidable as worded. Resolved in [Rewordings applied](#rewordings-applied). None remain. |

**Effort** is for the check, not the rule: `S` under an hour, `M` a day, `L` more than a day. The Effort column was dropped in 3.0 stage 1. Its values were: ROOT-18 M, FE-01 S, FE-02 S, FE-03 S, FE-04 S, FE-28 S, FE-05 S, FE-06 S, FE-07 M, FE-08 M, FE-09 M, FE-10 —, FE-11 S, FE-12 S, FE-13 S, FE-14 S, FE-15 S, FE-16 M, FE-17 M, FE-21 M, FE-22 M, FE-23 M, FE-24 S, FE-25 M, FE-26 M, FE-27 M, FE-29 M, FE-30 S, FE-31 S, FE-32 S, FE-33 M, FE-34 M, FE-18 S, FE-19 S, FE-20 S.

## Row history

Notes taken out of the inventory's Check cells in 3.0 stage 1, word for word except where a note was a bare citation, which is spelled out.

- FE-07: R-02 applied: `no-restricted-syntax` on casts of generated types
- FE-10: R-13 applied: enforced server-side by BE-19. Client half is now FE-21
- FE-12: Amended by A-06.
- FE-14: R-14 applied, amended by A-06.
- FE-16: Mark restored by P-03.
- FE-17: Mark stripped. A-10 adds a type-aware lint reading the class brand on telemetry call arguments; it supports the test and does not replace it, because whether an SDK's scrubbing is configured correctly is still a runtime fact
- FE-18: Amended by A-07
- FE-21: Added by R-13.
- FE-22: Added by P-03. Amended by A-10: the class reaches parameters as well as properties, and arrives as a type - such as a brand per class - rather than as metadata only a script can read.
- FE-23: Enabled by P-03. The check cited A-10 for reading the class brand on the value, and A-05 for excepting the identity library's session store in the rule itself.
- FE-24: Amended by A-05.
- FE-25: Same list of storage APIs as FE-23: the two rules previously named different ones. Amended by A-05: also flags a token value assigned to a module-scope binding.
- FE-27: Amended by A-05: in a host shell, the interactive step is the shell's sign-in page. Amended by A-10: in a host shell the refresh is the shell's too, and the application never refreshes a token itself
- FE-28: Added by A-01, scoped by A-02.
- FE-29: Added by A-03. Amended by A-10: in a host shell, the test proves sign-out is delegated to the shell's sign-out
- FE-30: Added by A-03.
- FE-31: Added by A-06.
- FE-32: Added by A-06.
- FE-33: Added by A-10.
- FE-34: Added by A-10.

## 3.0 stage 3

- §4: a mutation of a versioned resource sends `If-Match`, and a `412` refetches and tells the user rather than retrying. Closes A-18, A-35, NET-A-10.
- §6: field-level errors are the `errors` member of `ProblemDetails`. Closes A-27, FE-R-12.
- §10: a server error is mapped by `type` and `status`, `title` and field messages render only for a user-facing type, `detail` never; a shown error offers its `traceId`. Closes A-27, FE-R-12.
- §9: the token interceptor also sends `traceparent`, and the client attaches the trace id to its telemetry and error reports. Closes C-1, A-36, CORE-19.
- §3: the class's source in the OpenAPI document is named as `x-data-class`. Closes CORE-17.
- Core map: the correlation-id gap is closed by the §9 bullet; Gaps: none. Closes C-1, A-36, CORE-19.
- §13: new **Wire contract** required tests, FE-35 for `traceparent` and FE-36 for a `412`, with their rows; the Summary counts follow. Closes C-1, A-36, CORE-19 and A-18, A-35, NET-A-10.
- Review fix: §4: "Every list query MUST be paginated" and its server-side-enforcement sentence are replaced: a list query against a paginated endpoint pages with the page shape, and a collection the server bounds by construction arrives whole. FE-21's bullets are unchanged.
- Review fix: §4: a mutation of a versioned resource sends the `ETag` it read - from the response, or from a list item's `etag` field - as `If-Match`.
- Review fix: §9: the interceptor sends `tracestate` when present beside `traceparent`, and the client attaches its trace id to its telemetry and error reports.
- Review fix: Core map: core §9 is also carried by §9 Authentication, whose interceptor sends `traceparent`.
- Review fix: inventory: FE-35's section is §9 and FE-36's §4, the sections of their rules.
- Second review fix: §4: a mutation sends the `etag` it read - from the resource's `ETag` or `etag` field, a list item's `etag`, or the last write's response - as `If-Match`, since core now returns the new `etag` in a successful write's body. The representation-variant ETag and `Content-Location` were dropped from core as unowned and against RFC 9110 §8.7 / §13.1.1.
