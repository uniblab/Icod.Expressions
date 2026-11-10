# Pinned grapheme-cluster matching

## Contract and scope

Continue issue7 after PR210 with `\X` in byte/UTF8 mode. A cluster consumes
at least one scalar and is atomic internally; outer repeats backtrack by
whole clusters. Preserve captures/references/calls/assertions, original byte
spans, UCP/i/ASCII/newline independence, malformed-UTF fragment boundaries,
work accounting and cancellation. Bracket `\X` stays invalid; consuming
lookbehind use remains unsupported because cluster width is unbounded.
Forward assertions containing `\X` are zero-width for lookbehind analysis.
No class algebra, script-run, API/version/publication or migration work.

The reference is PCRE2 10.48, commit7978954dbd2efc6f2196869290553cf1871b4ce6,
Unicode17. Match its actual segmentation, including RI context before the
requested start, EP/Extend/ZWJ state and native table exceptions (RI before
Extend and Prepend before EP). It does not implement newer Indic-conjunct
GB9c segmentation; do not substitute host StringInfo/UAX behavior. Pin these
native outcomes explicitly rather than claiming unrestricted UAX conformance.

## Approach

Use a dedicated consuming AST node/opcode, with an allocation-free bounded
cluster scanner and existing outer repeat/capture/VM machinery. Expanding
clusters into ordinary regex nodes would obscure atomicity and state; host
segmentation would reintroduce Unicode/runtime drift. This is the author’s
implementation choice under the standing autonomous roadmap approval.

Add immutable 15-way break classification from GraphemeBreakProperty.txt and
Extended_Pictographic in emoji-data.txt, using checked C# generation and Linux
exact regeneration. Fingerprint one native break byte for each valid scalar,
ascending, skipping surrogates only:
`fe5b7e8e4feb352ea6f9ef976370bfcbb00b52a0466e32b0bc274d0f3b5d658d`.
Source hashes are d6b51d1d2ae5c33b451b7ed994b48f1f4dc62b2272a5831e7fd418514a6bae89
and2cb2bb9455cda83e8481541ecf5b6dfda66a3bb89efa3fa7c5297eccf607b72b.
Independent source parsing agrees with native for all1,112,064 valid scalars.
No PCRE source implementation is copied; data derives from Unicode inputs.

The scanner uses Step for each attempted scalar and each preceding RI inspected.
A per-match, input-derived contiguous RI-run cache avoids quadratic rescanning;
its two byte offsets are memoized facts, not backtracked semantic state. Scope
limits and validated fragment starts bound reads. The shared Step primitive receives the work counter by reference; no Step
delegate or per-atom allocation is needed. The cache does not live on the
compiled reusable pattern. Invalid gaps reset usable
context; start offsets inside valid clusters remain authoritative. Audit
assertion end limits where zero-width lookaheads are nested in lookbehind.
Native fixed-width behind allows the forward assertion to see following text;
native variable behind bounds it at the consuming end. Preserve both, including
negative-assertion outcomes; the existing scope rules need no correction.

## Evidence and review

Before implementation, publish 3,535 native classification boundary points,
all225 type pairs and3,375 triples at every scalar start (222,675 standalone
JIT/capture outcomes), all65,536 byte pairs, grammar/atomicity, RI prefix/cache,
malformed UTF, lookbehind diagnostics and long-cluster work/cancellation tests.
Observe successful-build RED and preserve new test/data blobs. Then require
all18 native framework pairs, full applicable compatibility, package/API,
extraction, workloads and frozen35/frozen214; author review per no-subagents
preference. C#13/net8/9/10, no runtime fallback/dependency, Python, SDK or
PowerShell installation. Class algebra is next; Grep stays last.
