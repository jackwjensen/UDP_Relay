# CLAUDE.md — UDP_Relay

Project context for Claude Code sessions in this repository. Read this before making changes.

## What this is

A bidirectional UDP relay. It listens for UDP broadcast/unicast packets on a local network, forwards them to a remote endpoint, and relays the remote endpoint's replies back — so request/response UDP protocols keep working across network boundaries (routers, VPNs, cloud). Primary use case: sensors/alarms/IoT/legacy gear that only speak LAN-local UDP, with the consuming application on a different network.

Public, marketing-facing project — it is the company's most-visited GitHub page (used to promote **Allegro IT**, https://allegroit.dk/). Keep the README polished and accurate; it doubles as a credibility surface. Company contact in any user-facing copy is `kontakt@allegroit.dk` (never the owner-login `jack@` address).

Repo: https://github.com/jackwjensen/UDP_Relay · License: MIT © 2023 Jack W. Jensen.

## Architecture

```
UDP_Relay_Core (netstandard2.0)  ← the engine; no host-specific or Windows-only APIs
   ├── UDP_Relay.cs        relay engine: StartRelaying / StopRelaying / Send / Receive (IDisposable)
   ├── XML_Wrapper.cs      thin XmlDocument wrapper; XPath get/set, typed helpers (int, IPAddress, IPEndPoint)
   ├── Logger.cs           static fan-out logger (Console + any registered ILogger)
   └── WithCancellation.cs Task<T>.WithCancellation(token) — adds cancellation to UdpClient.ReceiveAsync()

Hosts (consume Core):
   ├── UDP_Relay_Console (net8.0;net9.0;net10.0)         cross-platform console host (multi-targeted)
   └── UDP_Relay_Service (.NET Framework 4.7.2, WinExe)  Windows Service host (Event Log + install .bat scripts)

Test harnesses (manual end-to-end, run by hand):
   ├── TestSender   (net8.0;net9.0;net10.0)  simulates a broadcasting device; sends 100 packets, prints replies
   └── TestReceiver (net8.0;net9.0;net10.0)  simulates the remote server; echoes whatever it receives

Automated tests:
   └── UDP_Relay_Core.Tests (net8.0;net9.0;net10.0)  xUnit suite for Core (XML_Wrapper, WithCancellation, loopback relay round-trip)
```

**Dependency direction:** hosts and harnesses → `UDP_Relay_Core`. Core depends on nothing host-specific. **Keep Core framework-agnostic** — do not add modern-only (net8/9/10) or Windows-only APIs to it, or you break the Windows Service (which is .NET Framework 4.7.2) consuming it.

### The relay model (important)

A relay is built from one-way forwarding tasks: `StartRelaying(localEndPoint, relayEndPoint)` spins up a task that listens on `localEndPoint` and forwards each datagram to `relayEndPoint`. The hosts call it **twice** to get bidirectional flow:

```
StartRelaying(ListeningClient, SendingServer);  // device → remote server
StartRelaying(ListeningServer, SendingClient);  // server reply → device
```

Default port layout (device side ↔ relay ↔ server side):
`device → :55530 (ListeningClient) → relay → server :55550 (SendingServer)`, then
`server → :55540 (ListeningServer) → relay → device :55520 (SendingClient)`.

## Configuration

