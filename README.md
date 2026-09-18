<h1 align="center">EvenSteven</h1>

<p align="center">
  Split bills, keep the friendship.<br>
  Create a room — share the link — see who owes what.
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white" alt=".NET 10">
  <img src="https://img.shields.io/badge/C%23-14-239120?logo=csharp&logoColor=white" alt="C# 14">
  <img src="https://img.shields.io/badge/Blazor-WASM-5C2D91?logo=blazor&logoColor=white" alt="Blazor WASM">
  <img src="https://img.shields.io/badge/Dapper-1C1E24?logo=databricks&logoColor=white" alt="Dapper">
  <img src="https://img.shields.io/badge/SQLite-003B57?logo=sqlite&logoColor=white" alt="SQLite">
  <img src="https://img.shields.io/badge/SignalR-0078D4?logo=signalr&logoColor=white" alt="SignalR">
  <img src="https://img.shields.io/badge/.NET_MAUI-512BD4?logo=dotnet&logoColor=white" alt=".NET MAUI">
  <img src="https://img.shields.io/badge/License-Elastic%20License%202.0-yellow.svg" alt="Elastic License 2.0">
  <img src="https://img.shields.io/github/actions/workflow/status/evensteven-app/evensteven-app/ci.yml?label=CI" alt="CI">
</p>

---

## No spreadsheets. No group chats.

**EvenSteven** calculates who owes whom — instantly. No sign-ups, no installs, no ads. Open your browser, and in 10 seconds your room is ready. Works on phone and desktop, with live sync across all participants.

> Between friends, you don't need lawyers or receipts. A handshake and a clear calculator are enough. **EvenSteven**: honest math for honest friendships.

---

## How it works

**1. Create a room**

Click the button on the home page — and get three things:
- **Public link** — share in your group chat. Everyone can view balances, but no one can edit.
- **Editor link** — keep this for yourself. Only with this link can you manage participants and room settings.
- **Room password** — enable if you need protection from unwanted changes.

**2. Add people**

Enter participant names. EvenSteven generates a personal key for each. Send it via private message — and never wonder again who paid for what.

**3. Split expenses and track balances in real time**

- Log a single payment: "Alex paid $20 for Uber".
- Split among many: "Dinner $150 — split evenly among selected".
- Watch balances update live. Two people adding expenses from different phones? Data syncs instantly.

---

## Features

- **Link-based rooms.** No registration. The URL is your access key.
- **Three access levels.** Public link (view-only), editor link (manage room), personal key (add your own expenses without touching others').
- **Live sync.** SignalR updates expenses across all open tabs — no page reload needed.
- **Dark mode.** Toggle in one click, saved in browser. Applies instantly — no white flash on load.
- **History & undo.** Made a mistake? Revert any action in one click — EvenSteven recalculates balances automatically.
- **Split preview.** See exactly how much each selected person owes before confirming the split.
- **Password protection.** Lock a room to prevent accidental deletions or setting changes.
- **Advances & deposits.** Participants can contribute money via personal links before the final settlement.

---

## Roadmap

| Version | Status | What's inside |
|---|---|---|
| **v1.0** | In development | Rooms, participants, individual & group expenses, history, undo, dark mode, SignalR |
| **v1.1** | In development | Deposit system, "paid vs consumed" balance, "who owes whom" settlement, personal keys |
| **v2.0** | Planned | Trip Mode: track days, per-day splitting, weight coefficients, expense categories |
| **v3.0** | Planned | .NET MAUI mobile app, push notifications, offline cache |
| **v3.1** | Planned | Receipt OCR, CSV/PDF export, room archiving |

---

## Development

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- A SQLite provider is built in — no extra setup needed.

### Getting started

1. Clone the repository.

2. Configure the database connection. The repository ships with a placeholder, so create a local development config that points to a SQLite file:

   ```json
   // src/EvenSteven.Api/appsettings.Development.json
   {
     "ConnectionStrings": {
       "db-connection": "Data Source=../../../../application.db;"
     }
   }
   ```

   *This path is relative to the working directory. When you run the API from the repository root, the database is created there as `application.db`.*

3. Run the API:

   ```bash
   dotnet run --project src/EvenSteven.Api
   ```

   Migrations (FluentMigrator) apply automatically on startup and create the `application.db` files if missing.

4. Run the web client (separate terminal):

   ```bash
   dotnet run --project src/EvenSteven.BlazorWeb
   ```

> Never commit `appsettings.*.json` with real secrets — production connection strings and keys live in environment variables or local-only `appsettings.Production.json`.

### Tests

The test project ([`tests/EvenSteven.RepositoryTests`](tests/EvenSteven.RepositoryTests)) uses xUnit v3 on the **Microsoft.Testing.Platform**. Each test run creates its own throwaway SQLite database inside the project's `Test Databases` folder, so no setup is required.

Run the tests with:

```bash
dotnet test --project tests/EvenSteven.RepositoryTests
```

### Commit messages

We follow [Conventional Commits](https://www.conventionalcommits.org/) — a short imperative subject prefixed with a type, plus an optional scope.

Examples:

- `feat: add room expenses API`
- `feat(room): add participant keys`
- `fix: reset room balance on undo`
- `docs: update ADR-001`
- `refactor: extract BruteForceGuard`
- `test: cover settlement calculation`

Rules: one logical change per commit, subject under 72 characters, imperative mood; reference the issue/PR in the body when relevant (`closes #12`).

---

## Documentation

- [ADR-001: Hybrid architecture](docs/001-hybrid-architecture.md) — project layout, ideas, tooling.
- [ADR-002: Room access & authorization](docs/002-room-access-and-authorization.md) — keys, password, brute-force protection.

> Premium / subscription features ship in a separate closed-source core (`EvenSteven.Core`) and are **not** part of this repository. The public build runs with safe defaults (access gate stub).

---

## Security

Found a vulnerability? See [SECURITY.md](SECURITY.md) for our responsible-disclosure process.

---

## License

Distributed under the Elastic License 2.0. In short: you may use, modify and share the code, but you may **not** offer it to third parties as a hosted service. See [LICENSE](LICENSE) for the full terms.

<p align="center">
  <a href="https://github.com/evensteven-app/evensteven-app/issues">Found a bug?</a> ·
  <a href="https://github.com/evensteven-app/evensteven-app/discussions">Ideas & discussions</a>
</p>