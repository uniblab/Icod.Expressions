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

1. Merge Expressions PR #1 after its five CI jobs pass at the final head.
   Wait for the five-job `main` push CI at the merge commit as well: the
   publication workflow enforces this exact-commit gate. Check the pack,
   package-only sample, A05/E02 comparison, and cross-platform 725-case
   scratch bridge gate. Keep CommandFramework PR #16 additive.
2. In the nuget.org account that will own `Icod.Expressions`, create a trusted
   publishing policy for owner `uniblab`, repository `Icod.Expressions`,
   workflow file `publish-nuget.yml`, and environment `nuget`. Scope the
   policy to creating `Icod.Expressions` and its versions. In the GitHub
   repository, set the `NUGET_USER` variable to that account's profile name.
   The account owner must perform this setup; the workflow has no stored API key.
   Follow the [NuGet trusted publishing setup](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing)
   for the account and workflow policy.
3. From `main`, manually dispatch `publish-nuget` with input
   `0.1.0-alpha.1`. The workflow verifies the exact reviewed version, builds
   and tests Release on .NET 8/9/10, verifies frozen source and parity data,
   packs, restores and runs the package-only sample, then requests a short-lived
   NuGet credential and publishes that exact `.nupkg`.
4. Confirm the published version is available from NuGet and install it in a
   clean package-only consumer. Record the package URL and immutable release
   commit for the migration PRs.
5. Migrate and validate all known consumers, including CoreUtils, Grep,
   ProcPs, DiffUtils, and LineEditor, before any CommandFramework production
   regex code is removed or replaced. Keep `tac` and direct `diff -I` behavior
   corrections in their separate issues. A distributable bridge and ownership
   switch need their own review and full consumer evidence.

NuGet versions are immutable. If the release package or its metadata needs
correction after publication, make a new prerelease version and rerun the
release gate; do not assume the old version can be overwritten.
