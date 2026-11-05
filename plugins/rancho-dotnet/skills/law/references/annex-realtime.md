# Real-time annex - .NET stack law

Binds a tree once the tree meets its trigger: the first code that pushes messages to a client over a persistent connection - a SignalR hub, server-sent events, a raw WebSocket, or any other. Read it in full before writing or reviewing code in a tree that already meets the trigger, or in work that would make it meet it.

This annex is part of the .NET stack law, `../SKILL.md`, and uses its section numbers.

## 12. Real-time

- SignalR is a delivery channel, never a source of truth. Every state a client can receive over SignalR MUST be recoverable via REST.
- A hub method MUST map its argument to a message, dispatch it, and return. A hub MUST NOT reference any type other than its DTOs, the Application message it dispatches, and every type those are composed of; the hub base type; the dispatcher and the `Result` it returns; and the framework's primitives, such as `Task`, `CancellationToken` and the caller context. ⚙ BE-74
- Hub authorization MUST use the same named policies as the equivalent HTTP operation. ⚙ BE-75
- SignalR caches a connection's principal for the connection's lifetime and never revalidates it. The hub MUST close a connection when its authentication expires - `CloseOnAuthenticationExpiration` for bearer tokens - and every connection MUST have a bounded lifetime, no longer than the access token's, or the external authority's cache lifetime (§6) where one answers for permissions.
- Every hub invocation passes a rate limit applied by a hub filter, keyed on the acting principal: the HTTP rate limiter never sees one. A rejected invocation fails with a `HubException` naming the limit.
- A server push goes only to groups the server assigned from the principal's own permissions; a client never names the group it joins.
- Messages MUST carry DTOs. Domain types MUST NOT be serialized to clients. ⚙ BE-76
- Redis backplane for scale-out.

## Testing

The required tests of the rules above. `../SKILL.md` §14 states what a required test is, and applies to a hub as to any other code.

- **Real-time**: every hub MUST have a test proving a connection whose authentication has expired is closed, and that a client cannot join a group the server did not assign it. (BE-121)
