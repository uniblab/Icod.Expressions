# UTS#18 Class Algebra Implementation Plan

Goal: opt-in PCRE2 10.48 UTS#18 grammar using the shared class evaluator.
Architecture: bounded heap parser frames, contiguous raw leaves and postfix
operations; existing bitmap projection/UTF work and width machinery.
Spec: doc/PCRE-UTS-CLASS-ALGEBRA-DESIGN.md. Execute inline, no subagents.

## Global constraints and review focus

C#13/net8/9/10, UTF8+LF. No Python/SDK/PowerShell installation or new package.
Preserve frozen35/frozen214/120, GNU APIs and all fixed PR212 test/data blobs.
Native reference only; no runtime fallback. Default grammar unchanged. User
owns merge/tag/publication, Grep last. Check implicit-run grouping versus nested
union, whole-class complement, operator mixing, quote/range boundaries, local
xx scope and Perl leaf isolation; verify depth/flat/shared budgets and match
cancellation at every term. Keep generic optimizer gaps scoped to issue102.

1. Publish PcreUtsClassAlgebraReferenceTests.cs and fixed native mask fixture
   in a tests-only draft. Reflection sets the absent new option so the RED
   commit compiles successfully. Require RED with all46069 baseline passes,
   then freeze new test/data blobs.
2. Add internal AltExtendedClass option and parser configuration; implement
   PcreByteParser.UtsClass.cs frontend over existing terms and leaf readers.
   Reuse bounded projection/evaluation; preserve ordinary no-algebra readers.
   Require full applicable CI GREEN; fix production against unchanged tests.
3. Update syntax/design/resource/fixture inventories and current roadmap.
   Author-review complete diff and exact proposed merge tree. Verify18 native
   result rows, frozen/extraction/API/package/workloads, five Unicode
   regenerators, build scripts and all applicable compatibility jobs. Record
   evidence, mark ready, preserve the branch for user merge.

## Execution evidence

Test-only3aa3121 / RED639 run42926866683, Linux x64 job130215440647:
all frameworks compile with0warnings/errors; 46127 cases,58 expected failures
from the absent AltExtendedClass option,46069 baseline passes,0skips.
Fixed test blob997476f58dc8e310a42c9a39d0e1107df87d6060;
fixture1f9af6684d32ecd7c9ad518e890cf58ed1073ea5. Both remain unchanged.
