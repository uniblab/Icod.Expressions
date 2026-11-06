# Positive lookahead before mandatory group repeat heads

Continue issue #102 after merged PR171. Native PCRE2 10.48 preserves LF
after CR in the ANYCRLF first bitmap when a positive lookahead in an
optional bracket precedes one unquantified capture, noncapture or atomic
noncapture group whose branches start with mandatory positive-minimum
repeats. A bounded repeat can consume multiple characters; the bitmap
proof only needs its first character.

Admit literal, supported range-class and supported scalar-type repeat
heads, including possessive wrappers, only when every branch begins
immediately with an already proven CR-excluding repeat. Reuse the
existing repeat predicates and the group helper's bounded branch proof;
the speculative pre-scan uses a copy of the visit count. The iterative
mandatory-body traversal still owns its actual charges. Nullable heads,
empty branches, nested or repeated groups and unsupported heads remain
conservative. Preserve assertion and capture execution, parser, VM and
public API. Do not alter production consumers or CommandFramework.

Native JIT probes show LF present, CR absent and positive JIT size for
literal, class and type repeats in groups, a possessive head, an atomic
group and mixed repeat alternatives. Pin strict and tolerant full capture
spans on CRLF and ordinary subjects. Commit tests first and observe RED
confined to the new fixtures, then run six native Staging OS/architecture
jobs over net8/9/10, frozen 214, and configured gates without test skips,
failures, warnings or errors. Leave issue #102 open; Grep remains last.
