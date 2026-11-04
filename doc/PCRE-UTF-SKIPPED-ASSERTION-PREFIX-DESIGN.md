# Skipped assertion prefixes in UTF starter-class proofs

## Intent and approach
Continue #7/#102 after merged #156. PCRE2 10.48 set_start_bits skips negative
lookahead and every lookbehind body (including positive non-atomic lookbehind)
before studying subsequent starters. detect_early_fail stops at those assertions,
so a following PLUS/star must not gain its own type_skip recovery marker.

Extend the shared FindUtfCrLfLeadingNode switch with exactly these nodes:
PcreByteLiteralAssertion when negative or behind; negative PcreByteLookahead;
all PcreByteLookbehind. Charge two visits and return retained=true. Existing
BlocksSkip prevents later PLUS recovery while allowing its established guarded
nullable traversal; general-class finite/star admission and existing earlier
root-star identity/one-emission marker remain unchanged. Skipped bodies are
opaque only to bitmap analysis. Assertions still execute/capture/validate at
runtime, including reverse bounds and fragment isolation. Cancellation per
accepted prefix and pre-addition1000 guard stay in the shared walker.

Considered a separate general-only assertion scanner or arbitrary zero-width
admission. A duplicate scanner diverges from PLUS selection; arbitrary zero
width is not bitmap availability. Positive lookahead uses a different native
rule and stays outside this addition. Chosen exact shared opcode classification
is an implementer decision under the standing instruction to continue; no new
design approval is claimed. Existing C#13/compiler architecture remains.

## Constraints and verification
Root project/solution,src/tests/samples,UTF8 LF;net8/9/10. No subagents/Python/
SDK installation. Parser256/captures256/instructions4096/marks256/frames1024/
work1m/proof1000/window12 unchanged. Preserve all24324 baseline expectations and
frozen35 production blobs. No parser/emitter helper/owner finder/matcher/Step/
VM/frame/journal/API/limit/workflow/package/dependency/consumer/CF changes.

Native-first captures for literal/general negative ahead,positive/negative/
non-atomic/variable lookbehind,opaque property/class bodies,capture dependencies,
mixed prefixes,wrappers/counts/alternatives and exclusions. PLUS greedy/lazy/
possessive and earlier-vs-later star controls pin the no-new-owner rule. Six
newlines/UCP/every-offset tolerant fragments,strict validation and boundaries,
exact/exhausted/observed work,cancellation/default budget and long ordinary
literal-negative controls. No bitmap parity claim beyond conservative1000.
Observe new-class-only successful-build RED with all24324baseline passing;
tests immutable thereafter. Six native Staging/full/frozen214/net8/9/10,
zero failures/skips/compiler warnings/errors,configured gates/seven equal
workloads;author review/exacttree/nineblobs/frozen/clean before ready unmerged PR.
Append one progress row; update only issue102 footer after CI; preserve earlier
history/index block. Keep102/unsafe100/release/Grep gates open. Publication/all
known consumer migrations precede CommandFramework removal; Grep last.
