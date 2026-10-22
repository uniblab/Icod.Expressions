# Icod.Expressions

`Icod.Expressions` is the planned home for the GNU/POSIX regular-expression
engine currently in
[`Icod.CommandFramework`](https://github.com/uniblab/Icod.CommandFramework).
The repository has a buildable project shell, but **no regex implementation or
usable public API yet**. The project is intentionally not packable.

## Extraction sequence

1. Finish the in-place semantic audit and characterization in
   [CommandFramework PR #16](https://github.com/uniblab/Icod.CommandFramework/pull/16).
2. Freeze the source commit, public API, test corpus, reference revisions, and
   known discrepancies as the A05 baseline.
3. Copy the frozen tests and engine source into this repository, preserving
   observable behavior; compare both engines against the same parity corpus.
4. Validate consumer migration before proposing any removal or dependency
   switch in CommandFramework. That later change requires a separate PR.

The current evidence and remaining gates are in
[`doc/EXTRACTION-READINESS.md`](doc/EXTRACTION-READINESS.md). In particular,
the `tac` provider/option mismatch is recorded for a later bugfix in
[Icod.CoreUtils #190](https://github.com/uniblab/Icod.CoreUtils/issues/190).

## Build

Install the .NET 8, 9, and 10 SDKs or targeting packs, then run:

```sh
dotnet restore Icod.Expressions.sln
dotnet build Icod.Expressions.sln -c Staging --no-restore
```

The library targets `net8.0`, `net9.0`, and `net10.0` with C# 13. CI builds
all targets on Windows, Linux, and macOS. It does not claim semantic or
extraction parity until the frozen tests and production source arrive.
