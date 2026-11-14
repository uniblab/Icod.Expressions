# Caller-selected literal compilation

Add default-false internal Literal for PCRE2_LITERAL0x02000000. The complete
owned pattern is literal data: metacharacters, escapes, quotes, inline options,
comments, start directives and control verbs are not interpreted. Byte mode
reads bytes; effective caller UTF/MatchInvalidUtf reads validated scalars. Pattern
prefix text cannot enable UTF/UCP, set newline/notempty or change casing.

## Approach

Validate incompatible caller options at compile entry, before source UTF checks,
then bypass only prefix/grammar processing. A focused parser partial loops over
the existing validated literal reader, stamps the existing casing flags and
returns ordinary literal alternatives through existing whole-word/line wrappers.
The existing compiler and ordered matcher remain authoritative. Quoting/replacing
source would shift diagnostics and mishandle embedded quote terminators; a new
literal matcher would duplicate UTF/case/offset/resource behavior.

Pinned PCRE2 10.48 allows caller caseless, UTF/tolerant UTF and extra word/line/
restricted/Turkish policies. Its source/native masks allow all four extras even
though the public manpage lists only word/line. Use pinned native behavior.
Existing Turkish UTF/conflict validation remains after whole-source UTF checks.
Reject all other currently exposed main/extra regex flags with InvalidSyntax at
byte0 identifying literal compilation, matching native192 precedence. Newline/BSR
compile-context choices and internal work/observer policies remain allowed.

## Scope and gates

Production: options/compatibility predicate, compiler guard, start-settings loop,
parser snapshot/dispatch and new PcreByteParser.Literal.cs. No IR/opcode/matcher/
journal/shared-limit/exported API/dependency/workflow change. No regex captures,
depth or repeat grammar is created by literal-looking punctuation. Existing
compiled instruction/work/cancellation limits remain in force.

Baseline merged244075e6adc284187fd8716dc109af130fc6e533cb9/tree
328e5acad7569d300a752a6604a2d94a6ab90204;81,727 tests/framework and261 prior
test files. Preserve all261 hashes, GNU/BRE/ERE, frozen35 production blobs and214
frozen outcomes/120methods. C#13/net8/net9/net10, UTF8+LF; no Python, agents or SDK
installs. Execute inline in the existing linked worktree; user owns merge/tag/
publication, issue7 stays open and Grep migrates last.

Pin syntax-looking text, all octets, UTF/case policies and prefix suppression,
word/line/newline context, incompatible flags and validation precedence,
malformed source/subject offsets, owned state, canonical work and resource bounds.
Observe clean net8/9/10 tests-only RED and freeze the fixture before production.
Qualify all18 native results and every configured supporting gate. All seven
repository inventories precede final source qualification; final GREEN/review/
identities go on PR245 and only issue7's footer, without docs-only CI pushes.

References: pinned PCRE2 10.48 PUBLIC_LITERAL_COMPILE_OPTIONS/EXTRA_OPTIONS masks,
literal parse loop and native protocols; https://www.pcre.org/current/doc/html/pcre2api.html .

## Tests-first evidence

Tests-only [RED743](https://github.com/uniblab/Icod.Expressions/actions/runs/43483771907), source `02f144580c5af853578e682d3a5ae3a648252b2f`, Linux ARM job132086996194: net8/9/10 each built with zero compiler warnings/errors and ran **786 expected failures / 84,574 passes / 85,360 total / zero skips**. Failures were466 primary profiles,316 compatibility/error boundaries,1 work/ownership case and3 structural cases per framework. All81,727 earlier tests and2,847 new controls passed; every native assertion passed. Freeze `842c7045a4a7ed25d1f486f56702644eed8ac53c` before production. Before this clean RED, seven overlaps were deduplicated and the new helper's managed offset argument was corrected; no expected outcome was weakened.

Implementation stays within the five scoped source files. The parser snapshots Literal and dispatches before grammar/trivia/options; prefix scanning is suppressed while caller global policies remain active. The compatibility predicate accounts for all40 earlier boolean options:9 allowed and31 rejected. All261 prior test files stay hash-fixed. Final qualification/review/identities are external on PR245/issue7.
