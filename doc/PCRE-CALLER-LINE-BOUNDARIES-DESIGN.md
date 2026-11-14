# Caller-selected line boundaries

## Intent and architecture

Continue issue #7 after merged PR249 with default-false internal NotBol and
NotEol policies (native match options1/2). Select them once per owned internal
compiled pattern, as for PR249's empty policies; this is not a new public
per-call match-options API. Native reference fixtures pass them to Match,
never Compile. Keep library builds net8.0/net9.0/net10.0 with C#13, and run
test cases only on net10.0 across six native OS/architecture hosts.

Use the existing typed anchor factories, preserving the distinction between
source caret/dollar and explicit backslash-A/Z/z. Read both caller booleans
into readonly parser fields, annotate existing anchor nodes, and lower disabled
non-multiline anchors to existing Fail instructions. Multiline line anchors
retain newline behavior while suppressing only their subject boundary through
the existing instruction Second operand. No new AST kind, opcode, frame,
journal field, instruction count, capture/depth slot or exported API.

Two alternatives were considered: matching every anchor against mutable global
context would require retained source provenance anyway, while synthetic source
rewrites would change parsing, offsets, recursion and quotas. Existing typed
anchor lowering is the chosen bounded integration. Standing autonomous inline
authorization covers this choice; no subagents, approval pauses, Python or
local SDK installation. User owns merge/tag/publication; Grep migrates last.

## Native contract

- NotBol rejects caret at the physical subject start. Under multiline it does
  not reject internal newline starts, including the alternate-caret EOF case.
- NotEol rejects non-multiline dollar entirely, including before a final
  newline and under DollarEndOnly. Under multiline, internal newline positions
  remain eligible but the current subject-end boundary is suppressed.
- Explicit backslash-A, backslash-Z and backslash-z keep their own semantics.
  The flags do not redefine the requested offset as a physical boundary.
- Global flags survive local option reset, assertions, conditions and calls.
  MATCH_LINE inserts real caret/dollar guards; the flags affect those guards
  with entry-scope multiline settings. Literal source remains data, while
  implicit word/line guards retain their established meaning.
- Fixed assertions use physical subject context. Bounded-variable lookbehind
  retains the existing native JIT scoped end limit: multiline dollar at its
  clipped end is rejected by NOTEOL even if a physical newline follows, while
  a fixed lookbehind may still see that internal newline. Do not conflate them.
- Source quote/class/comment escapes remain inert; fake NOTBOL/NOTEOL source
  directives remain unsupported. Match flags do not rescue incompatible
  Literal+Multiline compilation or alter syntax/UTF diagnostics.
- Parse original anchor types before lowering, so forbidden quantifiers,
  lookbehind widths, names/references and source positions stay authoritative.
  Retain whole-source/subject UTF validation, invalid-offset/limit priority,
  cancellation, first-success work and owned source/options.

## Files and scope

PcreByteOptions.cs adds two bools. PcreByteParser.cs snapshots them;
PcreByteParser.Anchors.cs initializes existing typed anchors. PcreByteSyntax.cs
adds default-false metadata to existing anchor records. PcreByteProgram.cs
lowers disabled caret/dollar without erasing source structure and carries the
multiline boundary suppression in Second. PcreByteMatcher.cs checks that
operand only at the relevant StartLine/EndLine boundary. No compiler entry,
start-directive, capture/call/scope, dependency, packaging or workflow changes.

## Independent proof and review boundaries

Pin 1,023 groups/45,411 native10.48/JIT0 matching-offset protocols,69 compile
diagnostics and36 work/ownership/validation/capacity cases:1,128 new tests,
88,872 total. Native batches must precede managed comparison; no-match means
-1. Separate864 strict split-offset(-36) rows from match protocols, with explicit
strict validation tests. Preserve signed endpoints, all captures and successful
labels. Split the protocol data into two partial-class files; keep shared
helpers and supplemental cases in their own partial-class file.

The five review boundaries are: explicit versus caret/dollar provenance;
multiline/newline/EOF and fixed-versus-variable scope context; original offset,
UTF fragments and anchoring/cuts; caller/global/reset/Literal/word-line
composition; ownership, validation priority, stable budget measurement,
cancellation and exact capture/depth/instruction capacities.

Expected tests-only RED is679 missing-policy groups plus3 ownership cases:
682 failures/88,190 passes/88,872 total/zero skips, with zero compiler warnings
or errors. Work and capacity controls reach their assertions before caller
integration. Freeze all three new C# files only after clean RED, before
production. Preserve all266 prior test files/261 C# sources, frozen35 source
blobs and214 frozen outcomes/120 methods per native result.

All seven inventories precede final source qualification. Require six fresh
net10 results at88,872 each, all three library outputs per native host and
every configured supporting gate. Final GREEN/author-review/exact source,
base, tree and proposed-merge evidence goes on PR250 and only issue7 footer,
without a documentation-only CI repeat. Wider optimizer/resources #102,
remaining syntax/UTF/callout/API and realistic consumer/release gates remain.
