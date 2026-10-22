#!/bin/sh
set -eu

cd "$(dirname "$0")/.."

count=0
while read -r expected path; do
    actual=$(git hash-object "$path")
    if [ "$actual" != "$expected" ]; then
        printf 'Source drift: %s (expected %s, got %s)\n' "$path" "$expected" "$actual" >&2
        exit 1
    fi
    count=$((count + 1))
done < doc/Icod.Expressions-A05-Source-Blobs.txt

if [ "$count" -ne 35 ]; then
    printf 'Expected 35 source/enum/README blobs; got %s\n' "$count" >&2
    exit 1
fi
printf 'All 35 A05 production source blobs match.\n'
