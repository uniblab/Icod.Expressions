# Positive lookahead before mandatory character classes

Continue issue #102 after merged PR167 (30,595 tests; frozen 214). The bounded
optional-bracket bitmap proof can cross a positive lookahead only when its
first significant continuation is a direct CR-excluding literal, a mandatory
literal repeat or a supported scalar type. A direct mandatory range class is
already a valid bitmap head for the ordinary optional-bracket proof, but the
positive-ahead pre-scan stops before it. This loses LF entry after CR under
ANYCRLF even though pinned PCRE2 10.48 retains it.

Admit a direct `PcreByteScalarSet` in that pre-scan only when the existing
`HasUtfCrLfClassBitmap` predicate proves all ranges valid and CR-excluding,
with no negation or predicates. Probe with a copy of the visit count: the
mandatory leaf owns the actual range visits, and an unsuccessful speculative
probe must not spend the bounded proof budget. Preserve the iterative
all-branch proof, cancellation, limits, skipped assertion/boundary prefix,
success-only visit checkpoint, and runtime semantics. Optional classes,
CR-containing/negated/property classes and other unsupported heads remain on
their established paths.

Pinned native PCRE2 10.48 with JIT enabled reports LF present, CR absent and
positive JIT size for direct ASCII and mixed-width range classes after a
positive lookahead, in optional capture/noncapture/atomic and finite lazy
wrappers and in two-branch alternatives. Pin all capture spans for CRLF and
ordinary subjects in strict and tolerant UTF. Commit tests first and observe
a successful-build RED confined to the new class before production changes.
Then audit six native Staging OS/architecture jobs on net8/9/10, full suite,
frozen214 and all configured gates. Keep issue #102 open; no release, consumer
migration, CommandFramework removal or Grep readiness is implied.
