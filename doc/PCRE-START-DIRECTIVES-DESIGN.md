# Internal PCRE start directives

## Intent and scope

Continue issue #7 after #104 with the four pattern start directives whose
semantics already exist as internal compile options: `(*LF)`, `(*ANYCRLF)`,
`(*BSR_UNICODE)` and `(*BSR_ANYCRLF)`. This closes their syntax gate without
adding a matcher mode or changing the public PCRE/release/consumer gates.
Primary authority is pinned PCRE.NET1.6.0 / PCRE2 10.48,
`pcre2_compile.c` pso_list and the skipatstart scan around lines10499..10620.

## Contract

- Recognize exact, case-sensitive complete directives in an uninterrupted
  prefix starting at original byte0. Do this before option parsing, comments,
  quotes or whitespace skipping. Scan on the heap with cancellation per item.
- Repeated directives are allowed. Last directive wins independently for the
  newline convention and BSR, overriding the corresponding caller option.
  A missing family preserves the caller setting. Both choices are global,
  surviving groups, alternatives, inline option reset and subroutine calls.
- `LF`/`ANYCRLF` govern existing dot, non-newline, line anchors, final-newline,
  x/xx comment termination and candidate continuation. BSR governs only `\R`,
  including its existing atomic CRLF-first lowering and lookbehind widths.
  Newline and BSR must remain independent.
- Retain the original byte array and advance the parser index; never strip or
  rewrite the prefix. Quantifiers cannot repeat a directive. Diagnostics from
  the body retain original byte offsets, including UTF byte offsets.
- A misplaced, truncated, misspelled or other `(*...)` directive retains the
  existing explicit UnsupportedSyntax policy at its opening parenthesis.
  Existing group aliases remain supported. Quoted/bracketed text is ordinary
  text and never changes settings. Invalid UTF pattern validation keeps its
  existing compile-entry precedence. Work/cancellation/limits stay unchanged.

## Approaches and choice

1. **Parser prefix scan plus effective-setting properties (chosen by Codex):**
   extend the current parser input with BSR; let Parse consume exact prefixes
   before ordinary grammar and expose the final settings to the compiler.
   This keeps original offsets and one parser/compile configuration source.
2. Compiler preprocessing could select options and pass a start index to the
   parser. It adds a second source-position contract without benefiting these
   already supported modes. Stripping source would break diagnostics.
3. Runtime directive nodes would add needless opcodes/scoped state and get
   global precedence wrong. No semantic need justifies that route.

## Parts and constraints

A focused parser partial owns the four-token scan. Existing parser newline
state becomes writable only during the prefix scan. The compiler passes both
caller defaults in and sends both effective choices to PcreByteProgramCompiler.
No AST/opcode/frame/journal/matcher or Unicode tables change; no retained spans,
interior pointers, snapshots, cloned width engines or fallback.

C#13, net8/net9/net10; source in src, tests in tests, docs in doc. Local Debug;
six native PR Staging jobs. No local SDK: executable managed RED/GREEN uses CI.
No Python, public API, package/version/dependency/workflow/release/consumer
changes. Grep remains last. CommandFramework removal still requires published
Expressions and migration of every known consumer. User owns merge.

## Verification and boundaries

Literal native assertions precede managed comparison: both newline choices,
both BSR choices, duplicate/mixed precedence over opposite caller defaults,
CR/LF/CRLF/VT/NEL/LS/PS, x/xx comments, reset/groups/calls/behind, all captures,
all caller offsets and tolerant UTF fragments. Negative placement/truncation,
quotes/brackets, original-byte diagnostics, long prefixes and work equivalence
with direct options protect parser boundaries and resource behavior. Tests-only
CI must fail as UnsupportedSyntax before production changes. One fresh final
review and all six full/frozen214/configured CI gates precede ready status.

Other newline settings CR/CRLF/ANY/NUL, UTF/UCP directives, limits/optimization
verbs and other directives remain pending/rejected. This tranche does not fix
#99/#102 optimizer parity or imitate #100 unsafe native malformed-Xuc spans.
Builder may choose private helper/property names and fixture organization;
there are no new engine policy decisions left to infer.
