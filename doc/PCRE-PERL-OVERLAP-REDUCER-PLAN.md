# Perl overlap and reducer plan

Base `f578d1eea423e4cc7f1de3c94edbb22d2ec17ded`; 104,344 prior tests.
See [design](PCRE-PERL-OVERLAP-REDUCER-DESIGN.md). No agents, Python or local
SDK installation. Preserve prior source/tests and A05, keep #7/#102 and issue
#7 footer fixed, Grep last; user owns merge/tags/publication.

## Task 1: independent references and tests

- [x] Pin Perl release/UCD and independently compare 340 fixtures with both
  pinned native PCRE2 widths before managed assertions.
- [x] Define bounded reducer behavior and 31 tests, including native-backed
  comparison-fault reduction and ownership/cancellation/budget controls.
- [ ] Publish tests-only draft, reconcile exact successful-build RED31 and
  freeze fixture/assertion sources plus the Perl runner.

## Task 2: reducer and review

- [ ] Implement only the reducer; retain immutable oracle assertions.
- [ ] Freeze helper, review whole branch and all seven inventories, pass local
  gates and upload reviewed source/tree under a branch-head lease.

## Task 3: final qualification

- [ ] Six native104,715 full net10 / frozen214 cases /120 methods; all three
  library builds; zero failures/skips/coded warnings/errors.
- [ ] All configured support/package/script gates green; record classifier skips.
- [ ] Recheck identities/issues/reviews, preserve evidence, clean only exact
  task research and observe ready-for-review readback.

Final completion stays externally on PR282; source-plan final checkboxes stay
unchanged after GREEN. No public API, production/dependency/workflow/package
change or managed 16-bit/P01/P09-P18 closure claim.
