# Perl overlap development oracle

[perl-overlap.pl](perl-overlap.pl) is an original development-only runner for
Perl v5.40.1 / Unicode 15.0.0. Release sources:

- [tag v5.40.1](https://github.com/Perl/perl5/tree/v5.40.1), commit
  `f722f11a2b1a34e28743e331bad6dd7da41e2086`.
- [reference semantics](https://github.com/Perl/perl5/blob/v5.40.1/pod/perlre.pod).

No Perl code/test data is imported. CI needs no new interpreter dependency:
frozen author-designed expectations execute through the existing PCRE.NET
native widths, direct native16 bridge and managed byte matcher. Re-run the
independent Perl preflight only with the pinned release/UCD; the runner rejects
other versions. The original reference binary/hash and narrow comparison scope
are in [design](../../../doc/PCRE-PERL-OVERLAP-REDUCER-DESIGN.md).

Reconstruct LF input and expected output from the frozen C# catalog, using Node:

```sh
node <<'JS'
const fs = require('fs');
const source = fs.readFileSync('tests/Pcre/PcrePerlOverlapCases.cs', 'utf8');
const rows = [...source.matchAll(/yield return new object\[\] \{ (.*) \};/g)]
  .map(match => JSON.parse('[' + match[1] + ']'));
const inputs = rows.map(row => {
  const subject = Buffer.from(row[3], 'hex').toString('utf8');
  const scalarStart = Array.from(subject.slice(0, row[7])).length;
  return [row[0], row[1], row[3], scalarStart].join('|');
});
const expected = rows.map(row => [row[0], row[10] ? 'M' : 'N', row[10]].join('|'));
fs.writeFileSync('perl-input.txt', inputs.join('\n') + '\n');
fs.writeFileSync('perl-expected.txt', expected.join('\n') + '\n');
JS
perl tests/Pcre/Native/perl-overlap.pl < perl-input.txt > perl-actual.txt
cmp perl-expected.txt perl-actual.txt
```

Each input is `id|UTF8-pattern-hex|UTF8-subject-hex|scalar-start-offset`.
Empty hex is empty input. Each match output is `id|M|scalar-start:length,...`,
including trailing `-` unset captures; no-match is `id|N|`. A fresh original
subject receives Perl /g pos per row, preserving lookbehind/search-start
context. No subject slice or next-match iteration is involved. Compile errors,
invalid UTF/rows and timeout are process failures, never no-match records.

The runner bounds rows/line length/decoded lengths and uses a five-second
per-row alarm for controlled fixture development. It is not a general service
for untrusted regex execution. Only valid single-match numbered-capture
semantics on the selected Unicode literals/ASCII grammar form the overlap;
UCD-dependent differences and the wider Perl/PCRE feature families remain
outside the declared comparison.

340 rows (221 M /119 N) agree with all680 native8/native16 probes from the
pinned [C oracle](README.md), with independent scalar normalization and JIT
zero/positive size. Literal Perl SHA256:
`d38d9a3c75ae9acbf44e402294daa13747e01584e61bec2e3b463f9b2b9b81bf`.
See the design for the paired native digest and resource/reducer boundaries.
