# Unicode script and script-extension properties

## Scope and reference

Continue issue7 after PR208 with all 175 PCRE2 10.48 scripts and 344 normalized
long/short/legacy aliases from Unicode17. Exclude Hrkt/Katakana_Or_Hiragana,
which has no corresponding PCRE2 script. Bare names mean scx; sc/Script and
scx/Script_Extensions accept colon/equal separators and existing loose names.
P/caret/class complements retain existing parsing. Explicit script properties
are independent of UCP, i/r and ASCII restrictions, including byte projection.
Binary properties and script-run groups remain separate pending families.

PCRE2's scx matching includes the primary script as well as extension bits.
In particular Common/Inherited retain primary membership even where the Unicode
extension file names other scripts. Pin that native behavior rather than
substituting an interpretation based only on the source extension list.

## Architecture and alternatives

Add Script and ScriptExtensions scalar kinds with an explicit immutable
PropertyValue ID in AST classes/predicates and compiled class instructions.
Thread it through existing class dispatch and byte projection; reuse the
ScalarClass/ScalarSet opcodes, scalar widths, captures/journal and Step paths.
Do not overload UnicodeCategory or encode IDs as undocumented enum arithmetic.
Generate per-script sorted interval tables and offsets for primary and extended
membership; match-time lookup is allocation-free bounded binary search with
source bounds checks. Alias lookup occurs only at compile time.

A standalone C# generator validates Scripts.txt, ScriptExtensions.txt and
PropertyValueAliases.txt SHA256 hashes, checks all scalar membership against a
native fingerprint, then emits immutable data. Linux PR/main CI verifies exact
regeneration. Extend the packaged Unicode notice. Native fallback violates the
engine contract; host Unicode services do not supply the required pinned data.

## Evidence and tests

Use PCRE2 tag pcre2-10.48, commit7978954dbd2efc6f2196869290553cf1871b4ce6.
Assign script IDs by ordinal ISO four-letter code, excluding Hrkt. For each of
1,112,064 valid scalars in ascending order, serialize the primary ID byte then
22 little-bit-order bytes of scx membership (primary OR native extension bits).
Native UCD_SCRIPT / UCD_SCRIPTX produces SHA256
`f6e14157525f8aa69f6bd1564ec306717bcd6992b54280b92a105904f1868b82`.
Independent parsing of all three Unicode source files agrees at every scalar.

The native state transitions yield 3,508 boundary probes, including all256 byte
values and all175 script representatives. Store these in test-only fixture data;
compare all primary/extended members and selected nonmembers against JIT and
managed full capture spans. Cover every alias/form, byte/UCP domains, mixed
predicates, complements, i/r/ASCII independence, calls/assertions/backreferences,
CRLF, malformed-UTF offsets, work/cancellation and source-byte diagnostics.

Replace old unfrozen pending-Greek negative cases with distinct still-pending
binary/Bidi properties before RED; do not remove negative coverage. Preserve
all updated/new test blobs after successful-build RED. Baseline45,846 cases and
frozen214/frozen35 remain. C#13/net8/9/10; six native OS/architectures. No
subagents, Python, SDK or PowerShell installation; managed execution uses CI.
No public API, version bump, publication or consumer migration; Grep stays last.

Native JIT research verified 7,092 alias/literal and 1,227,800 complete boundary
outcomes. Corrected test-only RED626 built and observed42 new failures/45,851
passes on Linux ARM64/net8; all45,846 baseline cases passed. Test blobs:
reference292afaff1801a69b607060b7652e675fadc93211;
fixture07fba8b99edb62113858f6d49c9b035846020054;
bracket8fc864b00b32600e60643e1eeb574decccdee2ce;
foundationa47b704a9735b699566603da761d7672cfc360da;
bytepropertyc407f3adee8ab4960927db1daf0bc4d7739db318;
acceptancedb621d9550da07e17e970bb3a0205a3f641d7061.
