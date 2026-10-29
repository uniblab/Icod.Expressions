# Internal PCRE duplicate capture names

## Intent and approach

Continue issue #7 after #85 with source-local J and duplicate named captures,
completing the currently tracked inline option families. PCRE.NET1.6.0 / PCRE2
10.48 Compiled/JIT remains the oracle. The standing autonomous remit selects
an extension of the existing numbered backreference node/opcode: immutable
ordered capture numbers plus the existing comparison policy. Separate duplicate
opcodes would repeat comparison logic; lowering to alternatives would incorrectly
retry later participating captures after a comparison fails.

## Native contract

- J permits a later name declaration to identify another capture number. It is
  local to the source: leading/mid/scoped/mixed/set-unset settings, restoration
  at group exit, propagation into later alternatives, repeated flags and unset
  precedence use the common option reader. Reset ^ preserves J, U and ASCII masks.
- Same name/same number branch-reset aliases remain valid without J. Different
  names for one number remain invalid even with J. Validation compares the first
  historical occurrence of a name with the new number before accepting repeated
  aliases: when J is off, a different first number rejects even if a later entry
  already has the new number. Preserve registration order; deduplicate slots.
- Resolve every named reference after the whole pattern is parsed. All five
  spellings, including forward references, select the first participating slot
  in declaration order. This can differ from numeric order in branch-reset groups.
  An empty capture participates. No participating capture fails. A selected text
  mismatch fails the reference without trying another slot; ordinary outer
  backtracking may subsequently change participation. Numbered references remain
  tied to their own slot. Disabling J at a reference does not change resolution.
- The reference site's i/r controls byte or UTF comparisons. Capture bytes and
  all journal rollback/assertion transactions retain their existing semantics.
- Consumed references to actual duplicate names cannot establish lookbehind
  width, even with identical finite bodies or a zero repeat. Reject at the owning
  behind opener. J with a unique name, numbered references to duplicate-named
  slots and duplicate references solely in lookahead inside behind remain valid
  under existing bounded-width rules. Global branch-reset restrictions remain.

## Representation and execution

The parser stores each name's unique numbers in declaration order. Freeze one
array per multiply defined name during resolution and share it across references;
unique names still lower to the ordinary numbered fast path. Extend
PcreByteBackreference with optional ReadOnlyMemory<int> DuplicateNumbers, and
PcreByteInstruction/Emit with optional CaptureNumbers. The Backreference opcode
uses the ordinary First slot when the memory is empty; otherwise it polls Step
for each participation lookup and then uses the unchanged single comparison path.
No new saved frames, recursion, capture copies, runtime option stack or spans in
saved state. Selection allocates nothing during matching. Each list is bounded
by the existing256 capture limit, and total unique name/slot entries are similarly
bounded. Cancellation also polls during compile-time collection and fixups.

## Verification and boundaries

Test native explicit outcomes/group counts and every numbered capture byte span:
all declarations/references; source-order and registration traps; local/reset/
negative J; empty/unset/first mismatch; alternatives, repeat rollback, forward
references, numeric controls, atomic/general assertions; byte/UTF and local i/r;
consumed behind rejection versus allowed unique/numeric/ahead controls; selected
malformed UTF/CRLF caller offsets; header/name diagnostics, depth256/capture256,
work and cancellation. Replace obsolete J-pending tests. Update current syntax,
design, resource and consumer inventories, retaining historical design context.

No public API/version/dependency/release, Grep or CommandFramework changes. Keep
instructions4096, saved paths1024 and default work1,000,000. Unicode parity,
remaining syntax, performance/release and migration remain separate gates. Local
Debug and six-platform PR Staging conventions remain unchanged. No local SDK:
CI provides observed red/green on net8/net9/net10, full plus214 frozen tests and
configured consumer/workload checks. Local whitespace and35 frozen blob checks;
UTF-8 LF, no Python.

## Pinned sources

pcre2pattern.3 1820–1880,2074–2120: scopes, reset and first-participating semantics;
pcre2_compile.c5718–5767: first-history validation and slot alias deduplication;
pcre2_compile_cgroup.c146–185: name table preserves declaration order;
pcre2_compile.c7155–7195: unique versus duplicate named references and i/r;
pcre2_compile.c9790–9822: actual duplicate-name lookbehind rejection.
Native C probes check pinned version and nonzero compiled JIT size.
