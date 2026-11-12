# Caller ASCII character-type policies

Continue after merged PR230 with internal default-false AsciiBsS, AsciiBsW,
AsciiPosix and AsciiDigit. Native extra bits are 0x200,0x400,0x800,0x1000;
AsciiBsD already supplies0x100. Initialize the existing five-bit parser mask.
AsciiPosix sets only Posix; AsciiDigit sets only PosixDigit. Unlike the source
option aP, the caller Posix bit must not also seed PosixDigit: subsequent source
unsets expose the difference. Reset keeps all restrictions, and source scopes,
alternative propagation, explicit set/unset and restoration remain authoritative.

## Approach

My choice under the approved inline roadmap is four copied primitive options
and initialization of the existing mask. Runtime flags would duplicate existing
predicate lowering and source rewrites would corrupt diagnostics, prefixes and
entry-policy word guards. Both alternatives are rejected. No new node, opcode,
matcher path, dependency, exported API or consumer migration is needed.

Restricted backslash s/S and w/W use canonical ASCII predicates; w also controls
b/B and MatchWord's entry guards. Posix restricts all ordinary POSIX classes;
Digit restricts digit/xdigit alone. They remain independent of backslash types,
explicit Unicode properties, h/v/R and literal/range case folding. UTF/UCP and
byte modes retain their existing domains. Existing Perl/UTS class algebra,
lookbehind width, captures/calls/cuts, work/cancellation and quotas own semantics.

## Verification and review boundaries

Pin all32 combinations of the five flags in byte/UTF with/without UCP. Native
PCRE2 10.48/JIT profiles include all type predicates, boundaries, POSIX digits,
local unsets/restoration, aP/aT overlap/reset, assertions, references/calls, class
algebra, folding, raw labels and word/line wrappers. Supplement every POSIX name,
all octets, malformed-UTF/CRLF start offsets, old caller policies, source/options
ownership, exact legacy diagnostics, canonical work, cancellation and limits.
Precompile all supplemental native patterns before managed comparisons. Require
clean-build expected tests-only RED before freezing the new fixture, then full
18-result qualification at zero failures/skips/build warnings/errors.

Five review boundaries: Posix/Digit overlap and partial unsets; outer MatchWord
entry policy versus body aW; ordinary/algebra classes and unrelated folding;
UTF/UCP byte domains, offsets and diagnostics; owned settings and shared bounds.

Preserve all earlier tests, 35 frozen production blobs and214 frozen outcomes in
120 methods, GNU/BRE/ERE and API/dependencies. C#13/net8/9/10, UTF-8+LF, no Python,
subagents or SDK installs. User owns merges/tags/publication. Other modes/syntax,
optimizer/resources, UTF16/API and consumer/release gates remain open; Grep last.
