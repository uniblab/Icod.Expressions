# Word and line compile modes design

After merged PR229, add internal default-false MatchWord and MatchLine caller
options for PCRE2_EXTRA_MATCH_WORD (0x4) and MATCH_LINE (0x8). Line wins if both
are selected. Word mode is exactly an outer word boundary, noncapturing source
group and word boundary; it is not negative word-neighbor lookaround. Line mode
is the corresponding ^, noncapturing group and $ composition, including final-
newline behavior and the selected newline convention.

## Approach

My choice under the approved inline roadmap: copy two readonly options into the
parser and snapshot CurrentGroupOptions before leading inline options parse.
After successful source parsing, wrap the typed root in existing boundary/anchor
and noncapturing-group nodes using that entry snapshot. Wrap before named/call/
lookbehind resolution and final subroutine-target collection, so whole-pattern
recursion includes the wrapper. No source rewrite, new node, opcode or matcher
mode is needed; all instructions use shared work, journals and cancellation.

Native parsing emits the outer guards before source inline options. Thus (?m)
or (?-m) inside the source cannot alter line-wrapper anchors. UCP/ASCII word
policy for the outer boundaries likewise comes from entry settings, including
prefix directives, rather than inner (?aW). Implicit grouping covers all source
alternatives and establishes native cut/accept scopes without changing capture
numbers. ACCEPT may bypass the trailing guard and K may reset the reported start.

Appending rewritten source would disturb prefix/UTF rules and diagnostic bytes.
Filtering successful matches afterward would lose backtracking, capture, cut and
recursion behavior. Both alternatives are rejected here.

## Native evidence and review focus

1,543 distinct tests include 1,480 independently checked PCRE2 10.48/JIT profiles cover all four selections,
byte/UTF/UCP policies, initial versus local multiline, six newline conventions,
punctuation boundaries, alternatives, captures/references/calls/recursion,
assertions, K, ACCEPT/cuts and raw labels. Supplement with every byte, malformed-
UTF/CRLF offsets, caller-policy composition, ownership, original diagnostics,
capture numbering, canonical work equality, cancellation and existing quotas.
Precompile supplemental native patterns before managed matching. Freeze only
after valid clean-build RED with all defaults/diagnostics/native oracles passing.

Review five boundaries: entry versus inner options and Unicode word policy;
implicit alternatives/cut/ACCEPT/K scopes; wrapped whole-pattern recursion and
unchanged numbered captures; UTF/newline/start-offset behavior and diagnostics;
owned options/source and shared work/cancellation/resource bounds.

Preserve earlier tests, GNU/BRE/ERE, 35 frozen production blobs and 214 frozen
outcomes in 120 methods, exported API and dependencies. C#13/net8/9/10, UTF-8+LF.
No Python, subagents, local SDK installs or runtime fallback. User merges/tags/
publishes. Other modes/directives/callouts, failed/partial labels, UTF code-unit
escapes, wider optimizer/resources, UTF16/API and realistic consumer/release
qualification remain open. Grep stays last; these internal modes do not migrate it.
