# Interpreted named SKIP after a subroutine returns

[PR #297](https://github.com/uniblab/Icod.Expressions/pull/297) makes interpreted
root named-SKIP lookup see MARK history retained by completed subroutine calls.
Default/JIT lookup continues to hide that history. A still-active callee keeps
its own cut boundary. Successful atomic scopes discard their saved history.

The change uses the existing path-mark stack, capture journal, candidate
restart handling, and charged lookup. It introduces no new state arena or
opcode. Capture selection, compiled anchors, call restrictions, byte offsets,
and successful labels are exercised through the existing match-policy consumer.

The pinned native corpus contains 32 definitions across all 64 call masks,
for 2,048 outcomes. It covers ordinary, selected-capture, nested, and repeated
calls, marks at entry and return, non-ASCII names, supplementary subject
characters, compiled anchor defaults, and successful and local-cut controls.
Two additional tests check exact work exhaustion and cancellation after lookup.
The full suite reaches 106,844 cases. A fourteenth package record repeats all
2,048 protocols against the packed library.

See the [native evidence](../tests/Pcre/Native/returned-skip-oracle.md).
Observe the native-first regression failures and freeze the new catalog,
assertions, and project source link before changing the matcher. Final six-host
.NET 10 tests, .NET 8/9/10 builds, frozen results, package checks, both API
snapshots, and configured support jobs are recorded on the pull request.

This increment covers returned subroutine history visible to root lookup.
Unknown names that have no reachable MARK and interpreted assertion-local
named-SKIP propagation require separate conformance work. Broader P09 resource
and performance qualification, native UTF-16, later roadmap stages, and consumer
migrations remain open. Grep migrates last.

## Regression checkpoint

At commit e5c9ca3fdd03b38cc7cef947bf93904d19b7d74a, tree
e51f93fec3a1956cac754614028368001d2035af, Linux x64 reported 28 failures,
106,816 passes, and zero skips in
[run 44747599178](https://github.com/uniblab/Icod.Expressions/actions/runs/44747599178).
All 26 affected theory cases reached managed comparisons after their complete
native checks passed. The work and cancellation cases failed at their new
no-match assertions. All 106,810 prior tests and six new controls passed.

Freeze the catalog at Git blob c23a9a5fa6c5ad5d55b07d68e0c1f8c60ae26025,
the assertions at b81b2b19a4aa9f37bc50742a3e3c0feec735f27c, and the test
project at 552645f87c0c29588f5b7872cb6b84150282f136 before the correction.
