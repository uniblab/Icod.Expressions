# CI Configuration and Architecture Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build Debug locally, Staging on PRs, and Release on main and tags across six native OS/architecture lanes.

**Architecture:** Separate PR and main workflows match the repository template. The tag workflow validates all six Release lanes before its single-project package and publish jobs.

**Tech Stack:** GitHub Actions, .NET 8/9/10, PowerShell, YAML.

**Spec:** `docs/superpowers/specs/2026-10-24-ci-configuration-design.md`

## Global Constraints

- Keep one root `Icod.Expressions.csproj` package and source under `/src`, tests under `/tests`, samples under `/samples`.
- Local scripts default Debug; CI PR Staging; main and tags Release.
- No changes to CommandFramework or consumer production paths.

## Review Focus

- A tag at an exact main commit with fewer than six successful Release CI jobs must not publish.
- Each of Linux, Windows, and macOS must use real x64 and ARM64 runners.
- ARM64 tests must use the same three target frameworks as x64.
- PR source-pinned parity scripts that hard-code Staging must not run on main or tag.
- The NuGet package must be built from Release on a tag, after validation.

---

### Task 1: PR and main workflows

**Files:** Modify `.github/workflows/pull-request.yml`; create `.github/workflows/main.yaml`.

- [ ] Restrict PR workflow to pull requests, set Staging, and build/test on six named native runner lanes; keep source-pinned checks on supported hosts.
- [ ] Add main push workflow in Release, with the same six native runner lanes, .NET 8/9/10 build and tests, and single package verification on Linux x64.
- [ ] Check YAML syntax and assert trigger/configuration/matrix and command invariants with a static script.
- [ ] Commit.

### Task 2: Tag release workflow

**Files:** Modify `.github/workflows/release.yaml`.

- [ ] Require six successful `main.yaml` jobs at the exact tagged main commit.
- [ ] Add six Release build/test validation jobs and require them before the existing Release package/publish path.
- [ ] Check YAML syntax, six matrix pairs, dependency graph and Release commands; commit.

### Task 3: End-to-end PR verification

**Files:** None.

- [ ] Open PR targeting main and inspect all six CI job conclusions and their build configurations.
- [ ] Review the diff and summarize any runner-specific limits; leave PR unmerged.
