#!/bin/sh
set -eu

# Run the upstream regex test against Gnulib's included implementation.
# Keep Gnulib's GPL test source in an ephemeral checkout, not this repository.
revision=fb7312fa8d3df29f0ca0678f669b9a5b88a078ec
test_blob=0ab97819d2cdf3b29f5f4b989d1c33e0bd157f36
work=$(mktemp -d)
trap 'result=$?; if test "$result" -ne 0; then for log in bootstrap configure gllib test-build; do if test -f "$work/$log.log"; then printf "\n%s log tail:\n" "$log"; tail -n 50 "$work/$log.log"; fi; done; fi' EXIT

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
  nm gltests/test-regex | grep -Eq ' [TW] regcomp$'
  printf 'Pinned Gnulib %s; included regex.o linked into test-regex; LC_ALL=C\n' "$revision"
  LC_ALL=C ./gltests/test-regex
)
