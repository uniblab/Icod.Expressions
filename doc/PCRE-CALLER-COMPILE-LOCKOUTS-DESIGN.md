# Caller compile restrictions

Continue the approved internal PCRE roadmap with default-false NeverUtf,
NeverUcp and NeverBackslashC caller options, matching pinned PCRE2 10.48.
A caller can prohibit a pattern from enabling UTF/UCP or using the code-unit
escape. These are compile-time policies over existing syntax and modes.

## Approach and native precedence

Use the existing uninterrupted prefix parser and original source bytes. After
settings resolution, reject effective UTF (including MatchInvalidUtf) under
NeverUtf at BodyOffset, before whole-source UTF validation. Reject effective UCP
under NeverUcp after that validation and before Turkish-casing conflict checks.
UTF restriction therefore wins over malformed UTF; malformed UTF wins over UCP
restriction; UTF wins when both restrictions conflict. Return InvalidSyntax with
the caller restriction identified in the diagnostic. Keep cancellation and the
internal invalid-work-limit guard first.

Snapshot NeverBackslashC in the parser. In the canonical C escape branch,
retain the existing invalid-bracket check first, then reject a prohibited real
escape at its opening backslash, then retain the existing unsupported UTF-C
boundary. Parse all real escapes, including DEFINE, zero repeats and unreachable
verb tails. Quoting, comments, escaped backslashes, control operands and raw
MARK names do not contain such a token. Bracket and algebra C keep their existing
InvalidSyntax diagnostic; native class error107 precedes caller error183. Ordinary
classes retain the escape offset; Perl algebra retains its expression opener.
Erased UTF-C execution remains unsupported, while native consumed UTF-C behind
is itself invalid (error136). Caller prohibitions still apply before either boundary.

This follows native compile stages without raw-source scanning, source rewriting,
new modes, bytecode or matcher state. A blanket prefix rejection would get UTF
validation/UCP precedence wrong; a raw C scan would misread inert source text.
Neither is used. Existing unsupported prefixes and UTF-C execution remain outside
this increment; no claim of complete compile-option coverage is made.

## Qualification

Pin native error numbers and native end offsets before managed compilation;
managed syntax diagnostics continue to identify the owning source byte. Accepted
native/JIT profiles pin all captures and successful labels before comparison.
Cover all eight restriction selections and eight UTF/UCP/tolerant combinations,
prefix order and conflicting malformed bodies, byte0..255, grammar contexts,
ownership, existing cancellation/work/structural quotas and legacy UTF-C rejection.
Observe successful-build tests-only RED, freeze the fixture, then change only
PcreByteOptions.cs, PcreByteCompiler.cs and PcreByteParser.cs. Require all18 native
framework results and existing supporting gates before readiness.

C#13/net8/9/10, UTF8+LF; no Python, subagents or SDK installs. Preserve all248
earlier test files, frozen35/214 outcomes/120 methods, GNU/BRE/ERE, matcher,
public API, dependencies and workflows. User owns merge/tag/publication; Grep last.
