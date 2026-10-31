# Byte UCP design

## Intent and contract

Continue issue #7 after merged #113. Enable caller Ucp and exact leading (*UCP)
without UTF. Subjects/source remain bytes, including00/85/FF and UTF-looking
sequences. Each consuming character has width1. No decoder or Unicode capture-name
rules are enabled. Explicit property behavior from #113 remains independent.

UCP changes d/s/w and b/B, POSIX classes and caseless literal/range/reference/
fixed-assertion comparisons. Preserve local aD/aS/aW/aP/aT/bare-a, i/r/reset/scopes,
caller AsciiBsD and global prefix behavior. Casing uses existing simple Unicode
fold equivalence within the byte domain; no multi-character expansions or >255
source escapes. Non-UCP byte ASCII behavior and all UTF semantics stay unchanged.

## Approaches and maintainer decision

A second Unicode byte matcher would duplicate comparisons, classes and rollback.
A blanket byte-set widening would miss backreferences, boundaries and assertions.
The maintainer chooses compile-time projection through #113's shared membership,
shared POSIX-kind resolution, and reuse of existing Unicode fold equivalence.
One immutable Ucp program flag selects byte comparison semantics. There is no
new saved state, opcode or engine. This choice uses standing autonomous authority.

## Classification and source provenance

Remove only the compiler UCP-without-UTF rejection. Existing StartSettings already
resolves caller/prefix global UCP. Extract the UTF POSIX kind dispatch to a shared
CreatePosixClass(name,negated,offset) factory, keeping each reader's grammar/errors.
Byte UCP POSIX uses that kind plus existing256-value property projection; non-UCP
byte POSIX retains its existing tables/ASCII closure. Unicode lower/upper stay
Ll/Lu even under i; ASCII lower/upper widen to ASCII alpha under i.

Extend d/s/w scalar-kind selection to byte UCP and project those kinds. Local
ASCII restrictions still select their ASCII predicates. Add an optional prefixAny
argument to CreatePropertyClass: direct shorthands allow native type prefix
scanning, but bracket ASCII-restricted shorthands stay ordinary bitmaps. UCP
property-backed shorthands retain PrefixAny in bracket unions. UCP POSIX properties
also retain it; ascii and blank do not (native blank substitutes h, not a property),
and aP/aT restrictions retain bitmap provenance. Existing two-atom byte prefix
proof consumes these flags, never a blanket explicit-newline flag. Broader native
prefix/start-bitmap coverage remains #99/#102.

b/B keeps its existing per-node UnicodeWord choice. When Utf8=false classify the
adjacent byte directly with shared UnicodeWord membership; only UTF uses reverse
continuation scanning and decoding. Invalid-UTF third states remain UTF-owned.

## Casing and widths

PcreByteSet.UnicodeCaseClosure(token,restricted) enumerates at most256 input keys
using PcreUtf8CaseFold.Fold, then projects256 candidate keys into a new byte set,
polling cancellation on each visit. No scalar-domain bitmap/table duplication.
Use it for byte UCP literal/range bracket members before outer negation; existing
non-UCP ASCII closure stays unchanged. POSIX/predicate sets retain their specific
caseless policies and are not accidentally closed as literals.

Append immutable Ucp metadata to Program/ProgramCompiler, sourced from settings.
The shared byte comparison helper selects existing Unicode Equals when Ucp and
Caseless, otherwise preserves ASCII comparison. Literal, byte backreference and
fixed literal assertion paths call it with instruction r metadata. Fixed assertion
bytes remain bytes; UTF still uses existing scalar paths. Source scoping/call
annotation and all capture/journal/width paths remain unchanged.

## Diagnostics, resources and boundaries

UCP alone does not validate UTF, enable N{U+...}, admit >255 hex/octal or non-ASCII
capture names. Those remain governed by Utf8 and existing grammar. Original-byte
errors, cancellation precedence, InvalidWorkLimit/StartOffset remain. No raised
limits: depth256/captures256/instructions4096/marks256/frames1024/work1,000,000.
No pointer/span frames, snapshots, stackalloc loop or CLR-recursive traversal.

C#13; net8/net9/net10; UTF-8 LF; root project/solution, src/tests/samples.
No Python/local SDK installation. No public API/package/version/dependency/
workflow/release/consumer changes, production native/Regex fallback or engine copy.
Grep last; CommandFramework removal after publication and all known consumers.

## Evidence and delivery

Pinned PCRE.NET1.6.0 / PCRE2 10.48 Compiled/JIT: pcre2_compile.c handle_escdsw2884..2968,
posix_substitutes667..684 and parsing4089..4134; JIT scan_prefix byte PROP/XCLASS/
type versus bitmap paths. Capture-name read_name2593.. uses UTF, not UCP.

Native masks pin all256 predicate bytes and all256x256 caseless literal pairs.
Compare literal/bracket/range/ahead forms, plus managed native comparisons for
backreference/behind; literal spans pin scopes, resets, captures, calls/retries,
UTF contrast and CRLF prefix controls. Adjacent-byte word tests cover all values
against representatives in both orders, including raw continuation-looking bytes.
Every-offset/newline, grammar, projection-work, budget and cancellation controls
remain. Replace historical UCP rejection assertions with positive byte semantics, preserving
counts and non-UCP/UTF controls. Tests-only draft and observed expected RED precede
source. Final author self-review (no subagents), all six native Staging full/frozen214
net8/9/10 suites and configured gates, exact head/tree/frozen35/whitespace/clean
proof precede ready. User owns merge; preserve evidence.

First implementation run41162203194 Linuxx64/net8 passed all399 new cases;
4fail/5149pass/5153total came from two old UCP-rejection assertions repeated
by two parameter rows. Replace them with positive ASCII-restricted byte-UCP
checks; production remains unchanged. No count changes.
