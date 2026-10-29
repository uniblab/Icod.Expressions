# Internal PCRE capture-participation conditions

> Historical scope: traditional atomic assertion conditions are added by
> `PCRE-ASSERTION-CONDITIONS-DESIGN.md`; their pending status below describes #89.

## Intent and approach

Continue issue #7 after #88 with conditions that test whether a capture has
participated. Match PCRE.NET1.6.0 / PCRE2 10.48 Compiled/JIT. Lower to a single
capture-test instruction with yes/no targets, reusing existing numbered captures,
journal and heap compiler. Lowering to ordinary alternatives would incorrectly
fall back to the other branch after a selected branch fails; a second matcher or
new capture state would duplicate existing semantics. This choice follows the
standing autonomous development remit.

## Grammar and native contract

- Support `(?(N)yes|no)`, ASCII signed-relative `+N/-N`, and bare, angle-bracket
  or single-quoted names: `(?(name)...`, `(?(<name>)...`, `(?('name')...`.
  Positive relative1 means next capture; negative1 means most recently opened.
  Resolve against the current syntactic capture count, including branch-reset and
  n rules. Leading zeros are allowed; zero/sign-zero, unavailable targets and
  overflow are invalid. Wrapped numbers are not names and remain invalid.
- Headers contain no ignored whitespace, even under x/xx; braces are not a name
  delimiter. Reuse existing ordinal name grammar and128-byte limit. Resolve
  forward numeric/named targets after complete parsing; undefined targets reject
  even in erased or unselected branches.
- At most two direct branches; omitted no-pattern is an empty runtime branch.
  Nested alternatives remain unrestricted. The group adds one nesting level and
  no capture slot, restores options normally, and uses ordinary group repetition
  and progress rules. It does not create assertion context: K is allowed unless
  an enclosing assertion forbids it.
- A participating empty capture is true. Duplicate names under J test ANY set
  slot in declaration order; numeric conditions test only their own slot. Choose
  exactly one branch. Failure inside it cannot try the other branch, although
  outer backtracking may revisit the test after restoring captures.
- Bare R/Rdigits (ASCII digits only) resolves an actual matching capture name globally (including
  forward declarations); without one it denotes recursion and stays unsupported.
  Wrapped R/Rdigits always means a capture name. Bare DEFINE always denotes the
  pseudo-condition and stays unsupported, even when a capture has that name;
  wrapped DEFINE is supported. Plain VERSION is an ordinary capture name, while any longer bare uppercase
  VERSION prefix is reserved by native version parsing and remains UnsupportedSyntax,
  even if it names a real capture (VERSIONX, for example). Wrapped VERSIONX and
  lowercase versionx remain capture names. Assertion conditions, recursion
  conditions/calls, DEFINE, VERSION comparisons and other families stay pending.

## Representation and integration

Add PcreByteCaptureConditional(CaptureNumbers,Body) to the semantic tree. A private
parser token and ParsedCaptureConditional retain unresolved numeric/name/header
identity plus Body until post-parse resolution. GroupFrame holds the optional
condition token and rejects a third direct branch. Reuse ParseCaptureName for
names; bound decimal accumulation and relative arithmetic by capture256.

Extend heap traversal/rewrites for both temporary and resolved conditional bodies.
Resolve conditions after named-reference fixups and before lookbehind lengths;
cache immutable candidate arrays by name/number for reuse. Preserve node i/r in
replacement, even though only child comparisons use it. Compiler emits CaptureTest
with existing First/Second pc fields and CaptureNumbers payload, then compiles
yes/no sequences with one end jump and no conditional Split. Matcher Step-polls
each participation lookup, selects a target and consumes no subject bytes; existing
journal/frames handle any retries. No new saved-frame or scope-arena state.

## Native lookbehind distinction

Width analysis follows only explicit branches. An omitted else is not materialized
in Body: native considers its yes width alone, despite an implicit empty false
branch at runtime. For example `(a)?(?<=(?(1)b))x` misses x but matches x after b,
with capture1 unset; an explicit empty else permits x at start. Omitted-else
b{256} is fixed and accepted; explicit-empty-else b{256} is variable and rejects
at the existing255 limit. Preserve this pinned behavior using GroupWidth over
explicit Body branches, including all child/reference validation. Condition tests
themselves introduce no capture-width dependency. CanMatchEmptyAtAnyOffset treats
an omitted runtime else as nullable, independently of native width analysis.

## Verification and boundaries

Native explicit outcomes/group counts followed by every numbered capture byte
span: numeric/relative/forward/three name forms; duplicate ANY versus references;
empty/unset/self/loop captures; chosen-branch failure and later re-evaluation;
nested options, n/U/i/r/J, repeats, atomic/non-atomic/negative transactions;
fixed/variable behind including omitted/explicit else and256/255 distinction;
UTF/invalid-UTF/CRLF offsets; header/target/branch-count diagnostics and reserved
names; depth256/capture256/instructions4096/saved paths1024/work/cancellation.
Replace the obsolete capture-condition-pending row; retain other condition kinds.
Update current inventories and relevant historical notices.

The CRLF sweep also pins the existing multiline `$` path: PCRE2 JIT checks the
current newline character and accepts LF after a consumed CR. This is separate
from unanchored candidate advancement over CRLF. Assertion-condition aliases
beginning with `*` remain unsupported just like their `?` spellings.

No public API/version/dependency/release/consumer change. Reuse integer frames,
scope arena and journal; no recursion, capture snapshots or saved spans. Existing
default work1,000,000 and compile/runtime ceilings remain. Per-name candidate
lists and distinct names are bounded by capture256; numeric arrays can be shared
by target number. Root project/solution, src, tests, samples; UTF-8 LF; no Python.
Local Debug; six-platform PR Staging. No local SDK: observed CI red/green on
net8/net9/net10 full plus214 frozen tests and configured workload/API/consumer
checks; local whitespace/frozen35. User owns merge; migration remains gated.

## Pinned sources

pcre2pattern.3 conditional-group/capture-number/name/recursion/DEFINE sections;
pcre2_match.c6067–6084: capture participation selection;
pcre2_compile.c9895–9934: explicit conditional branches in width analysis.
Native probes check pinned10.48 and nonzero compiled JIT size.
