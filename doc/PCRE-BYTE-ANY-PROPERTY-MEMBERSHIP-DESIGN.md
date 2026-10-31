# Internal PCRE byte mixed-Any property membership

## Intent and pinned cause

Continue #7/#99 after #120. Resolve the mixed-property membership discrepancy
documented there before broadening bitmap search. PCRE.NET1.6.0 / PCRE2 10.48
byte compilation can omit a bitmap filled by positive Any when the bracket has
actual property data but no ordinary byte operand. The retained property union
then determines membership. This is counterintuitive, version-pinned behavior;
it is not a general character-set algebra rule.

Native byte `[\p{Any}\p{L}]` matches A but not !, in either property order;
`[^\p{Any}\p{L}]` instead matches ! but not A. Adding literal A or a bitmap
operand retains the full bitmap. UCP converts some shorthands/POSIX operands
to actual properties; local ASCII restrictions can restore bitmap operands.
UTF keeps its existing scalar union and is not affected by this byte-only rule.
Empty quotes and xx trivia contribute no byte operand. Nonempty quotes/ranges do.

Authority: pinned pcre2_compile_class.c PT_ANY1449..1456; real property emission
1459..1469; ordinary escape bitmap flag1477; POSIX flag1336; literal/range flags;
XCLASS_REQUIRED1713 and bitmap inclusion1729..1755. Native JIT/interpreter agree.

## Approach and contract

Alternatives: retain mathematical union as an intentional native deviation,
change runtime class evaluation, or reproduce the pinned compile-time membership
using #120's source provenance. Choose the third under standing implementation
authority because Grep compatibility targets this pinned implementation. Cost:
valid mixed-Any expressions follow native10.48's unusual behavior; revisit this
contract when the reference version changes. The safety deviation in #100 remains.

In byte ParseBracket, retain the ordinary union and add a property-only union
plus an ordinary bitmap-source flag. Each actual XclassProperty operand adds its
projected byte set to the property union. Each item without PrefixAny marks an
ordinary bitmap source. AnyProperty contributes neither; its full/empty byte set
still participates in the ordinary union. Literal/range/quote/ASCII/POSIX handling
already identifies these sources. Ignored trivia and empty quotes are not items.

At bracket close, when actual property data exists and no ordinary bitmap source
exists, select the property-only set; otherwise keep the ordinary union. Apply
outer negation afterward. Preserve the existing atom/exact provenance calculation,
source-newline flags, case closure, grammar, offsets and diagnostics. UTF brackets
use their existing separate scalar representation. No AST/width/compiler proof,
matcher/VM/state/journal/resource change. Fixed-size byte sets keep compilation
storage bounded; existing per-item/property cancellation remains in force.

## Scope and constraints

This resolves the named mixed-Any membership case only, not #99 overall. Ordinary
bitmap/coupled positive-interior source flags, broader optimizer selection,
variable/zero repeats and multi-literal/alternative/assertion/call/reset prefixes
remain. UTF search #102 and unsafe native #100 remain separate. Known broader
misses/false positives still exist; no full PCRE/public readiness claim.

Internal C#13/net8/net9/net10, UTF8 LF, root project/solution, src/tests/samples.
No subagents/Python/SDK installation, public/package/version/dependency/workflow/
release/consumer changes, native/Regex production fallback, engine clones, saved
spans/pointers/snapshots or raised limits. Depth256/captures256/instructions4096/
marks256/frames1024/default work1,000,000; proof1000/window12 unchanged. Frozen35
intact. Grep last; CommandFramework removal requires publication and migration
of all known consumers. User owns merge.

## Verification / builder check

Native-first anchored membership over all256 bytes, UCP/case/ASCII/xx policies,
literal/escape/POSIX/range/quoted source order and complements; full capture
fixtures with repeats/references/lookaround/atomic/groups and CRLF. All offsets
over six newline conventions and supported UTF/fragment controls. Work/exhaustion/
cancellation/depth/repeat limits shared. Successful-build tests-only RED before
production; all5566 baseline cases pass. Six native Staging platforms/net8/9/10
full/frozen214/configured gates green, zero failures/skips/build warnings/errors.
Separate whole-branch author review, exact head/tree/log/frozen/whitespace/clean
audit. Builder/interface check: existing tuple fields supply both aggregates;
only final byte set changes, then existing class lowering consumes it unchanged.
