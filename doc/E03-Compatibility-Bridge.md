# E03 compatibility bridge and consumer migration

**Status:** bounded E02 extraction parity and the three-platform 725-case
scratch bridge gate passed; no CommandFramework production source or consumer
dependency has changed. The alpha package can be built and used
independently, but its copied public types still have the legacy
`Icod.CommandFramework.RegularExpressions` and `.Text` full names.

## Boundary that needs a bridge

The A05 compiled API has 24 top-level regex types and two public Text enums.
The enums are used by `RegularExpressionInputOptions` and also by
CommandFramework's non-regex `ITextLocaleProvider`, `TextUnitReader`,
`PosixCLocaleProvider`, and `UnicodeTextLocaleProvider`. A future removal of
the original regex source must therefore account for both enum identities
and for non-regex Text APIs. Referencing both assemblies while both define
the same full type names can produce ambiguity and incompatible type
identities; adding a second package reference alone is not a migration.

The new package has no dependency on CommandFramework. Its package-only
sample exercises the old full names from `Icod.Expressions.dll` and checks
string leftmost-longest selection and authoritative byte offsets. This
establishes the direction needed for a later one-way bridge:

```text
Icod.CommandFramework  ->  Icod.Expressions
       forwarding             implementation
```

## Isolated binary-forwarding feasibility probe

On Linux with .NET SDK 10.0.402, an out-of-repository probe compiled a client
against the unchanged `Icod.CommandFramework` 2.2.1.0 assembly. A separate
test assembly named `Icod.CommandFramework` at version 2.2.1.0 compiled with
`TypeForwardedTo` attributes for all 24 public regex types and both shared
Text enums. With this test assembly and `Icod.Expressions` 0.1.0.0 in the
runtime dependency graph, the client loaded, matched UTF-8 bytes through the
forwarded provider, returned byte offset 2 and length 2, and reported that
the provider and `TextDecodingMode` came from `Icod.Expressions`. Before the
forwarder/dependency setup, the client failed to load the old assembly.

This is a feasibility result for the direction and two exercised type
identities, not a CommandFramework bridge. The test assembly omitted all
non-regex CommandFramework APIs, and it did not load the five real consumers
or verify every forwarded type at runtime. A production bridge requires its
own old-client compile/load regression in the separate CommandFramework PR.

## Cross-platform old-client forwarding gate

`tools/ForwardingSmoke/run.ps1` builds an old client against the published
CommandFramework 2.2.1 package. It also builds a thin assembly with the old
name and version that forwards the 24 regex types and both shared Text enums
to the **unpublished local Expressions package**. The two projects restore
the candidate from an isolated NuGet cache. The script runs the unchanged
client with the published DLL first, substitutes the thin DLL in a separate
temporary output, and requires all 26 old-compiled type references to resolve
from `Icod.Expressions`. It checks leftmost-longest string matching, a
capture, source-byte offsets, and that removing the new DLL makes the
forwarded client fail to load it. The Staging CI jobs run this check on
Windows, macOS, and Linux; locally, after packing the candidate, run:

```powershell
./tools/ForwardingSmoke/run.ps1 -PackageFeed artifacts
```

This thin DLL cannot replace CommandFramework for consumers of its other
APIs. The Linux package-backed gate below tests those consumers with a
temporary bridge built from the original non-regex source. A production
bridge still needs a separate reviewed CommandFramework PR and the complete
consumer gate.

## Cross-platform pinned consumer gate

