#!/bin/sh
set -eu

# Run the upstream regex test against Gnulib's included implementation.
# Keep Gnulib's GPL test source in an ephemeral checkout, not this repository.
revision=fb7312fa8d3df29f0ca0678f669b9a5b88a078ec
test_blob=0ab97819d2cdf3b29f5f4b989d1c33e0bd157f36
work=$(mktemp -d)
trap 'result=$?; if test "$result" -ne 0; then for log in bootstrap configure gllib test-build strict-build; do if test -f "$work/$log.log"; then printf "\n%s log tail:\n" "$log"; tail -n 50 "$work/$log.log"; fi; done; fi' EXIT

git clone --quiet --filter=blob:none https://github.com/coreutils/gnulib.git "$work/gnulib"
git -C "$work/gnulib" checkout --quiet --detach "$revision"
test "$(git -C "$work/gnulib" rev-parse HEAD)" = "$revision"
test "$(git -C "$work/gnulib" hash-object tests/test-regex.c)" = "$test_blob"

"$work/gnulib/gnulib-tool" --create-testdir --dir="$work/oracle" regex-tests > "$work/bootstrap.log"
(
  cd "$work/oracle"
  ./configure --with-included-regex > "$work/configure.log"
  make -j2 -C gllib > "$work/gllib.log"
  make -j2 -C gltests test-regex > "$work/test-build.log"
  test -s gllib/regex.o
  regex_symbol=$(nm gllib/regex.o | grep -E ' [TW] [^ ]*regcomp$' | head -n 1)
  test -n "$regex_symbol"
  symbol_name=${regex_symbol##* }
  nm gltests/test-regex | grep -F " $symbol_name" | grep -Eq ' [TW] '
  LC_ALL=C ./gltests/test-regex

  # The upstream table tolerates actual participating captures when its
  # expected register is (-1,-1). Make a temporary exact-comparison copy.
  test "$(grep -Fc 'if (tests[i].rm[n].rm_so == -1 && tests[i].rm[n].rm_eo == -1)' gltests/test-regex.c)" -eq 1
  sed '/if (tests\[i\]\.rm\[n\]\.rm_so == -1 && tests\[i\]\.rm\[n\]\.rm_eo == -1)/,+1d' \
    gltests/test-regex.c > "$work/strict-test-regex.c"
  test "$(grep -Fc 'if (tests[i].rm[n].rm_so == -1 && tests[i].rm[n].rm_eo == -1)' "$work/strict-test-regex.c" || :)" -eq 0
  cp "$work/strict-test-regex.c" gltests/test-regex.c
  make -j2 -C gltests test-regex > "$work/strict-build.log"
  nm gltests/test-regex | grep -F " $symbol_name" | grep -Eq ' [TW] '
  if strict_output=$(LC_ALL=C ./gltests/test-regex 2>&1); then
    printf 'The strict comparison unexpectedly passed.\n' >&2
    exit 1
  fi
  printf '%s\n' "$strict_output"
  test "$(printf '%s\n' "$strict_output" | grep -c '^test-regex: ')" -eq 4
  for row in 5 6 7 8; do
    printf '%s\n' "$strict_output" | grep -Fq "regexec $row match failure rm[1] 0..0"
  done
  printf 'Pinned Gnulib %s; included %s; four anchor captures participate at (0,0) in C locale.\n' "$revision" "$symbol_name"
)
