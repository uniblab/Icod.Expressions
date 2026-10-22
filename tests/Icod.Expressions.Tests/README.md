# Frozen extraction tests

The eleven C# files in `RegularExpressions/` are byte-identical to the
focused corpus frozen by `Icod.CommandFramework` A05. The adjacent historical
README is copied unchanged as provenance. The original namespace and using
directives are also unchanged; these are inherited behavior assertions, not
strict POSIX conformance claims.

E01 has copied the engine and the two external Text enums, and this project
is now in `Icod.Expressions.sln`. Its 193 runtime cases passed locally on
.NET 10 Staging without changing a test expectation. The test project also
targets .NET 8 and 9 for CI parity checks. The copied source
temporarily retains its legacy namespaces for exact API and behavioral
parity. See
`../../doc/EXTRACTION-READINESS.md` and `../../tools/verify-frozen-corpus.sh`.
