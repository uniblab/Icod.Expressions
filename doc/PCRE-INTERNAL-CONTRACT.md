# Internal PCRE consumer contract

The compiled [declaration snapshot](PCRE-INTERNAL-API.txt) records the internal
byte-pattern boundary at PR #293. PR and main CI compare it with the built
library using the existing API inspection tool on .NET 10. Changes to declarations,
nullability, diagnostic values, or default options require an explicit snapshot
review.

## Declaration scope

The snapshot selects eleven types by name. It includes their declared public
constructors, properties, fields, events, and methods, including generated record
members. It also records the internal successful-match mark, the default work
limit constant, readonly structs, enum storage, all option and match-policy
defaults, and exported PCRE types.
Types are ordered by name and members by ordinal text. The snapshot header
excludes assembly version so ordinary package-version changes produce stable
contract output.

| Area | Types |
|---|---|
| Compilation | PcreByteCompiler, PcreByteOptions, PcreByteCompileResult, PcreCompiledBytePattern |
| Match policy and results | PcreMatchPolicy, PcreByteMatchResult, PcreByteMatch, PcreByteCapture |
| Metadata | PcreCaptureInfo |
| Diagnostics | PcreByteDiagnostic, PcreByteDiagnosticCode |

The parser, program representation, VM instructions, optimizers, and test
progress hook are implementation details outside this declaration boundary.
Review new types reachable from the boundary and add their declarations to
the selection when extending the consumer contract.

Run the tool from the repository root to inspect the compiled declarations.

```sh
dotnet run --project tools/RegularExpressionApiSnapshot/RegularExpressionApiSnapshot.csproj -c Staging -- --pcre-internal
```

CI compares the entire output with PCRE-INTERNAL-API.txt. The existing
zero-argument command continues to check the public extraction API.

## Behavioral contract

| Operation or value | Current behavior and evidence |
|---|---|
| Compile | Copies the visible pattern bytes into owned storage. Returns a pattern or a diagnostic. The package consumer overwrites caller pattern storage after compiling. |
| Match | Reads the supplied byte memory view. Start offsets, captures, and diagnostics use that view's byte coordinates. An empty subject and an offset at the visible end are accepted inputs. |
| Compile result | IsSuccess requires a pattern and no diagnostic. |
| Match result | IsSuccess reports completion without a diagnostic. A completed search can have Match equal to null. |
| Captures | Group zero is the reported match range. An unset group has a null capture. Empty captures have zero length. Lookaround backslash-K can produce reversed reported group-zero endpoints when explicitly allowed. |
| Capture metadata | CaptureCount excludes group zero. Names use ordinal order. GetGroupNumbers returns canonical ascending numbers. Successful named lookup uses native registration order for duplicate names and returns the first participating capture. |
| Match policy | The six per-call restrictions combine with compiled and source policies. The existing 64-policy native corpus and packaged consumer exercise this composition. |
| Limits | MatchWorkLimit defaults to 1,000,000 and must be positive. MaxVarLookbehind defaults to 255. MaxPatternLength defaults to null and limits original visible pattern bytes when set. |
| Cancellation | Compile and Match propagate OperationCanceledException. Existing deterministic work-observer tests exercise cancellation during matching. |
| Diagnostics | The enum and diagnostic shape are snapshotted. Messages describe the internal engine's errors. Tests establish their declared priority and offsets. |

The existing full suite and ten package-consumer records supply behavioral
evidence. The declaration check detects structural changes independently of
those runtime cases.

## Compatibility and remaining work

| Area | Current acceptance |
|---|---|
| Reference profile | PCRE.NET 1.6.0 with pinned PCRE2 10.48. The native records and syntax matrix define the tested subset. |
| Input widths | Managed execution supports raw bytes and UTF-8. Native 16-bit references exist for future managed UTF-16 work. |
| Syntax exclusions | UTF code-unit escape execution and callout execution remain open. Their current rejection behavior is covered by existing syntax and package tests. |
| Match operations | Complete single matches are implemented. Partial matches, next-match iteration, and substitution remain open. |
| Resource accounting | Deterministic work, cancellation, and bounded engine structures have existing tests. Complete native-equivalent resource controls and hostile-corpus closure remain open. |
| Performance | Paired input allocation records cover ten existing profiles. Representative controlled-host timing and broader memory evidence remain open. |
| Release | P09 still needs broader conformance, hostile-input, and performance closure. P10–P12 native UTF-16, the remaining roadmap gates, and public API/release acceptance follow their declared requirements. |

See the [roadmap](PCRE-ROADMAP-STATUS.md),
[syntax inventory](PCRE-SYNTAX-ACCEPTANCE-MATRIX.md),
[resource contract](PCRE-RESOURCE-ERROR-CONTRACT.md), and
[Grep fixture inventory](PCRE-GREP-CONSUMER-FIXTURES.md) for detailed evidence.
