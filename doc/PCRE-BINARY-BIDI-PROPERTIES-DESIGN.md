# Unicode binary and bidirectional properties

## Contract

Continue issue7 after PR209 with the 57 binary predicates/106 aliases and
23 Bidi_Class values recognized by PCRE2 10.48 / Unicode17. Binary names are
bare; Bidi_Class/BC accept colon/equal and short values only. Preserve native
bare bidi-prefixed names as well. General-category namespace aliases remain
restricted to categories. Other namespaces, long Bidi values, Is/block names,
Boolean assignments and InCB=value remain explicitly rejected.

Explicit binary/Bidi predicates ignore UCP, i/r and ASCII restrictions. They
support existing complements, brackets, captures, assertions/calls/references,
byte projection, UTF fragments, work and cancellation. Native InCB is a
predicate over non-None values; only its short name is recognized. Bidi_Mirrored
uses native BidiMirroring.txt membership. DerivedBidiClass @missing defaults
must apply in file order before explicit assignments, including unassigned
right-to-left/Arabic/currency ranges.

## Architecture

Reuse PR209's immutable PropertyValue metadata and existing scalar opcodes.
Add BinaryProperty/BidiClass kinds, an alias resolver and generated per-property
intervals. Lookup has source bounds checks and bounded allocation-free binary
search. No VM frames/journal/limits, runtime file, dependency or public API.
Host services and a native fallback do not provide the required owned data.

A standalone C# generator validates seven pinned input hashes, builds complete
membership, verifies a native fingerprint, and emits immutable data. Linux CI
checks exact regeneration and the Unicode notice covers all inputs. The
catalogue is the native subset, not every Unicode property or alias.

## Independent reference

PCRE2 pcre2-10.48, commit7978954dbd2efc6f2196869290553cf1871b4ce6.
For each valid scalar in ascending order, serialize the two native Boolean
uint32 words little-endian then one native Bidi class byte. The 57 binary IDs
follow the canonical-name order in the native enum; Bidi IDs are ordinal short
names AL..WS. Skip only surrogate code points. SHA256:
`774fe3490bf3f2331c3a2fe2452622efe159b700070f90ad3b08131a7597a440`.
Independent Unicode source parsing agrees for all1,112,064 valid scalars.
Native state transitions yield7,706 boundary probes, including all256 bytes.

Pin data and live JIT outcomes before implementation. Test every alias/value,
member and nonmember transitions, complements and mixed predicates, mode
independence, unassigned Bidi defaults, nested grammar, malformed UTF and
resource/diagnostic paths. Replace obsolete unfrozen binary/Bidi rejection
cases with unsupported Is/block/long-Bidi names; retain positive native parity
where a test explicitly requires native acceptance. Observe successful-build
RED, then preserve the new test/data blobs. Force LF for raw fixture data and
also parse line framing independently of checkout newlines.

C#13/net8/9/10, six native platforms; no subagents, Python, SDK or PowerShell
installation. Baseline45,893 tests; frozen214/frozen35 unchanged. Full property
catalogue does not close graphemes, class algebra, script runs, optimizer,
UTF16/API, performance, release or consumer gates. Grep remains last.
