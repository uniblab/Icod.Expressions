# Internal PCRE capture and repetition options implementation plan

**Goal:** Implement standalone/scoped n/U without new VM behavior.

**Architecture:** Extend parser option state, capture allocation and repeat
lowering. Preserve the existing shared matcher, capture journal and numbered
byte-span result format.

**Tech stack:** C# 13; net8.0/net9.0/net10.0; PCRE.NET 1.6.0 / PCRE2 10.48.

**Spec:** [PCRE-CAPTURE-REPEAT-OPTIONS-DESIGN.md](PCRE-CAPTURE-REPEAT-OPTIONS-DESIGN.md).

## Constraints and review focus

- Root project/solution; src, tests, samples; Debug locally, Staging PR matrix.
- No subagents, public API/version/dependency, consumer or runtime-frame changes.
- Named captures stay capturing under n; already-open groups retain their slots.
- Suppressed groups affect decimal/octal and relative-reference numbering.
- Group restoration precedes the quantifier around that group.
- A possessive repeat remains greedy under U; a question suffix toggles U.
- Alternative propagation is compile time, including branch-reset groups.

## Task 1: Pin the behavior first

Create `tests/Pcre/PcreCaptureRepeatOptionReferenceTests.cs`, comparing whole
and every capture span with the pinned native oracle and explicit key outcomes.
Test capture modes/names/numbering, all repeat families/suffixes, restoration,
combinations, atomic fallback, empty/UTF/quoting, selected malformed-UTF and
CRLF offsets, limits and cancellation. Commit tests and these documents, open
a draft PR and inspect the expected missing-feature failures before coding.

## Task 2: Implement the parser changes

Modify `src/Pcre/PcreByteParser.cs`: add noAutoCapture and ungreedy state;
extend `TryParseOptions` with n/U set/unset flags; save/restore both around
all groups. Ordinary groups use !noAutoCapture at opening, while all three
named forms explicitly capture. Select quantifier suffix before computing
PcreByteRepeat.Greedy; plus forces greedy and uses PcreByteAtomic, question
reverses the current U setting. Preserve repeat marks and diagnostic offsets.

Update current design, acceptance and consumer inventories with this support
and the remaining pending families. Leave historical tranche design docs intact.

## Task 3: Remove recursive group parsing after the observed stack overflow

Run 40776896443 macOS x64 net8 aborts in the existing
BoundsPatternNestingDuringCompilation test, with 257 ParseAtom /
ParseAlternatives recursion pairs. Treat this existing test as the red
regression. Move group traversal into PcreByteParser.Groups.cs with bounded
GroupFrame objects and an explicit parent stack; retain source offsets,
branch numbering, quote state, option restoration before quantifiers and the
same nodes. Extract the unchanged suffix logic to AppendNode. Keep the
256-level limit; do not change runner/test stacks. Add a mixed 256-group
boundary and nested-unclosed diagnostic cases alongside existing depth tests.
Review the transitions against the former recursive parser, then rerun the
entire six-platform gate.

## Task 4: Review and verify

Review the whole diff directly. Check named branches, numbering/limits,
restoration before quantifiers, greedy/possessive suffix independence and no
matcher changes. Run git diff whitespace and tools/verify-frozen-source.sh.
Inspect all six final Staging job results/full framework counts and existing
frozen, workload and package gates. Record red/green evidence in the PR;
mark ready for the user's review and merge only after green.
