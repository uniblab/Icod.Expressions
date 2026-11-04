# UTF erased primitive prefixes before starter classes

## Intent and approach
Continue #7/#102 after merged #153. Native compilation erases direct zero-count
primitive/reference atoms before bitmap analysis. The general starter-class
proof currently skips only leading boundaries when no PLUS set was selected.
Reuse IsUtfCrLfErasedRepeat in that same scan to cross proven erased prefixes,
including possessive wrappers around repeats; do not clone its kind list.

Boundaries cost one visit, erased repeats two, and an exterior atomic repeat
wrapper adds one. Charge before adding under1000; check cancellation per prefix.
Retain an unrecognized node as the first significant node for the existing
worklist. Exhausted branches reject. The selected-repeat nullable walker,
recovery ownership/marker placement, required-repeat and all-alternative CR/LF
checks remain unchanged. Retained zero-count groups/assertions/calls, ordinary
queries/stars and other unproven shapes keep their existing admission.

Considered the general nullable walker and a new erased-kind switch. The former
would broaden ownership admission; the latter duplicates an established source
classifier. Chosen reuse is an implementer decision under standing autonomy.

## Constraints and verification
C#13/net8/9/10,UTF8 LF; root project/solution,src/tests/samples. No subagents/Python/
SDK installation. Parser256/captures256/instructions4096/marks256/frames1024/
work1m/proof1000/window12 unchanged. All23525baseline expectations/frozen35 stay.
No parser/emitter/matcher/Step/VM state/journal/API/limit/workflow/package/
dependency/consumer/CommandFramework changes.

Native-first full captures for erased literal/any/type/set/property/newline/
reference atoms, modifiers/casing, mixed boundaries, wrapper/depth/repeat/
alternative/branch-reset/capture-dependency forms and retained/CR/property/
anchor exclusions. Six newline/UCP/every-offset tolerant fragments, strict
validation, work exact/exhausted/observed, cancellation/default budget and long
ordinary erased-prefix controls; no bitmap parity claim beyond1000.
Observe successful-build new-class RED/all23525baseline pass before source;
tests immutable afterward. Six native Staging jobs/full/frozen214/net8/9/10,
configured gates/seven equal workloads,0test failures/skips/compiler warnings/
errors; author review/exacttree/nineblobs/frozen/clean audit before ready
unmerged PR. Keep102/unsafe100 and release/Grep gates open; publication/all
known consumer migrations precede CF removal; Grep last.

Issue102 is at GitHub's body limit. Preserve its entire recent-progress index
in doc/PCRE-UTF-RECENT-PROGRESS.md and link only that footer to the exact source
commit; keep all earlier body history intact. The versioned index supports
later tranches without a comment-only history. This is metadata maintenance,
not issue completion.
