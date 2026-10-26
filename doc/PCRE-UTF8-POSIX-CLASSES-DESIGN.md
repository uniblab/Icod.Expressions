# UTF-8 POSIX bracket classes

**Status:** internal PCRE syntax slice under issue #7.

## Intent and boundaries

Complete the fourteen ordinary POSIX bracket names already available in byte mode for the internal UTF-8 path. Preserve the existing single-scalar bracket union, source-byte captures, invalid-UTF fragment barriers, bounded work and cancellation. There is no public PCRE capability claim, Grep migration, or CommandFramework production change. Caseless, scoped `(?aP)`/`(?aT)` options, other properties, and pinned Unicode-version parity remain separate gates.

## Approaches

| Approach | Tradeoff | Decision |
|---|---|---|
| Expand each class into Unicode code-point ranges during compilation | Large tables tied to one Unicode release. | Reject. |
| Interpret the POSIX name in the matcher | Repeated string dispatch and option leakage into execution. | Reject. |
| Map each name to a scalar predicate at compilation | Fits the existing bracket-set union, with one charged predicate check per attempt. | Choose. |

## Semantics

Without UCP, names use the established ASCII `PcreByteSet` definitions, so scalars above U+007F fail a positive class. With UCP, `alnum` is Unicode L or N, `alpha` is L, `blank` is PCRE2's fixed `\h` list, `cntrl` is Cc, `digit` is Nd, `lower` is Ll, `upper` is Lu, `space` and `word` keep their existing semantics. `graph` is L/M/N/P/S/Cf, excluding U+061C, U+180E, and U+2066–U+2069; `print` adds Zs; `punct` is P plus S below U+0100; `xdigit` is ASCII hex plus fullwidth counterparts. `ascii` remains U+0000–U+007F. Inner `[:^name:]` complements one predicate; outer `[^...]` complements the union. Positive and negative classes consume only one valid scalar. These mappings follow [PCRE2's POSIX class specification](https://www.pcre.org/current/doc/html/pcre2pattern.html#SEC14); .NET's Unicode category data can differ by runtime and host, so version-wide PCRE2 conformance stays pending.

## Verification

Characterize each newly supported name in UCP and non-UCP modes, including non-ASCII boundaries and the PCRE2 exceptions above. Exercise nested unions, both negations, byte spans, malformed subject barriers, invalid range endpoints, diagnostics for unknown names, work limits and cancellation. Keep byte-mode behavior and the frozen GNU corpus green across Staging's six OS/architecture jobs and three .NET targets.
