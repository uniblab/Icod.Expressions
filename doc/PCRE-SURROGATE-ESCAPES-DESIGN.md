# Opt-in surrogate escapes

Add default-false internal `AllowSurrogateEscapes`, corresponding to PCRE2
10.48 `PCRE2_EXTRA_ALLOW_SURROGATE_ESCAPES`. Permit escaped D800..DFFF in UTF8
patterns; preserve byte-mode limits and reject values above10FFFF. Adjacent
surrogates remain separate code points. Source and subject UTF validation stay
strict, and tolerant matching continues to treat their encoded bytes as malformed
fragments. This does not introduce a NO_UTF_CHECK execution mode or UTF16 support.

## Approach

Keep the escaped integer in the existing canonical literal/range vocabulary.
Gate only the three surrogate checks in ordinary braced values and standard/extra
alternate grammar. Snapshot the caller policy in the parser; source reset options
cannot clear it. Existing numeric syntax, overflow, caller restrictions and raw
byte diagnostics keep their order.

The builder's choice is to retain code points rather than rewrite surrogates as
failure nodes or broaden subject decoding. Failure rewriting would lose literal
width, ranges, negation and quoted label identity. Broad decoding would violate
the existing validated UTF contract. Retained integer literals cannot equal a
valid subject scalar. Case folding treats surrogate code points as identity;
valid Unicode17 mappings and out-of-range guards remain unchanged. Existing
range closure already excludes the surrogate gap from valid subject membership.

Literal-only assertion optimization must fall back to existing general assertion
lowering for a surrogate literal, preserving canonical width and avoiding invalid
UTF byte assertions. A focused PcreUtf8.EncodeCodePoint helper writes the native
three-byte surrogate representation only for compiled label data and the existing
COMMIT first-code-unit metadata; other values retain Rune encoding. It never
participates in source or subject validation. Default-false behavior is unchanged.

## Qualification

Pin native-first spans, captures, success labels, compile errors/end offsets and
strict-subject errors. Cover hex/octal/N/alternate u spellings, bounds and malformed
tokens, classes/ranges/algebra/complements, empty/unreachable/repeated branches,
atomic and general assertions, nonpairing, source prefixes, case/UCP/Turkish/reset
scope, decoded/raw/inert names, caller restrictions and whole-pattern validation.
All2048 surrogate label encodings get independent native fingerprints. Selected
all-offset protocols cover six newlines and malformed fragments. Native no-match
is only -1; failed/partial labels remain outside the implemented result contract.

Add tests before production, observe clean three-framework RED, freeze the fixture,
then implement and qualify all18 native framework/platform results and configured
frozen/Unicode/component/extraction/POSIX/API/corpus/package/script gates. Include
all repository inventories before final qualification; final GREEN/review/head/
base/merge evidence belongs in PR239 and only issue7's roadmap footer.

Baseline merged238 c17ea37489f5a550f7952117db11fa1141125f2f, tree
337ea771449a3ee4f2c761958f6ef4a7f3919536;68,025 tests/framework and253 earlier
test files. Preserve GNU/BRE/ERE, frozen35/214 outcomes/120 methods, canonical
AST/IR shape/opcodes/matcher/journals/work/quotas/public API/dependencies/workflows.
C#13/net8/9/10, UTF8+LF, no Python/subagents/SDK installs. Reuse the isolated
worktree. User owns merge/tag/publication; Grep last; wider #102 remains open.

## PR239: optional surrogate escape code points

Default-false internal AllowSurrogateEscapes implements PCRE2 10.48 extra-option1
for escaped D800..DFFF in UTF8. The parser snapshots the flag and gates the three
existing surrogate checks in ordinary braced hex/octal/N and standard/extra u
grammar. Byte limits, values above10FFFF, malformed token syntax, uint overflow,
source offsets and prior caller restrictions retain their existing paths. Adjacent
surrogates remain separate code points; reset scopes do not clear caller policy.

Canonical integer literals/ranges retain widths, complements and class algebra.
Surrogate code points fold to themselves; valid Unicode17 mappings and other
out-of-range checks are unchanged. Existing class closure excludes the surrogate
gap from valid scalar membership. Literal-only assertion optimization falls back
to the existing general assertion path for non-Rune literals, avoiding invalid
byte assertions while retaining ordinary ahead/behind width and capture rules.

PcreUtf8.EncodeCodePoint writes native three-byte surrogate data only for decoded
labels and existing COMMIT first-code-unit metadata. All other values delegate
to Rune encoding. Strict source/subject TryRead is unchanged; accepting an escape
never makes raw surrogate bytes valid UTF input. Strict subjects retain errors,
and tolerant subjects retain malformed fragment barriers. NO_UTF_CHECK execution,
UTF16 and failed/partial labels remain outside this increment.

Eight production files change: PcreByteOptions.cs, PcreByteParser.cs,
PcreByteParser.AltBsux.cs, PcreByteParser.Groups.cs,
PcreByteParser.AltVerbNames.cs, PcreUtf8.cs, PcreUtf8CaseFold.cs and
PcreByteProgram.cs. No canonical AST/IR shape/opcode/matcher/journal/shared-limit/
public API/dependency/workflow changes. All253 earlier test files and frozen35/
214 outcomes/120 methods remain fixed.

The1,250 new cases include1,229 unique pinned native profiles:762 byte-span/
capture/success-label profiles,437 compile diagnostics and30 strict-subject errors.
Supplemental preflight verifies6,144 JIT0 label encodings (all2,048 surrogates in
three modes), three caller-restriction errors and2,664 selected offset protocols.
Coverage includes escape spellings and numeric bounds, classes/ranges/algebra/
negation, erased/unreachable/repeated bodies, assertions and widths, nonpairing,
casing/UCP/Turkish/reset scope, raw/decoded/inert labels, caller/UTF priority,
source/options ownership and shared work/cancellation/capture/depth/expansion.
Native error and span/label assertions precede managed comparison; only -1 is
classified as no-match. All-surrogate label fingerprint:
F268065A0C6B161BC958261B53D01F16174B8107640F79B8C5A262DAF1E0690E.

Refined tests-only [RED723](https://github.com/uniblab/Icod.Expressions/actions/runs/43402553616),
source846c85ee20f193dd02c8d857343c0666508e0c38, LinuxARM job131809003628,
built net8/9/10 without compiler warnings/errors. Each ran69,275 tests:572 expected
managed failures,68,703 passes,0skips. Failures were520 span/label profiles,
30 strict-subject profiles,3 reverse-range diagnostic owners,3 fingerprint
theories,12 offset theories and4 resource/ownership/restriction facts. Native,
default and earlier controls passed. Before valid RED, the unfrozen fixture
preserved existing outer-expression ownership for Perl/UTS algebra diagnostics;
all cases and native expectations were retained. Freeze fixture
751bd09a727ca5ea584b48527538f5d77ccddd07 before production; it is unchanged.

Qualification requires all18 native results at69,275 tests/framework and every
configured supporting gate. Repo evidence precedes the qualification commit;
final GREEN, author review and exact base/head/tree/proposed-merge identity live
on [PR239](https://github.com/uniblab/Icod.Expressions/pull/239) and issue7's roadmap
footer without another documentation-only CI run. Remaining syntax/modes/
directives/callouts, failed/partial labels, wider optimizer/resources (#102),
UTF16/exported API and realistic consumer/release gates remain open. User owns
merge/tag/publication; Grep last. See [design](PCRE-SURROGATE-ESCAPES-DESIGN.md)
and [plan](PCRE-SURROGATE-ESCAPES-PLAN.md).
