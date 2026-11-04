# Mandatory heads in optional group bodies

Continue #7/#102 after merged PR161 (full26465/frozen214). Extend finite optional
noncapturing group bitmap study to alternatives, nested capture/noncapture/atomic
heads, and bodies with a mandatory head followed by a tail. Every branch must
reach a supported mandatory literal/type/positive predicate-free set excluding
CR. The body need not be executed to prove its starting characters; runtime
capture, tail and option-restoration semantics remain with the existing matcher.

## Approach

Reuse the existing negative-assertion mandatory-body worklist and leaf guards.
A separate group walker would duplicate the same branch/wrapper logic; general
nullable continuation inference would exceed the native evidence for this
increment. This shared proof is an implementation choice under the standing
instruction to continue without approval pauses, not a newly requested design.

Extract HasUtfCrLfMandatoryBodyBitmap(PcreByteAlternatives,ref int) from the
assertion helper, preserving its visits and cancellation. Retain PR161's exact
single-leaf group path and its charges. For the additional group shapes, prove
the body using a local visit checkpoint and commit it only on success; failed
speculation must preserve existing nullable fallback budgets. Each attempt is
bounded by1000 proof visits and checks cancellation; parser/instruction limits
also bound the AST and number of compile-time attempts. No VM state is added.

The caller still requires Minimum0/Maximum1..499 before study/multiply, charges
2*Maximum+1 plus wrappers with checked pre-addition, and sets retained=true.
Preserve direct optional-atom source/caseless guards, exact/zero paths, root-star
identity and later PLUS recovery barriers. Larger-count/deep-capture exploratory
native probes did not justify changing existing copy accounting; they are not
included in qualification totals. No inference for nullable/repeated/prefix-
assertion heads, empty branches, capturing outer groups or unbounded groups.

## Constraints and verification

C#13/net8/9/10; root solution/project,src/tests/samples,doc; UTF8+LF. No subagents,
Python,SDK/PowerShell installation. No parser/emitter/owner-finder/matcher/Step/
VM/frame/journal/API/workflow/contract/dependency/consumer/CommandFramework change
or production reference fallback. No spans/interior pointers in backtracking
frames. Limits remain parser256/captures256/instructions4096/marks256/frames1024/
work1000000/proof1000/window12. Keep35 frozen blobs and all26465 baseline cases.

Pin native full capture spans for finite modifiers, branches, nested wrappers,
tail failures, consumed prefixes, scoped casing/restoration, UCP/ASCII/property/
CR controls, root alternatives/capture dependencies and PLUS/star ownership.
Six-newline/UCP/every-offset malformed fragments, strict boundaries, work/default
budget/cancellation, count499/500 ordinary controls and expansion rejection are
required. Counted ordinary controls use native interpretation in CI for159;
bitmap-sensitive cases retain JIT. Unsupported availability is ordinary-only.

Publish tests/design/plan first, observe successful-build RED confined to the new
class with all26465 baseline cases passing, then freeze its blob and implement.
Audit final exact-head six native Staging OS/arch jobs/net8/9/10, full suite plus
frozen214/120 methods, zero failures/skips/compiler warnings/errors and all
configured gates, including seven equal Linux component workloads. Whole-branch
author review, remote exact-tree/blob checks and historical inventory/index
preservation precede ready/unmerged PR. Update only issue102's recent footer
after success. Broader7/99/100/102/native159 and release/Grep gates remain open;
publication/all known migrations precede CommandFramework removal; Grep last.
