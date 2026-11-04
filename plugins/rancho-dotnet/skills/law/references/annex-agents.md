# Agents annex - .NET stack law

Binds a tree once the tree meets its trigger: the first code that sends data to a model or exposes an operation to one as a tool - an agent manifest, `Microsoft.Agents.AI`, an MCP server, Semantic Kernel, a direct model client, or any other. Read it in full before writing or reviewing code in a tree that already meets the trigger, or in work that would make it meet it.

This annex is part of the .NET stack law, `../SKILL.md`, and uses its section numbers: each section below holds the agent rules of the section of that name.

## 4. Application Layer

### Pagination

- An agent-reachable paginated query MUST declare a lower maximum page size than the UI's, in the manifest (§8). A collection bounded by construction is returned whole, and its rows count against the run's row budget (§9).

### Idempotency

- An agent tool invocation of a command that declares a key MUST carry a key derived from the run and the step. One that declares a client-supplied id carries the same id on every attempt, and a naturally idempotent one carries none. An agent that re-invokes a tool after a timeout MUST NOT produce a second write. ⚙ BE-31

## 6. Authorization

### External authorities

- Externally-resolved permissions are the user's permissions, not the caller's. For an agent-actor principal they MUST be intersected with the agent's manifest exactly as §8 requires.

## 7. Data Classification

### The model-visible surface

- Every model-visible DTO MUST be explicitly marked as such. ⚙ BE-58
- Reusing a UI Response DTO for a tool because the shape happens to fit is how a `Confidential` field reaches a vendor. Where the two audiences differ in class, they are two types.
- An architecture test MUST assert that no model-visible DTO contains a `Confidential` or `Restricted` property, transitively through every nested type and collection. ⚙ BE-59
- Free text a user can write into - notes, comments, descriptions, reasons - MUST be treated as `Confidential` when it is model-visible.
- A tool that needs a sensitive value in order to work is the wrong design. Move the work behind an operation and give the model the outcome, not the input: a model that must "check the national id" instead invokes an operation that checks it.
- Redaction MUST happen in the projection to the tool DTO, never in a prompt instruction. Prompt text is not a control (§8).

## 8. AI Agents

An agent is a caller with a fixed toolset. Its limits are enforced by code. **Prompt text is not a security control.**

### Manifest

- Each agent MUST have exactly one manifest listing the operations it may invoke.
- Tools MUST be projected from the manifest at composition time. Hand-registering a tool is forbidden. ⚙ BE-62
- The scopes requested for a run MUST derive from that same manifest. ⚙ BE-63
- An architecture test MUST assert every manifest entry resolves to a real handler whose declared policy matches. ⚙ BE-64

### Effective permissions

```
effective = user's permissions  ∩  agent manifest  ∩  operations not marked HumanRequirement.Required
```

- An agent principal is a strict narrowing of the calling user's: never equal, never wider (core §7).
- Human-initiated runs use on-behalf-of: the agent carries a down-scoped principal derived from the caller's token.
- The principal MUST carry an actor claim identifying the agent. Writes audit the agent, not only the user.
- An agent-actor principal MUST NOT start another agent run. That is the recursion cap.
- Autonomous runs require a separately declared manifest. It MUST NOT be inherited from the human-initiated one and MUST NOT contain any operation marked `HumanRequirement.Required`. ⚙ BE-67

### Tool shape

- Tool parameters MUST be typed wire values - `Contracts` types and the declared wrapper types below - never a `Domain` type (§3).
- A tool parameter MUST NOT be a bare `string`. Every string-valued parameter is a declared wrapper type - `SearchTerm`, `CommentBody` - so that admitting a new kind of free text is a deliberate, reviewable act. ⚙ BE-68
- No wrapper type may be declared for a URL, route, endpoint name, table name, column name, or query text. Anything that selects behavior is not data.
- Agent reads MUST go through the same query handlers as the UI. An agent-only data path is forbidden; it is where every control above gets bypassed.
- Tool output returned to a model MUST be a Response DTO. Domain objects and raw rows MUST NOT be serialized to a model. ⚙ BE-69

Verify Microsoft Agent Framework API specifics against official Microsoft documentation. Do not infer them.

## 9. Quotas and Rate Limits

### Agent runs

- Every agent run MUST carry a budget fixed when the run starts: maximum tool invocations, maximum rows returned across the whole run, maximum wall-clock duration, and maximum model spend. ⚙ BE-71
- Budgets MUST be enforced in the dispatch pipeline. A tool MUST NOT be trusted to count its own calls, and a model MUST NOT be asked to respect a limit stated in a prompt.
- Exceeding a budget MUST abort the run with a stated reason. It MUST NOT silently truncate: a partial read that looks complete is how a model reaches a confident wrong answer, and the caller cannot tell the difference.
- Commands already committed when a run aborts stay committed. The run outcome MUST report which operations completed.
- An autonomous run MUST have its own budget, declared with its own manifest (§8) and never inherited from the human-initiated one.
- Agent retries MUST be bounded and MUST reuse the original idempotency identity (§4).

## 10. Observability

### Correlation

- An agent run MUST carry its own run id **and** the correlation id of the work that started it. Every tool invocation logs both, plus the step id that keys its idempotency (§4).

## Testing

The required tests of the rules above. `../SKILL.md` §14 states what a required test is.

- **Authorization**: An agent-actor principal MUST have a test proving an operation the user holds but the manifest omits is denied when permissions come from the external authority (BE-103).
- **Agents**: every manifest MUST have a test asserting an operation outside it is rejected, a test proving the acting principal carries the agent's actor claim (BE-65), and a test proving an agent-actor principal cannot start another agent run (BE-66).
- **Budgets**: every budget MUST have a test proving a run that exceeds it aborts rather than truncates (BE-72).
- **Idempotency**: Every agent-reachable command declaring a client-supplied id MUST have a test proving that a re-invocation after a timeout carries the same id and writes once (BE-114).
