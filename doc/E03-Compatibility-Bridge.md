# E03 compatibility bridge and consumer migration

**Status:** integration design after extraction parity; no CommandFramework
source or consumer dependency has changed. The alpha package can be built and
used independently, but its copied public types still have the legacy
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

The probe used a direct project reference to the extracted assembly in the
temporary bridge and a published package reference in the original consumer;
it is **not** a package-only bridge, a CI gate, or proof for other consumers,
frameworks, platforms, and all forwarded API members. Do not merge a
CommandFramework bridge based solely on this result.

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
