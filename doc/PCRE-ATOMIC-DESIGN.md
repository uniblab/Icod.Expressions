# Internal PCRE atomic groups and possessive repetition

Historical tranche boundary: exact atomic-group and atomic-lookaround aliases now
share these existing semantic paths; see `PCRE-GROUP-ALIASES-DESIGN.md`. Earlier
alias-pending notes below describe the original tranche.

**Status:** implementation tranche under issue #7. No public API or consumer migration.

## Intent and scope

Support `(?>...)` as a noncapturing group and possessive `*+`, `++`, `?+`, and `{n,m}+` forms on the already supported atoms and groups. A successful atomic section commits to its first ordered path: later failure cannot revisit alternatives or repetition decisions made inside it, but may revisit choices made before it. Captures inside it remain visible on success and roll back if an outer path fails. Support byte and UTF modes, with the existing malformed UTF fragment barriers and resource limits. Lazy/possessive combinations remain invalid.

## Approaches

The chosen approach compiles an atomic section to enter/exit instructions around its existing bytecode. Enter records the current backtracking stack depth; exit discards only frames created since entry. Backtracking restores the active atomic boundary stack alongside capture and repeat state. This retains the ordered matcher and its limits. A second interpreter for atomic bodies would duplicate matching and complicate captures and work accounting. Rewriting possessive repetition into a separate optimized loop would require parallel semantics for empty matches and counted forms. Compile possessive repetition as an atomic wrapper over the ordinary greedy repeat for this first tranche.

## Verification

Compare selected spans with the pinned PCRE.NET test oracle and assert captures locally. Cases include nested atomic sections, preceding alternatives, inner failure before exit, first-path alternation, optional and counted possessive forms, empty-body repetition, high bytes and UTF scalars, and work/cancellation accounting. Keep unsupported special groups explicit. The existing six Staging platform jobs, frozen corpus and package smoke remain gates. Full PCRE2 conformance and performance thresholds remain pending.