Per-app XML files (`Settings_*.xml`) next to each executable, loaded via `XML_Wrapper` using XPath. They are marked `CopyToOutputDirectory` in the csproj — **if you add a setting, update the XML in the project root, not just the bin copy.** IPs/ports change without recompiling. `IP = 0.0.0.0` means listen on all interfaces; the sample send IP `192.168.1.255` is a subnet broadcast (set to the server's routable IP for real cross-network use). `TimeOut` is a socket timeout in ms (`0` = block forever).

## Build / run / test

SDK-style projects (Core, Console, TestSender, TestReceiver, tests) use the `dotnet` CLI. The executables and the test project **multi-target `net8.0;net9.0;net10.0`**, so `dotnet run` needs `-f` to pick a runtime:

```bash
dotnet build UDP_Relay_Console/UDP_Relay_Console.csproj -c Release
dotnet run  --project UDP_Relay_Console -f net10.0
dotnet run  --project TestReceiver -f net10.0     # start first
dotnet run  --project TestSender   -f net10.0     # then this; watch the echoes
dotnet test UDP_Relay_Core.Tests/UDP_Relay_Core.Tests.csproj   # runs on all three TFMs
```

The **Windows Service** project is classic .NET Framework (uses `packages.config` + `HintPath` into `packages/`). Build it in **Visual Studio** or with **MSBuild + `nuget restore` on Windows** — `dotnet build` does not handle its `packages.config`. Service lifecycle (run elevated, from the build output dir): `ServiceInstall.bat` → `ServiceStart.bat` → `ServiceStop.bat` → `ServiceUninstall.bat` (install/uninstall use `installutil.exe`).

Automated tests live in `UDP_Relay_Core.Tests` (xUnit, run with `dotnet test`); end-to-end checks can also be done by hand via the two harnesses. CI: `.github/workflows/build.yml` builds every project and runs the tests on `windows-latest` for each push/PR (dotnet for the modern projects, MSBuild + NuGet for the Service).

## Logging

`UDP_Relay_Core.Logger` is a static class: every `Log*` call writes to `Console` and fans out to each `ILogger` registered with `Logger.AddLogger(...)`. Hosts wire up sinks at startup via `Microsoft.Extensions.Logging` (Console + Debug everywhere; rolling `.log` files in a `Logs/` subfolder via `NetEscapades.Extensions.Logging.RollingFile` (configured with `options.Extension = "log"`); Windows Event Log in the Service). Use message templates and the existing `Log` / `LogDebug` / `LogTrace` / `Log(ex)` overloads.

## Conventions & gotchas

- **Naming:** types and projects use underscores (`UDP_Relay`, `XML_Wrapper`, `UDP_Relay_Core`). This is non-idiomatic C# but it is the established style here — **match it; don't rename to PascalCase.** Private fields are `_camelCase`.
- **Nullable:** enabled in `UDP_Relay_Console` and `UDP_Relay_Core.Tests`. Core, Service, and the harnesses do not have `<Nullable>enable</Nullable>` — don't assume nullable-aware contracts there.
- **Async:** methods that await carry the `Async` suffix (`RelayAsync`). `WithCancellation` is an extension method (suffix not expected).
- **Target frameworks are mixed on purpose** (Core `netstandard2.0`; Console/harnesses/tests multi-target `net8.0;net9.0;net10.0`; Service `net4.7.2`). .NET 10 is the primary LTS target; 8 and 9 are kept for compatibility. The framework list lives once in `Directory.Build.props` as `$(UdpRelayAppTargetFrameworks)`; the four multi-targeted projects reference it, so change it in one place. `Directory.Build.props` also holds shared assembly/package metadata (Company = Allegro IT, repo URL, etc.) and is imported by every project including the classic Service (SDK-only props there are simply ignored).
- **`Logger` writes to `Console` directly** (in addition to any registered `ILogger`s), even when Core is used as a library. Harmless for the hosts; keep in mind if embedding Core elsewhere.
- **The relay tolerates destination-unreachable resets — keep it that way.** `RelayAsync` catches `SocketError.ConnectionReset` (Windows `WSAECONNRESET`) / `ConnectionRefused` (Linux) and keeps relaying. Without it, a momentarily-down endpoint permanently kills that relay direction until the process restarts (regression-tested by `Relay_survives_send_to_unreachable_destination` in `UDP_Relay_Core.Tests`).

## Working agreements

- Follow the global commit/documentation rules in the user's `~/.claude/CLAUDE.md`: never commit/push without an explicit instruction (each is a separate gate), and update docs (this file + `README.md`) in the **same** commit as the code they describe.
- Prefer the correct fix over the quick one; keep Core dependency-light and host-agnostic.
