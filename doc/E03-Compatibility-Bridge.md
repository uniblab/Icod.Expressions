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
