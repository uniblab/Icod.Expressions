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

On Linux/glibc, run the additional C-locale ASCII differential:

```sh
dotnet run --project tools/PosixRegexDifferential/PosixRegexDifferential.csproj -c Staging
```

The library, focused tests, and parity runner target `net8.0`, `net9.0`, and
`net10.0` with C# 13. CI requires 214 focused tests: the original 193-test
copy-time corpus, five consumer-shaped cases, and 16 A02 syntax-option cases.
It also checks the original 44-case parity fixture on each framework across
Windows, Linux, and macOS. The copied production files and original tests
retain their pinned Git blobs. Run `sh tools/verify-a05-candidate.sh` to check
the consolidated source, corpus, and oracle-tool identities.

To inspect a local, unpublished package and its independent consumer:

```sh
dotnet pack src/Icod.Expressions/Icod.Expressions.csproj -c Staging -o artifacts
dotnet restore samples/PackageSmoke/PackageSmoke.csproj --source artifacts
dotnet run --project samples/PackageSmoke/PackageSmoke.csproj -c Staging -f net10.0
```

With PowerShell and .NET 10, the old-client forwarding check compiles against
the published CommandFramework 2.2.1 package, then checks all 26 forwarded
public identities and representative matches through the unpublished local
Expressions package. CI runs it on Windows, macOS, and Linux:

```powershell
./tools/ForwardingSmoke/run.ps1 -PackageFeed artifacts
```

The cross-platform consumer probe runs 725 old-package-compiled cases:
611 regex consumers from CoreUtils (174), ProcPs (24), DiffUtils (62), Grep
(113), and LineEditor (238), plus 114 CoreUtils shared-Text cases including
one direct Shared Text API integration case. It runs the original binaries
before and after replacing the CommandFramework DLL with a temporary full
bridge backed by the local package. CI runs it on Windows, macOS, and Linux:

```powershell
./tools/ConsumerBridgeProbe/run-cross-platform.ps1 -PackageFeed artifacts
```

That probe also packs an unpublished scratch CommandFramework bridge and
restores a fresh client with only the bridge package reference. It verifies
that NuGet supplies Expressions transitively and that forwarded regex and
Text enums work with the retained `TextUnitReader`. It then loads an old
client compiled solely against published CommandFramework 2.2.1 from the
package-backed output and checks all 26 forwarded types and representative
matches without copying the published CommandFramework DLL.

On Linux with .NET 10, the source-pinned consumer probe also uses that
unpublished package. It fetches six pinned public source commits, builds a
temporary forwarding bridge, and runs the original CoreUtils, ProcPs,
DiffUtils, Grep, and LineEditor command tests against both the published
CommandFramework dependencies and scratch bridge:

```sh
sh tools/ConsumerBridgeProbe/run.sh artifacts
```

The Grep suite needs Unix-domain socket creation for a device-policy test.
On a local environment where that is prohibited, set
`GREP_TEST_FILTER='FullyQualifiedName!=Icod.Grep.Tests.CommandTests.RecursiveDeviceDefaultsMatchGnuPolicy'`
for the probe. CI leaves the filter unset and runs the complete suite.

The bridge DLL and package exist only in disposable probe outputs; neither is shipped.

## COMPATIBILITY AND LIMITS

The [extraction readiness record](doc/EXTRACTION-READINESS.md) and
[A05 final candidate](doc/Icod.Expressions-A05-Final-Candidate.md) link the
copy-time source, API, tests, fixtures, reference revisions, and known gaps.
Bounded E02 parity over the final A05 corpus passed. Formal A05 acceptance
remains open pending review of the classified limits.
The inherited providers are not separate strict POSIX BRE/ERE contracts or
complete implementations of all Gnulib syntax bits. In particular,
multi-scalar collating elements are unsupported; ERE middle anchors diverge
from POSIX Issue 8 §9.4.9, and pattern-newline alternation is not a selectable
GNU syntax control. The probed empty-anchor captures agree with the included
pinned Gnulib engine. Those inherited outcomes are preserved during this
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
