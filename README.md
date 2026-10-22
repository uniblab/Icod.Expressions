# Icod.Expressions

`Icod.Expressions` is the planned home for the GNU/POSIX regular-expression
engine currently in
[`Icod.CommandFramework`](https://github.com/uniblab/Icod.CommandFramework).
The repository now contains a mechanical copy of the frozen GNU/POSIX regex
engine and its tests. Its public types temporarily retain their original
`Icod.CommandFramework.RegularExpressions` and `Icod.CommandFramework.Text`
namespaces so the extraction can be proved without redesigning the API. This
is a `0.1.0-alpha.1` development assembly and is not yet packable.

## Extraction sequence

1. Review the in-place semantic audit and frozen A05 extraction baseline in
   [CommandFramework PR #16](https://github.com/uniblab/Icod.CommandFramework/pull/16).
2. Verify the frozen tests, API snapshot, and parity fixture already copied
   here with `sh tools/verify-frozen-corpus.sh`.
3. Verify the copied source blobs with `sh tools/verify-frozen-source.sh`, run
   the focused tests, and compare both engines against the same 44-case parity
   corpus and compiled API inventory.
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
dotnet test Icod.Expressions.sln -c Staging --no-build --no-restore
dotnet run --project tools/RegularExpressionParityBaseline/RegularExpressionParityBaseline.csproj \
  -c Staging -- --verify doc/Icod.Expressions-A04-Parity-Baseline.jsonl
```

The library targets `net8.0`, `net9.0`, and `net10.0` with C# 13; the focused
test assembly targets `net10.0`, matching the frozen source baseline. CI
builds all targets, runs the tests and parity verifier on Windows, Linux, and
macOS, and checks original Git blob identities on Linux. The package remains
disabled pending full E02 validation and consumer migration design.
