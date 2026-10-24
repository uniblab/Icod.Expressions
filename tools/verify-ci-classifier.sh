#!/bin/sh
set -eu

root=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd -P)
classifier="$root/.github/scripts/classify-pr-changes.sh"

check() {
  expected=$1
  paths=$2
  actual=$(printf '%s' "$paths" | sh "$classifier")
  if [ "$actual" != "$expected" ]; then
    echo "Expected $expected for paths [$paths], got $actual" >&2
    exit 1
  fi
}

check false 'src/Pcre/PcreByteParser.cs
tests/Pcre/PcreByteAtomTests.cs
doc/PCRE-DESIGN.md
'
check true 'src/RegularExpressions/Regex.cs
'
check true 'src/Pcre/PcreByteParser.cs
Icod.Expressions.csproj
'
check true '.github/workflows/pull-request.yml
'
check true 'src/Pcre/PcreByteParser.cs
src/RegularExpressions/Removed.cs
'
check true ''

echo 'PR compatibility classifier passed.'
