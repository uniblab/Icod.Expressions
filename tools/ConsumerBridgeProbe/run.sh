#!/bin/sh
set -eu

repository_root=$(CDPATH= cd -- "$(dirname -- "$0")/../.." && pwd -P)
if [ "$#" -ne 1 ]; then
  echo "Usage: sh tools/ConsumerBridgeProbe/run.sh /path/to/local-package-feed" >&2
  exit 2
fi
package_feed=$(CDPATH= cd -- "$1" && pwd -P)
test -f "$package_feed/Icod.Expressions.0.1.0-alpha.1.nupkg"
dotnet_command=${DOTNET_CMD:-dotnet}
scratch=$(mktemp -d)
trap 'rm -rf "$scratch"' EXIT HUP INT TERM

command_framework_sha=e1e118f18246eed290bee4758325a20e302bfb92
coreutils_sha=2c47cd67bae490dedf16234d8460c8b9da8d3404
procps_sha=e802ac03d941b1ba9fbe30e8f9c68fc7efcf3cff
diffutils_sha=8192ec43e1f6add42bc759b225c814861e18c843
grep_sha=2ef1c11f11b77eb876242eb0c92a88d20857db64
lineeditor_sha=1705e1dc5107eb2ede6b5ecec5dad7ea18c93061

fetch_source() {
  target=$1
  url=$2
  revision=$3
  mkdir -p "$target"
  git -C "$target" init -q
  git -C "$target" fetch -q --depth 1 "$url" "$revision"
  git -C "$target" checkout -q --detach FETCH_HEAD
}

command_framework_root="$scratch/Icod.CommandFramework"
coreutils_root="$scratch/Icod.CoreUtils"
procps_root="$scratch/Icod.ProcPs"
diffutils_root="$scratch/Icod.DiffUtils"
grep_root="$scratch/Icod.Grep"
lineeditor_root="$scratch/Icod.LineEditor"
fetch_source "$command_framework_root" https://github.com/uniblab/Icod.CommandFramework.git "$command_framework_sha"
fetch_source "$coreutils_root" https://github.com/uniblab/Icod.CoreUtils.git "$coreutils_sha"
fetch_source "$procps_root" https://github.com/uniblab/Icod.ProcPs.git "$procps_sha"
fetch_source "$diffutils_root" https://github.com/uniblab/Icod.DiffUtils.git "$diffutils_sha"
fetch_source "$grep_root" https://github.com/uniblab/Icod.Grep.git "$grep_sha"
fetch_source "$lineeditor_root" https://github.com/uniblab/Icod.LineEditor.git "$lineeditor_sha"
test "$(git -C "$command_framework_root" rev-parse HEAD)" = "$command_framework_sha"
test "$(git -C "$coreutils_root" rev-parse HEAD)" = "$coreutils_sha"
test "$(git -C "$procps_root" rev-parse HEAD)" = "$procps_sha"
test "$(git -C "$diffutils_root" rev-parse HEAD)" = "$diffutils_sha"
test "$(git -C "$grep_root" rev-parse HEAD)" = "$grep_sha"
test "$(git -C "$lineeditor_root" rev-parse HEAD)" = "$lineeditor_sha"

original_tests="$coreutils_root/tests/Expr.Tests/Icod.CoreUtils.Expr.Tests.csproj"
"$dotnet_command" restore "$original_tests" -m:1 --ignore-failed-sources
"$dotnet_command" test "$original_tests" -c Staging --no-restore -m:1 -v minimal
original_output="$coreutils_root/tests/Expr.Tests/bin/Staging/net10.0"

# These three command projects compile the same GNU Extended adapter against
# the published CommandFramework 2.1.0 package. Keep their original binaries
# for the later assembly substitution; do not edit their project references.
for command in Pgrep Pkill PidWait; do
  project="$procps_root/tests/ProcPs.$command.Tests/Icod.ProcPs.$command.Tests.csproj"
  "$dotnet_command" restore "$project" -m:1 --ignore-failed-sources
  "$dotnet_command" test "$project" -c Staging --no-restore -m:1 -v minimal
done

# Diff and SDiff compile GNU Basic call sites against CommandFramework 2.1.0.
for command in Diff SDiff; do
  project="$diffutils_root/tests/$command.Tests/Icod.DiffUtils.$command.Tests.csproj"
  "$dotnet_command" restore "$project" -m:1 --ignore-failed-sources
  "$dotnet_command" test "$project" -c Staging --no-restore -m:1 -v minimal
done

grep_tests="$grep_root/tests/Grep.Tests/Icod.Grep.Tests.csproj"
"$dotnet_command" restore "$grep_tests" -m:1 --ignore-failed-sources
# Hosted Linux CI runs all 113 tests. GREP_TEST_FILTER is only for local
# environments that cannot create the Unix socket required by one device test.
if [ -n "${GREP_TEST_FILTER:-}" ]; then
  "$dotnet_command" test "$grep_tests" -c Staging --no-restore -m:1 -v minimal \
    --filter "$GREP_TEST_FILTER"
else
  "$dotnet_command" test "$grep_tests" -c Staging --no-restore -m:1 -v minimal
fi

for command in Sed Ed.Shared Ed Red; do
  project="$lineeditor_root/tests/$command.Tests/Icod.LineEditor.$command.Tests.csproj"
  "$dotnet_command" restore "$project" -m:1 --ignore-failed-sources
  "$dotnet_command" test "$project" -c Staging --no-restore -m:1 -v minimal
done