`tools/ConsumerBridgeProbe/run-cross-platform.ps1` fetches the same pinned
CommandFramework and five consumer source commits used by the Linux gate
below. It builds unchanged CoreUtils regex callers (174), ProcPs (24),
DiffUtils (62), Grep (113), and LineEditor (238) tests against their published
CommandFramework 2.2.1 or 2.1.0 dependencies. A separate 114 CoreUtils cases
cover `cut` (28), `expand` (18), `unexpand` (20), `fold` (18), `fmt` (29), and
one CoreUtils Shared integration case. The commands use the two shared Text
enum identities forwarded from CommandFramework into Expressions, even when
they do not call regex. The Shared case passes both enums directly into the
bridge-owned `TextUnitReader` and checks the resulting UTF-8 bytes. The gate
requires all 725 original passes.
It builds the **full scratch bridge** from CommandFramework's non-regex
source and the local Expressions package. An old-compiled assembly-origin
probe must reject the original regex identity and accept the forwarded
provider and Text enums with `CommandContext` and `TextUnitReader` still in
the bridge. The original test binaries then run in separate outputs with the
bridge and candidate package; all 725 must pass again. The 114 added cases
must retain identical test names and outcomes across the original and bridge
runs. [CI run 40021465552](https://github.com/uniblab/Icod.Expressions/actions/runs/40021465552)
passed on Windows, macOS, and Linux. After packing the candidate, run with
PowerShell:

```powershell
./tools/ConsumerBridgeProbe/run-cross-platform.ps1 -PackageFeed artifacts
```

The same gate also packs a disposable `Icod.CommandFramework`
`2.2.1-expressions-bridge.1` from the pinned non-regex source. A fresh
`PackagedClient` references **only** that bridge package, restores
`Icod.Expressions` transitively from the local feed, and checks a regex match
and `TextUnitReader` with both forwarded Text enums. This checks the NuGet
dependency graph in addition to the old-binary substitution above. The
package remains an unpublished scratch artifact; the CommandFramework
repository and published packages are unchanged.

This validates the pinned original test corpus across three platforms. The
tests do not exhaust every regex option or Text API caller, and the scratch
bridge is not ready for distribution. Other standalone CoreUtils Shared Text
API cases remain outside this 725-case gate. The direct
`diff -I` correction remains deferred to
[DiffUtils #8](https://github.com/uniblab/Icod.DiffUtils/issues/8).

## Source-pinned CoreUtils `expr` command probe

A second, still isolated Linux/.NET 10 probe used
[`Icod.CoreUtils` commit `2c47cd67bae490dedf16234d8460c8b9da8d3404`](https://github.com/uniblab/Icod.CoreUtils/tree/2c47cd67bae490dedf16234d8460c8b9da8d3404).
Its unmodified `expr` project and 50 command tests were compiled against the
published CommandFramework 2.2.1 package. The ordinary test run passed 50/50.
In a separate scratch output directory, the compiled binaries were run with
a temporary `Icod.CommandFramework` 2.2.1.0 bridge assembled from the
unchanged CommandFramework non-regex source. The bridge excludes all
`src/RegularExpressions/*.cs` and the original `InvalidEncodingPolicy` and
`TextDecodingMode` declarations, forwards the 24 public regex types and two
enums, and references the local `Icod.Expressions` assembly. It compiled with
zero warnings or errors. No repository source, package, or consumer project
was edited to make the bridge.

All 50 original `expr` tests passed again under this replacement. They include
actual command invocation, first-capture and null-capture output, anchored
match length, invalid-pattern status, and the `match` keyword. A scratch-only
test compiled against the original package checked that the forwarded
`GnuBasicRegularExpressionProvider` resolves from the copied
`Icod.Expressions.dll` in the test output while `CommandContext` remains in
`Icod.CommandFramework.dll`; the combined run passed 51/51. That extra test
failed against the original package as intended. Removing the Expressions
DLL from the bridged output made the extra test fail with
`FileNotFoundException`, confirming the copy was exercised rather than the
old regex implementation. The test sources in CoreUtils were not committed.

That one-off probe used a direct project reference to the extracted assembly
in the temporary bridge and a published package reference in the original
consumer. By itself it was **not** a package-backed or CI gate, nor proof for
other consumers, frameworks, platforms, and all forwarded API members. Do
not merge a CommandFramework bridge based solely on this result.

## Reproducible package-backed consumer gate

`tools/ConsumerBridgeProbe/run.sh` makes the next check repeatable on Linux
with .NET 10. After packing the unpublished `0.1.0-alpha.1` candidate into
`artifacts`, run:

```sh
sh tools/ConsumerBridgeProbe/run.sh artifacts
```

The script fetches the exact CommandFramework source commit
`e1e118f18246eed290bee4758325a20e302bfb92`, CoreUtils
`2c47cd67bae490dedf16234d8460c8b9da8d3404`, ProcPs
`e802ac03d941b1ba9fbe30e8f9c68fc7efcf3cff`, DiffUtils
`8192ec43e1f6add42bc759b225c814861e18c843`, Grep
`2ef1c11f11b77eb876242eb0c92a88d20857db64`, and LineEditor
`1705e1dc5107eb2ede6b5ecec5dad7ea18c93061` into temporary directories.
It builds and runs the unchanged CoreUtils `expr` tests against the published
CommandFramework 2.2.1 package (50 cases). It then uses an isolated NuGet
cache for the candidate package, so an earlier alpha with the same version
cannot satisfy the bridge restore. The script builds a throwaway
CommandFramework 2.2.1.0 assembly from the pinned non-regex source and 26
type forwards, resolving `Icod.Expressions` from the **local NuGet package**.
An old-package-compiled probe must reject the unbridged type identity, then
report the forwarded provider from the package and `CommandContext` from the
temporary bridge. The original 50 consumer tests run again with those
assemblies substituted in a temporary test output. ProcPs's unchanged
`pgrep`, `pkill`, and `pidwait` projects compile against their published
CommandFramework 2.1.0 dependency, and their 11, 7, and 6 original command
tests run first with that package and then with the same candidate package
and temporary 2.2.1.0 forwarding assembly substituted in separate outputs.
They cover GNU Extended alternation, process matching and no-match paths;
they do not exhaust regex options or diagnostics. DiffUtils's unchanged
`diff` and `sdiff` projects compile against published CommandFramework 2.1.0;
their 22 and 40 original command tests run with that package and then with
the candidate package and scratch bridge. The `sdiff` suite has two `-I`
ignored-line regex cases; the `diff` suite checks `-F` function-context
matching but has no direct `-I` ignored-line case. That consumer follow-up is
tracked in [DiffUtils #8](https://github.com/uniblab/Icod.DiffUtils/issues/8).
The copied regex source and original consumer binaries are not edited.

Grep's unchanged test project compiles against published CommandFramework
2.2.1. It exercises GNU Basic/Extended byte matching, source-byte offsets,
and prepared-input command paths. The Linux CI gate runs the full original
suite against the published package and the scratch bridge. One device-policy
test creates a Unix-domain socket, which this local workspace prohibits. For
local verification, `GREP_TEST_FILTER` may exclude that one test in both runs;
the remaining 112 tests passed with the published dependency and bridge.
[CI run 39888672536](https://github.com/uniblab/Icod.Expressions/actions/runs/39888672536)
left the filter unset and passed all 113 Grep cases, including the socket
test, before and after the scratch bridge substitution.

LineEditor's unchanged Sed, Ed.Shared, Ed, and Red suites compile against
published CommandFramework 2.1.0. Their 116, 68, 26, and 28 original tests
exercise substitution, repeated searches, and shared editor regex paths.
All 238 cases passed locally with the published package and with the scratch
bridge in separate outputs. [CI run 39889627829](https://github.com/uniblab/Icod.Expressions/actions/runs/39889627829)
passed the full 487-case Linux gate, including LineEditor and unfiltered Grep.

The same pinned CoreUtils checkout now adds `nl`, `csplit`, and `ptx` original
command suites (35, 43, and 46 cases). They exercise GNU Basic line styles,
regex split controls, and prepared byte input respectively. All 124 cases
passed locally against the published dependency and in separate scratch
outputs with the candidate package and temporary bridge.
[CI run 39890569950](https://github.com/uniblab/Icod.Expressions/actions/runs/39890569950)
passed the full 611-case source-pinned gate, including those commands and
unfiltered Grep. `tac` remains deferred under
[CoreUtils #190](https://github.com/uniblab/Icod.CoreUtils/issues/190).

These counts describe the original suites, not the number of regex calls.
Some tests launch separate apphosts from their original output directories;
the substituted test-host outputs and assembly-origin probe are the
forwarding evidence. Each consumer's uncovered options still need audit.

This Linux gate supplements the three-platform Staging bridge gate above.
Together they validate five source-pinned consumer families, but they do not
prove the complete consumer graph or produce a distributable bridge. Formal
A05 acceptance still requires review of its classified limits. Actual
consumer migration and a CommandFramework ownership change belong to
separate, reviewed PRs.

## Separate, reviewable bridge sequence

1. Keep the extracted source and its original namespaces in this package
   while the A05/E02 parity baseline is preserved. Review the alpha package
   and its three-framework package-only smoke tests.
2. Prepare a **separate** CommandFramework bridge PR that references the
   validated package, replaces its regex declarations with type forwards for
   all 24 public regex types, and handles both shared Text enums without
   duplicate definitions. Internal CommandFramework callers and its other
   Text APIs must compile against the forwarded identities. Do not change
   any existing result, diagnostic, or command behavior in that PR.
3. Build and test CommandFramework and compare the consumer-visible API
   through a test application compiled against the old package and run with
   the bridge. Type forwarding must be checked with real compile/load tests;
   a source-only name comparison does not prove binary compatibility. The
   original CommandFramework test assembly also exercises internal prepared
   types. Keep those assertions in the new repository's 193-case corpus and
   either give the old test assembly explicit friend access plus a direct
   reference to the new library, or retire only its duplicated focused
   tests after the new suite passes. Do not silently drop them.
4. Validate each consumer against the bridge and new package before merging
   an ownership change. Use the [A03 call-site matrix in PR #16](https://github.com/uniblab/Icod.CommandFramework/blob/docs/icod-expressions-a00-in-situ-inventory/doc/Icod.Expressions-A03-Consumer-Compatibility-Matrix.md)
   as the source-pinned inventory. Start with string-only ProcPs and
   DiffUtils, then CoreUtils `expr`/`nl`/`csplit`/`ptx`/`tac`, Grep's byte
   and prepared paths, and LineEditor's Sed/Ed repeated searches and
   replacement ownership. This order is a validation sequence, not a claim
   that earlier consumers are already migrated.
5. Keep application-specific search loops, empty-match progression,
   replacement, output formatting, and command diagnostics in their
   consumers. Reject any silent replacement of GNU/POSIX calls by
   `System.Text.RegularExpressions`. Preserve the `tac` call shape for this
   migration and fix its provider/options mismatch separately under
   [CoreUtils #190](https://github.com/uniblab/Icod.CoreUtils/issues/190).

The bridge does not merge until every discovered A03 consumer builds and its
relevant tests run against the new package, including a package-only path.
CommandFramework keeps its existing implementation until that separate
reviewed change is ready. This document records the necessary gate; it does
not authorize a deletion or dependency switch by itself.
