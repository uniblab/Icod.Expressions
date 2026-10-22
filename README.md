# Icod.Expressions

## NAME

`Icod.Expressions` — managed GNU-style regular expressions for .NET strings and
authoritative byte input.

## STATUS

This repository contains the semantics-neutral extraction of the regex engine
from [`Icod.CommandFramework`](https://github.com/uniblab/Icod.CommandFramework).
The current `0.1.0-alpha.1` package is a **local development candidate**; it
has not been published. The copied public types retain their original
`Icod.CommandFramework.RegularExpressions` and `Icod.CommandFramework.Text`
namespaces while extraction parity and the later compatibility bridge are
reviewed. Do not reference both assemblies' duplicate type names in one
consumer without an explicit migration plan.

The existing CommandFramework implementation remains in place. This PR does
not remove it, redirect consumers, or fix its known behavior differences.

## SYNOPSIS

```csharp
using Icod.CommandFramework.RegularExpressions;

var provider = new GnuExtendedRegularExpressionProvider(
    PosixCLocaleRegularExpressionCharacterClassProvider.Instance
);
var compiled = provider.Compile("(a|aa)");
if (!compiled.IsSuccess) {
    Console.Error.WriteLine(compiled.Diagnostic?.Code);
    return;
}

var result = compiled.Expression!.Match("zaa");
if (result.IsSuccess && result.IsMatch) {
    Console.WriteLine($"{result.Match!.Index}: {result.Match.Value}"); // 1: aa
}
```

The [package-only sample](https://github.com/uniblab/Icod.Expressions/blob/main/samples/PackageSmoke/Program.cs) also checks source
byte offsets. Its project references the locally built NuGet package, not the
library project or CommandFramework.

## DESCRIPTION

The three providers compile GNU Basic, Extended, and Emacs-style operator
profiles. The engine selects a leftmost-longest whole match, returns numbered
captures, and provides controlled compile and match diagnostics. Providers
accept an injectable character-class/collation policy; the default uses the
current .NET culture, while `PosixCLocaleRegularExpressionCharacterClassProvider.Instance`
selects the built-in invariant C-locale policy.

`ICompiledRegularExpression.Match(string)` returns UTF-16 positions. Its
`ReadOnlyMemory<byte>` overload accepts byte-valued input or explicitly
decoded UTF-8 and returns authoritative source-byte positions. Set
`RegularExpressionInputOptions.DecodingMode` and `InvalidEncodingPolicy` to
choose decoding and malformed-input behavior. Prepared byte input supports
repeated searches without changing the source-byte coordinate contract.

`RegularExpressionOptions` controls case comparison, newline sensitivity,
line separator, NUL matching, compatibility handling of invalid repetition
and empty ranges, and nesting/match-state limits. The compile provider
selects the grammar: passing an option record whose `Syntax` names another
grammar does not override that provider.

## RESULTS

Check `IsSuccess` before using a compile or match result. A successful
no-match has `IsSuccess == true`, `IsMatch == false`, and a null `Match`;
controlled failures carry a `RegularExpressionDiagnostic`. String captures
use UTF-16 indices and byte captures use source-byte indices. Cancellation
tokens are accepted by compile and match operations; the async overloads
do not offload synchronous matching to a thread pool.

## BUILD AND TEST

Install the .NET 8, 9, and 10 SDKs or targeting packs and runtimes:

```sh
dotnet restore Icod.Expressions.sln
dotnet build Icod.Expressions.sln -c Staging --no-restore
dotnet test Icod.Expressions.sln -c Staging --no-build --no-restore
dotnet run --project tools/RegularExpressionParityBaseline/RegularExpressionParityBaseline.csproj \
  -c Staging -f net10.0 -- --verify doc/Icod.Expressions-A04-Parity-Baseline.jsonl
sh tools/verify-frozen-corpus.sh
sh tools/verify-frozen-source.sh
```

The library, focused tests, and parity runner target `net8.0`, `net9.0`, and
`net10.0` with C# 13. CI requires all 193 frozen tests and the 44-case parity
fixture on each framework across Windows, Linux, and macOS. The copied
production files and frozen tests are checked against their A05 Git blobs.

To inspect a local, unpublished package and its independent consumer:

```sh
dotnet pack src/Icod.Expressions/Icod.Expressions.csproj -c Staging -o artifacts
dotnet restore samples/PackageSmoke/PackageSmoke.csproj --source artifacts
dotnet run --project samples/PackageSmoke/PackageSmoke.csproj -c Staging -f net10.0
```

## COMPATIBILITY AND LIMITS

The [extraction readiness record](https://github.com/uniblab/Icod.Expressions/blob/main/doc/EXTRACTION-READINESS.md) links the
frozen source, API, tests, fixtures, reference revisions, and known gaps.
The inherited providers are not separate strict POSIX BRE/ERE contracts or
complete implementations of all Gnulib syntax bits. In particular,
multi-scalar collating elements are unsupported; contextual anchors,
pattern-newline alternation, and an empty-anchor capture choice differ from
specific GNU reference behavior. Those outcomes are preserved during this
mechanical move. The `tac` consumer's provider/options mismatch is tracked
separately in [Icod.CoreUtils #190](https://github.com/uniblab/Icod.CoreUtils/issues/190).

Consumer migration and any removal of CommandFramework's engine require a
separate review under the
[compatibility bridge plan](https://github.com/uniblab/Icod.Expressions/blob/main/doc/E03-Compatibility-Bridge.md).
No consumer should silently fall back to `System.Text.RegularExpressions` for the
GNU/POSIX calls owned by this engine.

## LICENSE

LGPL-3.0-or-later, as recorded in the source project's package metadata.
The repository includes the
[license text](https://github.com/uniblab/Icod.Expressions/blob/main/LICENSE);
source and test provenance is recorded in CommandFramework's A00/A05 audit.
