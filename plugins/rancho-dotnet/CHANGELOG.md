# Changelog

## 4.1.0

- **Several aggregates in one transaction, through a domain service.**
  §5 no longer says a command changes exactly one aggregate. A rule over
  several aggregates of the module, such as a transfer or a bulk
  correction, is a domain service the handler calls: it loads, changes
  and saves them, and they commit together. §3 already placed a rule
  spanning aggregates in `Domain` as a domain service, so §5 contradicted
  it. Dropped "If two aggregates must always change together, they are
  one aggregate: redraw the boundary". Across modules nothing changed: a
  command can't reach another module's aggregates, so modules still meet
  only through messages and sagas.
- **Upgrade:** none for a tree that changes one aggregate per command.
  In the `rancho-dotnet` template, a domain service implements
  `IDomainService`, and its command is an `IMultiAggregateCommand`: it
  carries no `If-Match`, each aggregate's version is checked at commit,
  and a lost race answers 409. `RenameProducts` is the example.

## 4.0.0

- **Breaking.** The modular .NET law (formerly `rancho-modular-dotnet`) replaces the 3.x law, rewritten to hold only what the `rancho-dotnet` template's checks and reference code can't show. The annexes are gone; caching, model calls, real-time push and uploads are "stop and ask". Bind trees with `Stack law: /rancho-dotnet:law`.

Earlier versions are under the `law-archive-4.1` tag.