# Use an isolated package cache so this run cannot reuse a previous alpha
# package with the same version from a developer's global NuGet cache.
NUGET_PACKAGES="$scratch/packages"
export NUGET_PACKAGES
bridge_project="$repository_root/tools/ConsumerBridgeProbe/Bridge/Bridge.csproj"
"$dotnet_command" restore "$bridge_project" -m:1 \
  -p:CommandFrameworkRoot="$command_framework_root" \
  --source "$package_feed" --source https://api.nuget.org/v3/index.json
"$dotnet_command" build "$bridge_project" -c Staging --no-restore -m:1 -v minimal \
  -p:CommandFrameworkRoot="$command_framework_root"

probe_project="$repository_root/tools/ConsumerBridgeProbe/Probe/Probe.csproj"
"$dotnet_command" restore "$probe_project" -m:1 \
  -p:OriginalConsumerOutput="$original_output" \
  --source "$package_feed" --source https://api.nuget.org/v3/index.json
"$dotnet_command" build "$probe_project" -c Staging --no-restore -m:1 -v minimal \
  -p:OriginalConsumerOutput="$original_output"
probe_output="$repository_root/tools/ConsumerBridgeProbe/Probe/bin/Staging/net10.0"
if "$dotnet_command" "$probe_output/Probe.dll" > "$scratch/unbridged-output.txt" 2>&1; then
  echo "Unbridged consumer unexpectedly resolved regex from Expressions." >&2
  exit 1
fi
case "$(cat "$scratch/unbridged-output.txt")" in
  "Unexpected assembly origins: regex="*"/Icod.CommandFramework.dll; framework="*) ;;
  *)
    cat "$scratch/unbridged-output.txt" >&2
    echo "Unbridged control failed for an unexpected reason." >&2
    exit 1
    ;;
esac
echo "Unbridged assembly identity rejected as expected."

bridge_output="$repository_root/tools/ConsumerBridgeProbe/Bridge/bin/Staging/net10.0"
cp "$bridge_output/Icod.CommandFramework.dll" "$probe_output/"
"$dotnet_command" "$probe_output/Probe.dll"

mkdir -p "$scratch/ExprTests"
cp -a "$original_output/." "$scratch/ExprTests/"
cp "$bridge_output/Icod.CommandFramework.dll" "$scratch/ExprTests/"
cp "$probe_output/Icod.Expressions.dll" "$scratch/ExprTests/"
"$dotnet_command" vstest "$scratch/ExprTests/Icod.CoreUtils.Expr.Tests.dll" \
  --Logger:'console;verbosity=minimal'

for command in Pgrep Pkill PidWait; do
  original_procps_output="$procps_root/tests/ProcPs.$command.Tests/bin/Staging/net10.0"
  bridged_procps_output="$scratch/ProcPs.$command.Tests"
  mkdir -p "$bridged_procps_output"
  cp -a "$original_procps_output/." "$bridged_procps_output/"
  cp "$bridge_output/Icod.CommandFramework.dll" "$bridged_procps_output/"
  cp "$probe_output/Icod.Expressions.dll" "$bridged_procps_output/"
  "$dotnet_command" vstest "$bridged_procps_output/Icod.ProcPs.$command.Tests.dll" \
    --Logger:'console;verbosity=minimal'
done

for command in Diff SDiff; do
  original_diffutils_output="$diffutils_root/tests/$command.Tests/bin/Staging/net10.0"
  bridged_diffutils_output="$scratch/DiffUtils.$command.Tests"
  mkdir -p "$bridged_diffutils_output"
  cp -a "$original_diffutils_output/." "$bridged_diffutils_output/"
  cp "$bridge_output/Icod.CommandFramework.dll" "$bridged_diffutils_output/"
  cp "$probe_output/Icod.Expressions.dll" "$bridged_diffutils_output/"
  "$dotnet_command" vstest "$bridged_diffutils_output/Icod.DiffUtils.$command.Tests.dll" \
    --Logger:'console;verbosity=minimal'
done

original_grep_output="$grep_root/tests/Grep.Tests/bin/Staging/net10.0"
bridged_grep_output="$scratch/Grep.Tests"
mkdir -p "$bridged_grep_output"
cp -a "$original_grep_output/." "$bridged_grep_output/"
cp "$bridge_output/Icod.CommandFramework.dll" "$bridged_grep_output/"
cp "$probe_output/Icod.Expressions.dll" "$bridged_grep_output/"
if [ -n "${GREP_TEST_FILTER:-}" ]; then
  "$dotnet_command" vstest "$bridged_grep_output/Icod.Grep.Tests.dll" \
    "--TestCaseFilter:$GREP_TEST_FILTER" --Logger:'console;verbosity=minimal'
else
  "$dotnet_command" vstest "$bridged_grep_output/Icod.Grep.Tests.dll" \
    --Logger:'console;verbosity=minimal'
fi

for command in Sed Ed.Shared Ed Red; do
  original_lineeditor_output="$lineeditor_root/tests/$command.Tests/bin/Staging/net10.0"
  bridged_lineeditor_output="$scratch/LineEditor.$command.Tests"
  mkdir -p "$bridged_lineeditor_output"
  cp -a "$original_lineeditor_output/." "$bridged_lineeditor_output/"
  cp "$bridge_output/Icod.CommandFramework.dll" "$bridged_lineeditor_output/"
  cp "$probe_output/Icod.Expressions.dll" "$bridged_lineeditor_output/"
  "$dotnet_command" vstest "$bridged_lineeditor_output/Icod.LineEditor.$command.Tests.dll" \
    --Logger:'console;verbosity=minimal'
done
echo "Source-pinned CoreUtils, ProcPs, DiffUtils, Grep, and LineEditor consumers passed with the package-backed scratch bridge."
