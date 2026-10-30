# Internal PCRE control escapes

## Intent and approach

Continue issue #7 after merged #103 by implementing the pending `\cX` escape
family. Use the existing byte/scalar literal AST and VM paths. Candidate optimizer
work in #99/#102 and other escape/property families remain separate gates.

Pinned PCRE.NET1.6.0 / PCRE2 10.48 ASCII/Unicode check_escape,
pcre2_compile.c2148..2206, defines `\c` followed by exactly one printable ASCII
source character (20..7E). Uppercase ASCII a..z, then flip bit40. No locale case
mapping or EBCDIC profile. Examples: @->NUL, A/a->01, J->LF, M->CR, ?->DEL,
space->grave, !->a, 1->q, grave->space, {->semicolon. Punctuation is an operand,
not a delimiter, quantifier, comment or quote marker. Nonprintable/non-ASCII
operands and a missing operand are invalid.

Approaches considered: expand/control-rewrite source before parsing (would damage
original offsets and token boundaries); add a new control opcode (unnecessary
state/width duplication); or return PcreByteLiteral from ParseEscape. The
implementer's choice under the standing autonomous development remit is the last.

## Parser and shared semantics

In ParseEscape, after consuming c, validate the next source byte20..7E without
skipping x/xx trivia. Consume it, uppercase a..z arithmetically, XOR40, and return
PcreByteLiteral. Byte and UTF share this operation because every valid operand
and result is ASCII. Existing strict UTF pattern validation runs first.

A missing/invalid operand reports InvalidSyntax at the original escape backslash.
Existing unclosed-class/group diagnostics still own an outer structural error;
`[\c]` consumes ] as the operand and then lacks its class closer. UTF invalid
pattern bytes retain InvalidUtf8Pattern before escape parsing. Do not reinterpret
uppercase `\C` or other unknown escapes as literals.

Existing paths provide all behavior: bracket/range membership and negation,
source CR/LF metadata for cJ/cM (with native negated-singleton rules), repetition,
case folding/options (including letters produced from punctuation), captures and
backreferences, recursion/calls, literal/general assertions and lookbehind widths,
original byte offsets, fragment barriers, cancellation and resource limits.
Quoted `\Q...\E` remains literal text; `\c\Q` consumes backslash as the operand
and leaves Q as ordinary text. No preprocessor, new AST kind/opcode/frame/payload,
matcher changes, snapshots, saved spans or interior pointers.

## Constraints and verification

Internal C#13, net8/net9/net10, UTF-8 LF; root project/solution, src, tests,
samples; no Python. Existing depth256/captures256/instructions4096/marks256/
savedframes1024/default work1,000,000 unchanged. No API, package/version/dependency,
workflow, release or consumer changes, production native/Regex fallback, or raised
limits. Frozen35 BRE/ERE source unchanged. User owns merge; Grep stays last and
CommandFramework removal requires publication and all known consumer migrations.

Pin a literal95-byte printable-operand output table independently against native;
check every operand in atoms/brackets/negation/assertions/repeats under byte/UTF.
Native-first full-capture literals cover punctuation delimiters, quoting, x/xx,
NUL/DEL/CR/LF, folding/r restrictions, captures/retries/calls and behind widths.
All-offset byte/tolerant UTF matrices cover LF/ANYCRLF, malformed/multibyte input,
G/captures and source newline metadata without claiming unresolved #99/#102 cases.
Reject every nonprintable/high-byte operand in byte mode; verify non-ASCII UTF
operands, missing operands, structural errors and original diagnostic offsets.
Keep observer/cancellable/work-limit and compile/match cancellation behavior.
Tests-only draft must fail after native assertions before production edits. No
local SDK: executable verification uses six native Staging jobs, full/frozen214
and configured workload/extraction/API/package gates plus fresh whole-branch review.
