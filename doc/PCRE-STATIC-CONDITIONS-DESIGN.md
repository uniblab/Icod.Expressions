# Internal PCRE DEFINE and VERSION conditions

## Intent and approach

Continue issue #7 after merged #91 with DEFINE declaration groups and VERSION
comparisons against the current internal PCRE2 10.48 compatibility target. This
is a compatibility level, not the Icod.Expressions package/runtime version or a
claim that a native engine is present. It stays internal pending public release
review. Standing autonomous authorization covers this bounded extension.

Represent both as static conditional nodes and compile a Jump to the selected
branch, retaining code generation for both bodies. This shares existing capture
numbering, global fixups, validation, compilation limits and later subroutine
foundations. Erasing unselected bodies during parsing would lose declarations
and diagnostics; a second matcher or public version facility is unnecessary.

## Native contract

- Bare exact DEFINE is always false, even if a capture named DEFINE exists.
  Allow exactly one direct branch, with nested alternatives/groups unrestricted.
  Runtime never enters this body, but its captures and names are globally
  registered and its syntax/references/nested assertions are validated.
  Wrapped `<DEFINE>` and quoted names retain capture-test semantics.
- Bare VERSION with no comparison remains an ordinary capture name. A longer
  uppercase VERSION prefix is reserved for version syntax. Accept only
  `VERSION=major[.minor]` and `VERSION>=major[.minor]`, ASCII decimal components
  each0..1000, leading zeros allowed, omitted minor defaults0. Compare integer
  tuples with compatibility(10,48): `=10` is false, `>=10` true, `=10.048` true,
  `>=10.480` false. No signs, header trivia, extra components or other operators.
  Malformed reserved prefixes such as VERSIONX now reject as InvalidSyntax;
  wrapped VERSIONX and lowercase names retain existing capture semantics.
- VERSION allows at most two direct branches. Missing else is empty at runtime.
  A fixed choice never tries its sibling on failure. Both branches still parse,
  register captures, resolve references, validate assertions and compile within
  the existing bounded instruction/node budget. Unreachable captures stay unset.
- These groups add one source nesting level and no capture slot. Option scope,
  branch numbering, UTF source byte positions and enclosing assertion K contexts
  remain existing group rules. Unsupported recursion/subroutine/callout syntax
  remains rejected even in unreachable bodies.
- Native ignores a quantifier on an always-false single-branch conditional:
  DEFINE and false VERSION with omitted else retain an ignored-runtime Repeat
  wrapper for native width analysis, with no repeat mark, expansion or possessive
  wrapper in execution. Still parse/validate numeric
  bounds, suffixes and repeated-quantifier errors. Explicit else, even empty,
  remains an ordinary repeatable conditional.

## Width and metadata

DEFINE contributes zero width, regardless of an unbounded body. In a consumed
lookbehind width scan its entire body, including nested behind widths, is skipped.
In a global scan (outside a consumed behind, or inside a lookahead reached from
a behind), nested behinds inside DEFINE are validated. A heap prepass mirrors
these native GlobalScan/WidthScan modes and records source offsets of suppressed
behinds; a suppressed DEFINE body remains suppressed through all nested contexts.
Global lexical traversal still records capture definitions and resolves references
unconditionally; consumed-reference dependency analysis validates any definition
body it actually needs. Suppressed behinds can retain unlowered reverse metadata
because they are unreachable inside DEFINE. VERSION width analysis considers ALL
explicit branches, independent of the constant result. As for existing conditions,
an omitted runtime else is not synthesized in the AST or native width analysis.
Ignored-runtime repeats retain their original min/max in width analysis: unbounded
forms behind reject, and a false VERSION omitted else with yes-width1 repeated
{2} still requires two left-context characters. DEFINE repeated finite times
stays width0. Ignored repeats retain a conservative logical repeat-count charge
against the existing4096 compile-visit budget (no expansion or iteration), so
enormous finite/minimum bounds still reject rather than bypass compilation limits.

Runtime nullability follows the selected branch (or empty omitted else). Explicit
newline metadata visits ALL static bodies, including DEFINE/unselected VERSION
branches: pinned native compilation sets its global CR/LF flag regardless of
reachability. This preserves CRLF candidate advancement. Existing fixed/variable
behind limits and invalid-UTF fragment behavior remain shared.

## Representation and parser/compiler integration

Add PcreByteStaticConditional(Value,Body,IsDefinition=false). A private static
condition token and TryParseStaticCondition(int offset,out StaticCondition) in
`PcreByteParser.StaticConditions.cs` recognize exact DEFINE and reserved VERSION
headers before the capture-condition reader. Use bounded decimal accumulation
and per-byte cancellation checks. Internal compatibility major/minor constants
are10/48. Extend GroupFrame with the optional token, one-branch DEFINE validation
and semantic construction; preserve ignored false-one-branch Repeat AST metadata
after validating full syntax, skip its mark/possessive state, and normalize only
at compiler lowering. The body is never erased.

Tree traversal/rewrites visit every body. Extend the lookbehind-resolution prepass
with suppressed source offsets, preserving unconditional capture maps. Width
analysis skips DEFINE bodies and distinguishes DEFINE0 from VERSION explicit
GroupWidth. Nullable and newline analyses follow the rules
above. Compiler emits one choice Jump plus normal yes/no sequences/end Jump;
compile both bodies without conditional Split; ignored-runtime repeats schedule
their child once. No VM instruction/frame/scope/
journal changes. Existing capture journal continues to govern surrounding paths.

## Verification and boundaries

Publish tests/docs only as a draft and observe missing-feature red before source
edits. Native10.48/nonzero JIT outcomes and every numbered source-byte capture:
DEFINE declarations/names, nested alternatives, global refs/numbering/options,
version grammar/bounds/order/equality and reserved-name priority; chosen branch
failure, unreachable bodies and captures; ignored versus ordinary repeat paths;
zero DEFINE widths and nested validation, VERSION explicit-only width quirks;
malformed UTF/CRLF/G offsets, K contexts, depth/capture/instruction/repeat/work/
cancellation budgets. Fresh whole-branch review and six Staging jobs with
net8/net9/net10 full+214 frozen and configured compatibility/consumer gates.

Root solution/project, src, tests, samples; C#13, UTF-8 LF, no Python. No local SDK:
managed red/green uses CI; local whitespace/frozen35. Local Debug, PR Staging,
main/tag Release. No public API/version/dependency/release/consumer change.
Preserve depth256/capture256/instructions4096/saved paths1024/default work1,000,000
and cancellation. User owns merge; Grep/CommandFramework migration stays gated.

## Pinned sources

pcre2_compile.c VERSION parser5468ff, DEFINE/version compilation6795/6830,
branch-count validation6968, ignored false-one-branch repeats7545,
DEFINE zero-width9891/VERSION branch-width9913 and global newline8473;
pcre2_compile_class.c global newline1499/1516; pcre2_match.c CRLF advancement8050.
