# Security Policy

## Supported versions

UDP Relay is maintained on the `main` branch; fixes are applied there and flow into
the next tagged release. Please test against the latest `main` before reporting.

## Reporting a vulnerability

**Please do not open a public GitHub issue for security vulnerabilities.**

Instead, report them privately to **[kontakt@allegroit.dk](mailto:kontakt@allegroit.dk)**
(or use GitHub's [private vulnerability reporting](https://docs.github.com/en/code-security/security-advisories/guidance-on-reporting-and-writing-information-about-vulnerabilities/privately-reporting-a-security-vulnerability)
on this repository).

Please include:

- A description of the issue and its impact.
- Steps to reproduce (or a proof of concept).
- The affected host (Console or Windows Service) and OS / .NET version.

We aim to acknowledge reports within a few business days and will keep you updated
as we work on a fix. Thank you for helping keep the project and its users safe.

## Scope notes

UDP Relay forwards arbitrary UDP datagrams between endpoints you configure. When
deploying on an untrusted network, bind the listeners to specific interfaces (set the
`*IP` values in the `Settings_*.xml` to a specific address rather than `0.0.0.0`) and
restrict who can reach the relay's ports with your firewall.

---

Maintained by [Allegro IT ApS](https://allegroit.dk/).
