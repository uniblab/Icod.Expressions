# Internal PCRE literal braces and counted-quantifier recognition

## Intent and approach

Continue issue #7 after merged #90 by closing the ordinary literal-brace gap.
Follow PCRE.NET1.6.0 / PCRE2 10.48 Compiled/JIT. Standing autonomous development
remit covers this internal syntax tranche.

Use one syntax-only counted-quantifier recognizer shared by all ordinary brace
contexts, followed by the existing bounded numeric reader. Native validates a
complete shape BEFORE interpreting counts: `a{65536x}` contains literal braces,
while `a{65536}` is an invalid quantifier. Merely catching numeric parser errors
and turning them into literals would hide genuine overflow/reversed-bound errors;
rewriting source text would disturb byte diagnostics and lexical scopes.

## Contract

- Complete `{n}`, `{n,m}`, `{n,}` and `{,m}` forms are counted quantifiers. At
  least one number is required. ASCII digits only; SP/HT may appear after `{`,
  before `}` and around comma, not inside a decimal number. Recognition does not
  ignore x/xx comments, line breaks or other whitespace.
- Validate complete shape first. Only then parse decimal bounds, reject values
  above65535 or reversed finite bounds, and apply ordinary compile expansion
  ceilings. Leading zeroes remain safe. Huge numbers in a NON-quantifier shape
  remain literal digits; do not report numeric overflow for them.
- An opening brace that does not start a complete counted form is an ordinary
  literal atom. A closing brace is always an ordinary literal atom outside its
  explicit token readers. Incomplete forms, empty/comma-only braces and strings
  such as `{x}`, `{2x}`, `{1 2}` are parsed character by character. Characters
  after the opening brace retain their usual grammar: quoting, comments, options,
  groups or quantifiers are not hidden inside a synthetic brace token.
- A complete counted form without a preceding repeatable atom is invalid;
  recognized forms after anchors, bare options or condition assertions and
  repeated quantifiers remain invalid. Non-counted literal braces are allowed
  after those items. Greedy/lazy/U/possessive semantics for real repetitions stay
  on the existing path. Literal braces themselves can be quantified.
- Escaped and quoted braces and bracket members remain literal. Explicit braced
  tokens keep their own syntax: x/o/N codepoints, properties, capture names and
  references do not acquire ordinary literal fallback. In particular N braces
  must name a valid UTF codepoint or contain a real counted quantifier.
- Source byte positions, UTF scalar widths, local casing/options, captures,
  assertion/condition contexts, lookbehind widths and newline metadata remain
  shared. Validate syntax even in erased/unselected branches.

## Representation and integration

Add a focused parser partial `PcreByteParser.Repeats.cs`. Move existing
ParseCountedBounds, ReadCount and SkipBraceHorizontalSpace there without changing
numeric/escape semantics. Add IsCountedQuantifier(int offset): scan a local source
cursor through the complete grammar without mutating parser index or allocating;
poll cancellation while scanning digits and SP/HT. ParseCountedBounds requires a
recognized shape before interpreting numbers, preserving UnsupportedSyntax for
its private non-counted caller contract used by the N reader.

At each ordinary quantifier boundary, classify `{` before applying repeatability
or duplicate checks: AppendNode initial and trailing checks, bare option setting
and condition assertion completion. ParseAtom treats `}` and non-counted `{` as
literal nodes; a recognized leading `{` invokes numeric validation then reports
quantifier-without-atom. All literal/quantifier nodes and compiler/VM logic remain
unchanged. Scans examine at most a bounded number of times each numeric/space
segment; no retries over arbitrary source suffixes or saved spans are introduced.

## Verification and limits

Publish a new native reference fixture and adjusted obsolete brace diagnostics
before source edits; observe expected red CI. Pin explicit outcomes, group counts
and every numbered source-byte capture: malformed/unclosed/large-number literals,
recognized overflow/order errors, optional bounds/SP/HT, literal brace repetition,
post-quantifier/anchor/option/condition positions, x/xx/comments/quotes/classes,
escape-token boundaries, UTF offsets, nested lookaround/conditions and behind
widths. Include malformed UTF/CRLF caller offsets and existing instruction/work/
cancellation ceilings. Use fresh whole-branch review and all six Staging jobs
on net8/net9/net10 full+214 frozen; local whitespace/frozen35. No local SDK.

No public API/version/dependency/release/consumer/VM changes. Root project and
solution, src, tests, samples; C#13, UTF-8 LF, no Python. Local Debug, PR Staging,
main/tag Release. Preserve depth256/capture256/instructions4096/saved paths1024,
default work1,000,000 and cancellation. User owns merge; Grep/CommandFramework
migration gates remain.

## Pinned sources

pcre2_compile.c read_repeat_counts1350ff: complete syntax before numeric decoding;
ordinary left-brace dispatch3830ff; check_escape N-brace dispatch1593ff.
