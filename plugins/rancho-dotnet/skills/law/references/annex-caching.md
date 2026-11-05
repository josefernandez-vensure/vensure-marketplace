# Caching annex - .NET stack law

Binds a tree once the tree meets its trigger: the first code that reads or writes a cache - a cache decorator, `IMemoryCache`, `IDistributedCache`, `HybridCache`, output caching, Redis, or any other. Read it in full before writing or reviewing code in a tree that already meets the trigger, or in work that would make it meet it.

This annex is part of the .NET stack law, `../SKILL.md`, and uses its section numbers: each section below holds the caching rules of the section of that name. The idempotency store's exclusion of Redis stays in `../SKILL.md` §4.

## 5. Persistence

### Redis

- Redis is a cache, a SignalR backplane, and short-lived coordination state. It MUST NOT be a source of truth.
- Every entry MUST have an explicit TTL. Unbounded entries are forbidden.
- Keys MUST be namespaced by module.
- The system MUST be correct with an empty cache. A cold cache is a performance event, not a failure.

### Read-side caching

Caching is a **decorator over `IQueryHandler<TQuery, TResponse>`**, never a branch inside a handler. One generic decorator serves every cached query: check Redis, fall through to the database handler on a miss, populate on the way out. Both paths return the same DTO.

- Decorators MUST be registered explicitly per query at the composition root, never auto-applied, so the cached set is auditable. ⚙ BE-44
- The decorated and undecorated paths MUST return the same type and the same values. Removing the decorator changes latency, never behavior.
- A decorator MUST contain cache logic only. Any business rule inside one is a defect - it is invisible to every test that runs the handler directly.
- Cache keys MUST be derived from the query message type and its parameter values, and MUST carry a schema version segment so a changed DTO shape cannot deserialize into a stale one.
- **Every cache key MUST include the acting principal**, added by the one key builder every cached query uses. ⚙ BE-45

**Invalidation.** A query populates the cache on a miss; a write to an aggregate invalidates every entry derived from it. Entries are not enumerable from an aggregate id, so invalidation uses **version stamps**, not key deletion.

- Redis holds a stamp per aggregate type (`ver:<module>:<aggregate>`) and, for queries targeting a single instance, per id.
- Every cache key MUST embed the stamps of the aggregates its result depends on, and a cached query MUST declare those dependencies explicitly. ⚙ BE-46
- A write increments the relevant stamps. Old keys become unreachable and expire by TTL.
- Stamps MUST be incremented **after** the transaction commits, never before.
- Enumerating keys - `KEYS` or `SCAN`, for invalidation or anything else - is forbidden. ⚙ BE-47
- Command handlers MUST NOT touch the cache. Invalidation runs after the unit of work commits: the unit-of-work behavior hands the committed domain events to an Application slice, which invalidates through an `ICacheInvalidator` port implemented in Infrastructure. ⚙ BE-48 It is a post-commit effect, not an in-transaction reaction (§3).
- An invalidation failure MUST be logged at `Error`, never silently discarded. The committed result is returned; the TTL bounds how long the stale entry is served. The mandatory TTL is the backstop bounding how long a failed invalidation serves stale data - that is why unbounded entries are forbidden.
- A flow requiring read-your-own-writes MUST NOT be cached unless invalidation completes before the response returns. TTL expiry is not a substitute for invalidation.
- A Redis failure MUST be treated as a miss. An unreachable cache degrades latency, never availability.
- Failures and empty results SHOULD NOT be cached, unless the negative result is itself expensive to compute and then only with a short TTL.

## 7. Data Classification

### Cache, events, and storage

- `Restricted` MUST NOT be written to Redis in any form, including inside a serialized DTO. ⚙ BE-60
- A classified value MUST NOT appear in a cache key in cleartext. Keys derive from ids, never from values.

## Testing

The required tests of the rules above. `../SKILL.md` §14 states what a required test is.

- **Cache**: the read-side decorator MUST have a test proving an unreachable Redis is served as a miss, not as a failure (BE-49), and every cached query a test that exercises the handler undecorated (BE-140).
