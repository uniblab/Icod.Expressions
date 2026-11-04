# Optional assertion bitmap heads

Continue #7/#102 after merged #158 and CI #160. The selected increment completes
bounded optional negative-ahead study for mandatory literal/type heads and
mandatory singleton folded classes. Grep migration/release and #99/#100/#159
remain separate. No native/System.Text.RegularExpressions production fallback.

## Native contract and choices

BRAZERO enters the assertion body, so every studied branch must reach a known
mandatory bitmap head. Required exact assertion copies still skip their bodies.
Admitting all nullable bodies would infer unproven continuations; rejecting all
non-class heads leaves native-confirmed gaps. Extend the existing bounded
worklist and share its leaf classification instead of introducing a second
walker or copying an engine. This is an implementation choice under the standing
instruction to continue development without further design approval pauses.

Mandatory literals, including CHAR/CHARI and mandatory PT_CLIST, support study.
Their optional TYPEQUERY/PT_CLIST counterparts have different rules: preserve
all existing optional-atom source/caseless guards. A mandatory literal must be a
valid scalar excluding CR; collapsed literal assertions decode only their first
UTF scalar through the existing PcreUtf8.TryRead. The four supported type cases
are positive AsciiDigit/AsciiWord/HorizontalSpace and negative AsciiSpace. UCP
properties and scoped ASCII restrictions remain distinguished by parsed Kind.
Mandatory positive predicate-free sets require valid nonempty ranges excluding
CR; singleton source/caseless query guards do not apply to a mandatory head.

General negative lookahead must prove every branch through existing capture,
noncapture and atomic groups. No empty/nullable/repeated-head, body-tail return,
word-boundary-prefix, positive-ahead or lookbehind admission is added. Collapsed
negative literal ahead is nonempty. Existing class-body shapes remain admitted.

Keep Maximum>0, Minimum!=Maximum and Maximum<=(1000-1)/2 before study/multiply;
charge2*Maximum+1 and retained=true, preserving pre-addition and wrapper charges.
The worklist shares ref visits, charges pops/branches/ranges and checks
cancellation. Keep root-star identities and later PLUS/star recovery barriers.
No parser/emitter/owner finder/matcher/Step/frame/journal/API/resource limit change.
Compile-time AST references only; no spans or interior pointers in VM frames.

## Verification and constraints

C#13/net8/9/10; root project/solution,src/tests/samples,doc. No subagents,Python,
SDK installation or PowerShell installation. Keep35 frozen production blobs,
all25373 baseline test expectations and214 frozen tests unchanged. Bounds remain
parser256/captures256/instructions4096/marks256/frames1024/work1m/proof1000/window12.

Native first: pin full literal capture spans across counts/modifiers, collapsed
and general heads, grouped alternatives, rollback/capture dependencies, scoped
UCP/ASCII/caseless controls, root optional-atom pairs, mixed wrappers and PLUS/
star ownership. Six-newline/UCP/every-offset malformed fragments, strict caller
boundaries and work/cancellation/default-budget controls remain. Ordinary
out-of-proof-limit controls use the native interpreter where needed for #159;
no bitmap parity claim beyond1000. Preserve tests after observed successful-build
RED and retain every baseline expectation. Run the merged concurrent CI: all six
native Staging jobs/net8/9/10, all full results/frozen214 subset identities,
zero failures/skips/compiler warnings/errors, all configured gates and seven
equal Linux workloads. Author whole-branch review and remote exact-tree checks
before ready/unmerged PR. No merge/tag/publication/consumer/CommandFramework work.
Preserve issue102 history/index; append one progress row and update only its
recent footer after all final gates. Broader availability remains open.

## Scoped-group correction after observed RED

The first tests-only run compiled cleanly and failed555/26062 tests per TFM;
all25373 baseline cases passed. The initial assertion proof resolved545 failures.
The ten remaining original controls use optional scoped groups, e.g. (?i:s)?,
whose mandatory inner CHAR/PROP head differs from direct optional (?i)s?.
Preserve the original test blob and add403 focused cases before this correction.
Native qualification adds756 literal,4248 protocol and16 resource checks.

Admit only finite optional noncapturing groups with exactly one branch and one
mandatory literal/type/set leaf. Check Minimum=0 and Maximum<=499 before study
or multiplication. Reuse the assertion mandatory-head classifier; charge the
group/body and existing range visits, plus2*Maximum+1 and wrapper costs. Reject
unsupported shapes before changing the shared visit counter, preserving existing
nullable-repeat fallback budgets. No nested/capturing/empty/nullable/tail-return
or unbounded-group expansion is added. Direct optional-atom guards remain exact.

Focused tests pair scoped and global casing, consumed prefixes and option
restoration, type/UCP/ASCII/property/CR controls, PLUS/star/wrapper ownership,
all caller offsets and malformed fragments, strict validation and resource
contracts. Expected final full suite26465, including1092 new cases. Keep both
new test blobs immutable after their successful-build RED observations.
