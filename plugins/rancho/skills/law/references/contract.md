# Contract Law

This file is the **contract law**: what a server and a client MUST agree on across HTTP. It binds every tree whose stack law, or standalone law, declares a role (core §2). A `server` publishes this contract and a `client` consumes it. It states the shape of the wire, never how either side builds it; that is each stack law's.

Its rules are core rules. They use the core law's rule language (core §1) and `ROOT` ids, and they are listed in the core inventory, `enforcement.md` beside this file. "Core §N" is a section of the core law, `../SKILL.md`.

In a tree bound to a standalone law, the core law does not bind, and this file binds without it. There, MUST, SHOULD and MAY keep the meaning core §1 gives them. A citation of the core law points to what the standalone law states on the same subject, such as the data classes or the trace id. A standalone law that declares a role MUST use the core law's four data classes and the W3C trace id, because the wire carries both. A `⚙` here claims only that a check can be built: the standalone law says how its tree builds its checks, and the core inventory does not bind that tree.

## 1. Style and the document

- REST by default. RPC-style endpoints - `POST /<resource>/<verb>`, or `POST /<resource>/{id}/<verb>` for a transition of one resource, such as approving it - are permitted for an operation that is not a plain create, replace, update or delete. GraphQL is forbidden.
- The OpenAPI document is generated and committed. ⚙ ROOT-09
- Client API types are generated from that document, never hand-written. How a client consumes them - and that contract drift surfaces as a compile error rather than a runtime surprise - is the client stack law's.

## 2. Collections

- Every endpoint returning a collection whose size is not bounded by construction MUST be paginated. A collection bounded by construction - such as a lookup by a capped list of ids, or a fixed registry - MUST declare its bound as `maxItems` on its array schema, and a request that would exceed it is rejected, never truncated. ⚙ ROOT-11
- One page shape across the whole API. A per-feature paging envelope is a defect. ⚙ ROOT-12
- The maximum page size is enforced by the server. A request above it MUST be rejected, never silently truncated.

## 3. Idempotency

- Every state-changing endpoint MUST declare how it is idempotent: naturally, by client-supplied id, or by idempotency key. Undeclared is a defect. ⚙ ROOT-13
- Where a key is used it travels as the `Idempotency-Key` header, and a replay MUST return the original response rather than executing again. A replay is answered from the record of the first attempt before any precondition, such as `If-Match`, is evaluated: a retried write whose version has since moved returns its recorded response, not `412`.
- A request to an operation that declares a key and carries none is refused with `400`. The same key with a different payload is refused with `422`. A duplicate that arrives while the first attempt has not completed is answered as a replay once it completes, or with `409` where the server cannot wait for it - never with a `500`.

## 4. Limits

- Every endpoint MUST be covered by a rate limit policy. A rejection is `429`, with `Retry-After` wherever the limiter can estimate it, shaped as problem details (§7).

## 5. Errors

- Every failure response is RFC 9457 problem details, in the shape of §7 *Problem details*.
- An unexpected failure is answered `500`.
- Error payloads MUST NOT leak exception messages, stack traces, SQL, or internal type names.

## 6. Data classes

- A field's class MUST travel with its contract: the OpenAPI document carries the class (core §4) of every property of every request and response, and of every parameter. The client receives each class from the contract and enforces it at its own boundaries; the mechanism is the client stack law's. ⚙ ROOT-18
- An HTTP response carrying `Confidential` or `Restricted` data is sent `Cache-Control: no-store`.

## 7. Wire contract

Every format a client and a server must agree on is stated here.

- **Data class.** Every property schema and every parameter's schema in the OpenAPI document carries `x-data-class`, whose value is `public`, `internal`, `confidential` or `restricted` (core §4). The extension sits on the parameter's schema, not on the Parameter Object. Every property schema the document contains carries it, framework schemas included, such as problem details and the page.
- **Page.** Every paginated response has one shape: `items`, the page; `nextCursor`, an opaque string, or `null` on the last page; and `totalCount` only where the operation opts in. A request pages with the `pageSize` and `cursor` query parameters. Paging is keyset: the cursor is the only position a request may send. The server encrypts and authenticates the cursor, and the `x-data-class` of `nextCursor` and of the `cursor` parameter is `internal`, whatever the cursor's payload.
- **Stream.** A stream is one representation - a file or an export - sent as a body of its declared media type, never an unbounded JSON array of records. It is not a collection under §2, and it is under the export rule of core §4.
- **Problem details.** A problem's `type` is its identifier: a client decides by `type` and `status`, never by `title` or `detail`. Every problem a server returns has a `type` it declares, never a default per status. The OpenAPI document lists every type in a top-level `x-problem-types` object keyed by type URI, each value carrying `status`, `title` and `userFacing`. The validation problem type, returned with field errors, is user-facing. Field errors travel in `errors`, an object whose keys are paths into the request - into the body as JSON property names as sent, joined by `.`, with `[n]` for an array index, such as `address.lines[0]`; or a query or path parameter's name - and whose values are arrays of messages. `traceId` carries the trace id: the 32-hex-digit trace-id of *Trace context*, not a whole `traceparent`.
- **Concurrency.** A resource whose writes can conflict carries a version the application manages, which changes on every change to the resource and anything inside it. Its `ETag` is that version, as a strong entity tag. A response whose representation depends on the principal is sent `Cache-Control: private` or stricter, so no shared cache serves one principal's representation to another. A read returns the `ETag`, and a Response DTO of such a resource carries it as `etag`, so a client editing from a list can send it. `etag` holds exactly the `ETag` header's value, quotes included, and a client sends it back unchanged as `If-Match`. A write to an existing resource MUST carry it as `If-Match`, evaluated before the write is performed: a write without one, or with `If-Match: *`, is refused with `428`, and one whose version is stale with `412`. A create that names its own id carries `If-None-Match: *`, refused with `412` if the resource exists. A successful write that leaves the resource in place returns its new `etag` in its body; a successful delete returns no body (`204`).
- **Trace context.** Requests carry the W3C `traceparent` header, and `tracestate` when present. Its trace id is the correlation id of core §5. A server adopts the caller's `traceparent` only for a request that arrives through an edge that only callers inside its trust boundary - other services of the same deployment - can reach. Trust is decided by the edge, before authentication runs, never by the caller's credentials. A browser, or any other external caller, is outside it: the server starts a new trace and links it to the caller's wherever the caller's `traceparent` reaches it; an ingress that strips the header leaves nothing to link. The server's trace id reaches that caller as `traceId` on every problem details response.
- **Cross-origin.** Where a client is served from another origin, the server's CORS policy admits every request header this contract uses and exposes every response header a client reads, such as `ETag`, `Location` and `Retry-After`. The server stack law states the policy.
