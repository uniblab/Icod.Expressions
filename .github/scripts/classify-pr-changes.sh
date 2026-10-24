#!/bin/sh
set -eu

full_compatibility=false
seen=false
while IFS= read -r path || [ -n "$path" ]; do
  seen=true
  case "$path" in
    src/Pcre/*|tests/Pcre/*|doc/PCRE-*.md) ;;
    *) full_compatibility=true ;;
  esac
done

if [ "$seen" = false ]; then
  full_compatibility=true
fi
printf '%s\n' "$full_compatibility"
