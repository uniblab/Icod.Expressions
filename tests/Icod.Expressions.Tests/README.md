# Frozen extraction tests

The eleven C# files in `RegularExpressions/` are byte-identical to the
focused corpus frozen by `Icod.CommandFramework` A05. The adjacent historical
README is copied unchanged as provenance. The original namespace and using
directives are also unchanged; these are inherited behavior assertions, not
strict POSIX conformance claims.

This project is intentionally outside `Icod.Expressions.sln` until E01 copies
the engine and the two external Text enums. Before that copy, compiling the
project for `net10.0` fails on missing regex/Text namespaces. After E01,
only mechanical namespace/project changes may be applied; test expectations
and the JSONL parity fixture must retain their A05 outcomes. See
`../../doc/EXTRACTION-READINESS.md` and `../../tools/verify-frozen-corpus.sh`.
