# Frontend

The storefront is a Next.js 16 App Router application written in React 19 and TypeScript, styled
with Tailwind CSS v4 and shadcn/ui. Bun is both the runtime and the package manager. The `dev`,
`build` and `start` scripts call `bun --bun next`, which runs Next.js on Bun instead of Node.js.

Commands run from `frontend/`. The [root README](../README.md) covers starting the app and running
the tests.

## Generating the API client

The API client's types live in `src/lib/api/schema.d.ts`, generated from the API's OpenAPI
document. Codegen reads the API's address from `.env.local`, which you create from the tracked
example:

```bash
cp .env.example .env.local
```

With the app running, regenerate the types:

```bash
bun run codegen
```

Commit the regenerated file, then run the type check below to find anything the change broke.

## Checking types

`bun run typecheck` runs `next typegen`, which generates the route types without a full build,
followed by `tsc --noEmit`:

```bash
bun run typecheck
```

CI runs the same script.

## Lint and formatting

`bun run lint` runs ESLint, and `bun run format` runs Prettier, which also sorts Tailwind classes:

```bash
bun run lint
bun run format
```

CI runs `bun run lint` and `bun run format:check`, and fails if either reports a problem.

## Filtering test runs

Arguments after a script name pass straight through to its test runner. To run one component test
file:

```bash
bun run test src/components/header.test.tsx
```

To run one end-to-end spec:

```bash
bun run test:e2e e2e/search.spec.ts
```

## End-to-end stack

When the storefront is already running on port 3000, Playwright tests against it. Otherwise it
starts the whole stack through Aspire, then removes the SQL Server and Meilisearch containers once
the run finishes.

To open the report from the last run:

```bash
bun run test:e2e:report
```

On Windows, `test:e2e:stop` stops an AppHost or containers left behind by an interrupted run:

```bash
bun run test:e2e:stop
```

## Installing packages

CI installs with `bun install --frozen-lockfile`, which fails when `bun.lock` is out of date.
Commit it with any package change.

`bunfig.toml` holds back any version published in the last three days, except for `@types/bun` and
`typescript`. Socket's security scanner also checks each package before it installs, and cancels
the install on a fatal finding.

## README screenshots

With the app running, this recaptures the five images in the repository's `docs/images`:

```bash
bun run screenshots
```

On its way to the checkout page, the script adds two albums to the cart and signs in with the demo
account. It captures `http://localhost:3000` unless `SCREENSHOT_BASE_URL` is set.
