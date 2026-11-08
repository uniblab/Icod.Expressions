# Leading negative assertions before nullable group repeats

## Native evidence and intent

After merged #194, probe pinned PCRE2 10.48 for negative assertions
before the existing nullable repeated-group and CR-excluding final class.
Sixty patterns across four leading negative assertions, three middle
prefixes and five group shapes have LF set, CR clear in the native first
bitmap, positive JIT size, and expected CRLF and ordinary match offsets.

## Scope

The leading `(?<!\r)` and `(?!\n)` assertions fail at the LF in CRLF.
Their optional group falls through to the mandatory final class, which
admits LF. The controls `(?<!B)` and `(?!B)` can succeed there and must
retain conservative handling. Establish RED first with 120 full-match
and capture fixtures / 240 strict and tolerant checks. If confirmed,
extend only the existing bounded optional-group proof for the two
caseful, single-scalar failing assertions. Keep source, caller offset,
capture, cancellation and 1,000-visit guards. Parser, matcher, API,
limits and consumers are outside this change.
