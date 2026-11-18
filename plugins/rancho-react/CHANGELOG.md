# Changelog

## 3.0.1

- **The skill's description loads.** The frontmatter `description` held an
  unquoted `: ` (in `Stack law: /rancho-react:law`), so YAML failed to parse it and
  the skill loaded with no description. It is now quoted. No upgrade step.

## 3.0.0

- **Breaking.** Rewritten to hold only what the `rancho-react` template's checks and reference code can't show, for a micro-frontend in the PrismOne shell on the organization's stack. Routing moves from TanStack Router to React Router, which Prismatic and the IAM library are built on; npm replaces pnpm. The real-time annex is gone: real-time push is "stop and ask".

Earlier versions are under the `law-archive-4.1` tag.
