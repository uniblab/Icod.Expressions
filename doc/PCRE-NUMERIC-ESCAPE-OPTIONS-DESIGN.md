# Numeric escape options design

## Intent and contract

After merged PR225, add internal caller options PythonOctal and NoBackslashZero,
both false by default. These map native PCRE2_EXTRA_PYTHON_OCTAL (0x2000) and
PCRE2_EXTRA_NO_BS0 (0x4000); they are compile options, not invented (*...) syntax.
Python mode makes a nonzero digit escape octal only when its first three digits
are all octal; otherwise the complete decimal run is a globally resolved
backreference, regardless of preceding capture count. Leading zero and bracket
escapes keep octal grammar. Python unbraced octals never exceed255, even in UTF;
braced o escapes keep the existing Unicode/byte range rules.

NoBackslashZero rejects only a zero-valued unbraced octal with one digit. It
rejects bare zero followed by8/9 or a quantifier, but permits two/three-digit
zeros, hex/braced octals/control-NUL, raw NUL and quoted literal text. The two
options combine independently. Original-byte InvalidSyntax diagnostics remain
at the offending backslash, following the existing numeric-error contract.
Global UTF validation still runs first. Local/reset/definition scopes cannot
change these caller choices.

## Approaches and selected implementation

Use readonly parser flags copied from PcreByteOptions. Add a constant two-digit
peek for Python three-octal recognition; otherwise reuse the existing bounded
decimal scan and global reference validation. Count consumed octal digits for
the bare-zero guard, and narrow the existing unbraced range guard under Python.
Keep typed literal/backreference nodes, widths, compilation and the matcher.
This is my implementation choice under the approved inline roadmap. Source
rewrites would move diagnostics and change quoting; separate escape parsers or
new opcodes duplicate an already shared component.

## Evidence and review focus

709 new cases include688 independently checked native PCRE2 10.48 profiles:
534 valid patterns compiled with native JIT,154 expected native compile errors.
Eight combinations cover byte/UTF and both flags; extra Perl/UTS contexts have
explicit native flags. Controls cover full digit runs, trailing digits, forward/
previous captures through256 slots, default/Python differences, all256 octets,
classes/ranges/algebra, quotes, braced forms, resets, assertions/calls, source
ownership, malformed UTF/CRLF offsets and work/cancellation/resource bounds.
Native source: pcre2_compile.c check_escape Python branch and CHAR_0 range/NO_BS0
guards; pcre2.h extra bits. The extended standalone probe sets native compile
context extra flags; it does not alter the production/native reference engine.

Review five input classes: three-digit vs full-decimal disambiguation; zero
consumption boundaries across all class/quote contexts; UTF/unbraced/braced
range and diagnostic precedence; forward/branch-reset capture semantics and
widths; owned options/source plus bounded work/cancellation. Each has tests.

All existing reference files and default behavior stay fixed. Freeze the new
file after clean-build expected RED before production. Preserve GNU/BRE/ERE,
frozen35 source blobs, frozen214/120 outcomes, exported API and dependencies.
No runtime native fallback or SDK installs. User owns merge/tag/publication;
Grep stays last. Other directives, failed labels, UTF C, wider optimizer/resources,
UTF16/public API and realistic consumer/release qualification remain open.
