# UTF-8 invalid-subject implementation plan

1. Add `PcreUtf8InvalidSubjectTests` for valid islands, malformed UTF classes, negated atoms, captures/backreferences, fragment-edge assertions, offsets and limits. Preserve explicit strict-subject tests. Run the Staging PR matrix and observe expected red for the new option.
2. Add the internal `MatchInvalidUtf` option and make it imply UTF-8 in `PcreByteCompiler`. Carry it on `PcreByteProgram`. Reject invalid patterns as before.
3. Scan the subject in `PcreByteMatcher` into valid UTF fragments within the existing work budget; search candidates per fragment. Guard consuming instructions and backreferences against fragment ends. Retain one edge per invalid run plus the subject end, and allow the empty pattern at the requested byte offset. Use fragment context for `\b`/`\B`, retaining whole-subject line and `\z`/`\Z` anchors. The later pinned PCRE2 10.48 offset matrix corrected the original fragment-edge anchor assumption.
4. Update `PCRE-DESIGN.md` coverage and caveats. Run all six Staging jobs, frozen GNU tests, and package smoke. Request independent review, fix material findings, rerun verification, then mark the PR ready.
