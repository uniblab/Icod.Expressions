# Capture metadata inventory addendum

## PR286: immutable internal capture metadata

Compiled byte patterns gain an owned CaptureInfo snapshot: count excluding group
zero, unique UTF-16-ordinal names and exact ordinal lookup to sorted distinct
positive slots. Duplicate names/branch-reset aliases retain their mappings.
Unknown names return empty; null is an argument error. Read-only stable views
support allocation-free repeated reads, before/after matches and diagnostics.
The parser's declaration/resolution order and all matching/work semantics stay
fixed. The width-neutral metadata shape can later serve native16 lowering.

Independent pinned PCRE2/SLJIT preflight records 214 complete count/name/match
protocols (133 matches/81 misses), all JIT zero/positive size, including raw/UTF/
UCP, caller/source flags, literal text, Unicode ordering, aliases, DEFINE/recursion/
assertions, unused slots, 256 captures and 128-byte names. Canonical name ordering
is explicit; raw native entries remain recorded. 223 additions give 105,394 total.
All-six successful-build RED is exactly222 unused CaptureInfo stub failures /
105,172 passes /zero skips, with native/old match checks and pre-cancellation
passing. Data/assertion/consumer/C/XML sources froze before implementation.
The snapshot copies/sorts owned arrays after successful program admission; parser
resolution order and matcher/VM bodies remain fixed.

A fourth isolated package result requires all 214 metadata protocols. Preserve
42+1+2, four allocation and six fragment-allocation results, fresh feed/cache,
old sample helpers/data, workflow call sites, dependencies/version/release and
all old C# test blobs/A05. Two new Compile links are the sole old-test-file XML
exception. No public PCRE API, matcher/VM or consumer migration is introduced.

See [design](PCRE-CAPTURE-METADATA-DESIGN.md),
[plan](PCRE-CAPTURE-METADATA-PLAN.md) and
[PR286](https://github.com/uniblab/Icod.Expressions/pull/286). Whole-branch review
and seven inventories precede final six-host 105,394/frozen214/120, all target
builds/four package records and actual compatibility/Gnulib/support/script gates,
zero failures/skips/coded compiler warnings/errors. Final identities/results,
completion/cleanup/ready stay external on PR286 without source/docs/test edits
after GREEN. #7/#102/footer fixed; broader P09, mandatory native16/P10-P18,
remaining P01/Stage C and real migrations remain open. Grep last; user owns
merge/tags/publication.

## Shared inventory mapping

This addendum records the PR286 increment for the five shared inventories.
Their earlier gates and limitations still apply.

| Inventory | Increment recorded here |
|---|---|
| [Roadmap](PCRE-ROADMAP-STATUS.md) | P09 internal capture metadata; wider release gates remain open |
| [Recent progress](PCRE-UTF-RECENT-PROGRESS.md) | Native-first214 protocols/223 tests and all-six RED222 |
| [Syntax](PCRE-SYNTAX-ACCEPTANCE-MATRIX.md) | No grammar change; metadata preserves parsed declarations and aliases |
| [Resources/errors](PCRE-RESOURCE-ERROR-CONTRACT.md) | Owned bounded copy after admission, cancellation, stable read-only/no-allocation reads; existing diagnostics/work unchanged |
| [Grep fixtures](PCRE-GREP-CONSUMER-FIXTURES.md) | Four package records including214 metadata protocols; actual migration remains separate |

The large syntax/resource/Grep inventory files remain byte-identical to merged
PR285. Automatic review rejected publishing the proposed large resource-document
replacement even after its public baseline and small authored addition were
verified. This small authored addendum records the update without publishing
that rejected replacement or retransmitting those large inventory payloads.
