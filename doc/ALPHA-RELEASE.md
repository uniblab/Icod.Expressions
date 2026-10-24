# 0.1.0-alpha.1 release gate

This alpha preserves the inherited `Icod.CommandFramework.RegularExpressions`
and `.Text` public type names in the independent `Icod.Expressions` assembly.
The [accepted A05 extraction baseline](Icod.Expressions-A05-Final-Candidate.md)
is bounded to its frozen corpus. The known strict POSIX/GNU differences and
uncovered cases are recorded there. This is not a stable standards release.

The package has no CommandFramework dependency. Adding both assemblies to a
consumer while they still define the same full names needs an explicit type
identity strategy. No production CommandFramework regex source is removed by
publishing this package.

## Publish sequence

1. Confirm the five-job `main` push CI passes at the release commit. The
   publication workflow enforces this exact-commit gate. Check the pack,
   package-only sample, A05/E02 comparison, and cross-platform 725-case
   scratch bridge gate. CommandFramework's merged audit PR #16 changed no
   production regex source.
2. Use the active NuGet trusted publishing policy for package owner
   `uniblab`, package `Icod.Expressions`, repository owner `uniblab`,
   repository `Icod.Expressions`, workflow file `release.yaml`, and
   environment `Release`. The policy allows new packages and versions.
   The `NUGET_USER` Actions secret holds the publishing profile name. The
   workflow exchanges its GitHub OIDC token for a short-lived credential;
   it has no stored NuGet API key. See the
   [NuGet trusted publishing setup](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing)
   for how the short-lived credential is issued.
3. Tag the validated `main` commit `v0.1.0-alpha.1` and push the tag.
   The tag starts `release.yaml`. It verifies that the tag matches the
   project version, belongs to `main`, and has the five passing CI jobs
   for that exact commit. It then builds and tests Release on .NET 8/9/10,
   verifies frozen source and parity data, packs exactly one
   `Icod.Expressions.0.1.0-alpha.1.nupkg`, checks its nuspec, and runs
   the package-only sample. The same package artifact publishes to
   NuGet.org and GitHub Packages; when both succeed the workflow creates
   a GitHub prerelease with a SHA256 checksum. This library has no
   executable archive path and no multi-package selection.
4. Confirm the published version is available from NuGet and install it
   in a clean package-only consumer. Record the package URL and immutable
   release commit for the migration PRs.
5. Migrate and validate all known consumers, starting with ProcPs and
   DiffUtils, then CoreUtils and LineEditor, and leaving Grep last for its
   planned PCRE implementation. Do this before any CommandFramework production
   regex code is removed or replaced. Keep `tac` and direct `diff -I` behavior
   corrections in their separate issues. A distributable bridge and ownership
   switch need their own review and full consumer evidence.

NuGet versions are immutable. If the release package or its metadata needs
correction after publication, make a new prerelease version and rerun the
release gate; do not assume the old version can be overwritten.

Publish `.snupkg` symbol packages in a later release under
[issue #5](https://github.com/uniblab/Icod.Expressions/issues/5). The initial
alpha package remains unchanged.
