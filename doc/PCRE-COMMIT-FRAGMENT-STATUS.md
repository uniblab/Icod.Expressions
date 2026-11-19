# COMMIT fragment inventory addendum

## PR289: reached root cuts and physical end anchors

Tolerant UTF under per-call native anchoring uses interpreter fragment dispatch.
A reached root COMMIT abandons the current fragment, allowing later valid
fragments. One local flag reuses the existing outer traversal; scoped/discarded
cuts, stack/journal/capture/mark cleanup and one shared work budget remain intact.
Default/compiled JIT-profile root-cut behavior is preserved. Absolute z/Z anchors
use physical input end, retaining scoped limits, while call EndAnchored keeps its
separate fragment-local acceptance check. No new VM/opcode/frame/API/quota,
source/program/subject clone, transcoding or Step site.

The unchanged original native C oracle pins52x64=3,328 complete outcomes:
1,744 matches/1,584 misses, all JIT compile0/positive size. Match-time anchors
disable native JIT execution as documented.56 additions give106,502 tests.
Observed successful-build RED32/106,470 passes/zero skips isolates28 semantic
definition failures and four later-fragment resource/view/lifetime facts after
native/default assertions.24 new controls and all106,446 prior tests pass.
Freeze new catalog/assertion/XML sources before implementation; reuse old
policy case model/consumer/C probe without changes or native-row removal.

A seventh package record requires52 definitions/3,328 protocols, preserving all
six previous results, original-view ownership checks and isolated feed/cache.
One Compile link is the sole old-test-file exception; prior C# tests/A05 stay fixed.

| Shared inventory | Increment recorded here |
|---|---|
| [Roadmap](PCRE-ROADMAP-STATUS.md) | Bounded P09 hostile-fragment conformance |
| [Recent progress](PCRE-UTF-RECENT-PROGRESS.md) | Native3,328 outcomes/56 tests and exact semantic RED32 |
| [Syntax](PCRE-SYNTAX-ACCEPTANCE-MATRIX.md) | Existing COMMIT and z/Z semantics; no grammar change |
| [Resources/errors](PCRE-RESOURCE-ERROR-CONTRACT.md) | Shared budget/cancellation, local fragment exit, existing state/layout/charges |
| [Grep fixtures](PCRE-GREP-CONSUMER-FIXTURES.md) | Seventh package record; production migration remains separate |

See [design](PCRE-COMMIT-FRAGMENT-DESIGN.md), [plan](PCRE-COMMIT-FRAGMENT-PLAN.md)
and [PR289](https://github.com/uniblab/Icod.Expressions/pull/289). Review whole
branch/baseline inventories/design/plan/addendum before final qualification.
Large syntax/resource/Grep inventories retain base blobs. Require six native
106,502/frozen214/120, library/consumer builds8/9/10, execution10 only, seven
package records and actual configured support/script gates with zero failures/
skips/coded warnings/errors. Final identities/raw metrics/results/completion/
cleanup/ready stay external without source/docs/test edits after GREEN.
#7/#102/footer fixed; broader P09, mandatory native16/P10-P18, remaining P00/P01/
Stage C and actual migrations remain open. Grep last; user merges/tags/publishes.
