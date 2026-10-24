#!/bin/sh
set -eu

cd "$(dirname "$0")/.."

sh tools/verify-frozen-corpus.sh
sh tools/verify-frozen-source.sh

set -- tests/RegularExpressions/*.cs
if [ "$#" -ne 13 ]; then
    printf 'Expected 13 focused C# test files; got %s\n' "$#" >&2
    exit 1
fi

while read -r expected path; do
    actual=$(git hash-object "$path")
    if [ "$actual" != "$expected" ]; then
        printf 'A05 candidate drift: %s (expected %s, got %s)\n' "$path" "$expected" "$actual" >&2
        exit 1
    fi
done <<'BLOBS'
ef9cd9e358460f412d343c91db228c6179c964ff tests/RegularExpressions/IcodExpressionsA02SyntaxOptionCharacterizationTests.cs
589a52b42d5e3010e9e678f771cc85b156236e0b tests/RegularExpressions/IcodExpressionsA04ConsumerShapeTests.cs
59a10c631112307159a0682a493062de4cd82c80 tools/PosixRegexDifferential/Program.cs
7511107d8c12d1e98b53c88311c2d44ae0c0c387 tools/GnulibRegexOracle/run.sh
BLOBS

printf 'A05 candidate identities match: 13 focused files and two oracle tools.\n'
