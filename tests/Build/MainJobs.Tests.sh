#!/bin/sh
set -eu
root=$(CDPATH= cd -- "$(dirname -- "$0")/../.." && pwd)
temporary=$(mktemp -d)
trap 'rm -rf "$temporary"' EXIT HUP INT TERM
jq -n '{total_count:7,jobs:(["Linux x64","Linux ARM64","Windows x64","Windows ARM64","macOS ARM64","macOS x64 net10.0","macOS x64"]|map({name:.,status:"completed",conclusion:"success"}))}' > "$temporary/valid.json"
jq -e -f "$root/.github/scripts/verify-main-jobs.jq" "$temporary/valid.json" > /dev/null
for mutation in \
  '.total_count=6' \
  'del(.jobs[5])|.total_count=6' \
  '.jobs[5].name="macOS x64 net9.0"' \
  '.jobs[5].name="unexpected"' \
  '.jobs[5].conclusion="failure"' \
  '.jobs[5].conclusion="skipped"' \
  '.jobs[6].status="in_progress"' \
  '.jobs[6].conclusion="cancelled"'
do
  jq "$mutation" "$temporary/valid.json" > "$temporary/invalid.json"
  if jq -e -f "$root/.github/scripts/verify-main-jobs.jq" "$temporary/invalid.json" > /dev/null; then
    echo "Accepted invalid main jobs: $mutation" >&2
    exit 1
  fi
done
echo 'PASS: complete main jobs and eight rejection cases'
