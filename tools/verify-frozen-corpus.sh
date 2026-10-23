#!/bin/sh
set -eu

cd "$(dirname "$0")/.."

while read -r expected path; do
    actual=$(git hash-object "$path")
    if [ "$actual" != "$expected" ]; then
        printf 'Frozen corpus drift: %s (expected %s, got %s)\n' "$path" "$expected" "$actual" >&2
        exit 1
    fi
done <<'BLOBS'
8d32f8728b8eb746c7e01e7c0e143c836481c629 tests/Icod.Expressions.Tests/RegularExpressions/GnuBasicRegularExpressionTests.cs
f5a5182f86bfa7ef746419baf06d5e848c7d65a6 tests/Icod.Expressions.Tests/RegularExpressions/GnuEmacsRegularExpressionTests.cs
98be9ebda6dc3a251a2d7ba3fa9f26692cd085da tests/Icod.Expressions.Tests/RegularExpressions/GnuExtendedRegularExpressionTests.cs
7c6b52ec36571189cd6255467117823cc615d12e tests/Icod.Expressions.Tests/RegularExpressions/IcodExpressionsA04CharacterizationTests.cs
429ab21e8ca849c09524695031841394dc6bb14d tests/Icod.Expressions.Tests/RegularExpressions/LineEditorRegularExpressionContractTests.cs
0219c1b1f8d09aad126a4097147bd3a93157b607 tests/Icod.Expressions.Tests/RegularExpressions/LiteralPrefixRegularExpressionTests.cs
f4bf0de57e6dfdc896dc13023b8e66b1dc7f2ae9 tests/Icod.Expressions.Tests/RegularExpressions/PreparedRegularExpressionProfileTests.cs
a97f137c3048c319234250c40639df7086ac6b15 tests/Icod.Expressions.Tests/RegularExpressions/PreparedRegularExpressionTests.cs
44d9f850c233d7d53253de702e8b8be92c8b3eb1 tests/Icod.Expressions.Tests/RegularExpressions/PublicPreparedByteInputIsolationTests.cs
da24093369581bfbe40b4f655537b48ccbbb8159 tests/Icod.Expressions.Tests/RegularExpressions/PublicPreparedByteInputTests.cs
a35ab805ca570cedae62149941e6f1b3650d8e81 tests/Icod.Expressions.Tests/RegularExpressions/R2_4RegularExpressionClosureTests.cs
54cc715b8da972321490916e0cf026d2f9d8403a tests/Icod.Expressions.Tests/RegularExpressions/README.md
0c1cd09d4510815256ab80bd9c3fed66f88c2297 doc/Icod.Expressions-A04-Parity-Baseline.jsonl
c1f138d20daf3c3904c899349891f2a9af59300b doc/Icod.Expressions-A00-Compiled-API.txt
BLOBS

printf 'Frozen corpus Git blobs match A05.\n'
