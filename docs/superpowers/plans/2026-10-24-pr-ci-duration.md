# PR CI Duration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Parallelize source-pinned CI work and skip it for internal PCRE-only PRs while preserving broad validation for changes that may affect consumers.

**Architecture:** A conservative changed-path classifier gates three independent x64 compatibility jobs; the six native core jobs remain unconditional. The broader PowerShell consumer probe replaces the duplicate Linux shell invocation.

**Tech Stack:** GitHub Actions, POSIX shell, .NET 8/9/10.

**Spec:** `docs/superpowers/specs/2026-10-24-pr-ci-duration-design.md`

## Global Constraints

- Local Debug, PR Staging, main/tag Release.
- Six native OS/architecture build and test lanes; frozen GNU corpus and package-only smoke remain.
- Exact six-job main Release guard for tag publishing.
- No changes to consumer or CommandFramework production paths.

## Review Focus

- A renamed/deleted GNU file must trigger full compatibility checks.
- Empty or unexpected paths must trigger full checks.
- A mixed PCRE and public project change must trigger full checks.
- Internal-only PRs must still build and run full/frozen tests across six lanes.
- The optimization PR itself must run the full compatibility checks before review.

---

### Task 1: Conservative classifier

**Files:** Create `.github/scripts/classify-pr-changes.sh`, `tools/verify-ci-classifier.sh`.

- [ ] Write cases for PCRE-only, mixed, GNU, workflow, and empty input and watch the test fail.
- [ ] Implement the classifier, run its tests and commit.

### Task 2: Workflow gating

**Files:** Modify `.github/workflows/pull-request.yml`.

- [ ] Add a classification job using full git history, the event base SHA and `--no-renames`; gate three independent x64 compatibility jobs that build their own candidate package.
- [ ] Remove the duplicate Linux shell bridge invocation; keep the six core jobs and package smoke.
- [ ] Check YAML/job dependencies and run the six core and three compatibility jobs on the optimization PR; compare measured timings and commit.
