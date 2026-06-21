# Contributing to UDP Relay

Thanks for your interest in improving UDP Relay! This is a small, focused project, so
the guidelines are short.

## Getting set up

- Install the [.NET 10 SDK](https://dotnet.microsoft.com/download) (the apps also target
  .NET 8 and 9; the SDK builds all three).
- The **Windows Service** is a .NET 10 Worker Service and builds with `dotnet` like the
  rest of the solution (`dotnet build UDP_Relay.sln`); it installs and runs on Windows,
  and can be run as a plain console for testing.

```bash
git clone https://github.com/jackwjensen/UDP_Relay.git
cd UDP_Relay
dotnet build UDP_Relay_Console/UDP_Relay_Console.csproj -c Release
dotnet test  UDP_Relay_Core.Tests/UDP_Relay_Core.Tests.csproj
```

## Before you open a pull request

1. **Tests pass** — `dotnet test` is green on every target framework.
2. **CI is green** — the GitHub Actions `Build` workflow builds all projects and runs
   the tests on Windows; it must pass.
3. **Docs updated** — if your change affects behaviour, setup, or configuration, update
   `README.md` and `CLAUDE.md` in the same change.
4. **Add a test** for bug fixes and new engine behaviour where practical (see the
   `UDP_Relay_Core.Tests` suite for examples, including the loopback relay tests).

## Conventions (please match the existing style)

- **Naming:** types and projects use underscores (`UDP_Relay`, `XML_Wrapper`,
  `UDP_Relay_Core`). This is deliberate here — match it, don't rename to PascalCase.
  Private fields are `_camelCase`.
- **Keep `UDP_Relay_Core` framework-agnostic** — it targets `netstandard2.0` so it stays
  consumable from any modern .NET host and, as a NuGet package, from .NET Framework too.
  Don't add modern-only or Windows-only APIs to Core.
- **Async** methods that await carry the `Async` suffix.
- **Nullable reference types** are enabled — don't introduce new nullable warnings.

## Reporting bugs / requesting features

Open an issue using the templates. For security issues, see [SECURITY.md](SECURITY.md)
— please report those privately, not as public issues.

---

Maintained by [Allegro IT ApS](https://allegroit.dk/) · questions: kontakt@allegroit.dk
