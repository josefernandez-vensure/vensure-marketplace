# Real-time annex - .NET stack law

Binds a tree once the tree meets its trigger: the first code that pushes messages to a client over a persistent connection - a SignalR hub, server-sent events, a raw WebSocket, or any other. Read it in full before writing or reviewing code in a tree that already meets the trigger, or in work that would make it meet it.

This annex is part of the .NET stack law, `../SKILL.md`, and uses its section numbers.

## 12. Real-time

- SignalR is a delivery channel, never a source of truth. Every state a client can receive over SignalR MUST be recoverable via REST.
- A hub method MUST map its argument to a message, dispatch it, and return. A hub MUST NOT reference any type other than DTOs and the dispatcher. ⚙ BE-74
- Hub authorization MUST use the same named policies as the equivalent HTTP operation. ⚙ BE-75
- Messages MUST carry DTOs. Domain types MUST NOT be serialized to clients. ⚙ BE-76
- Redis backplane for scale-out.

## Testing

No required test belongs to this annex alone. `../SKILL.md` §14 applies to a hub as to any other code.
