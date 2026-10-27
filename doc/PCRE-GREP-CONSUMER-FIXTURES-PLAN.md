# Grep consumer fixture inventory plan

**Goal:** make Grep's current PCRE behavior traceable to authoritative byte fixtures, while naming consumer-level and performance gaps.

**Architecture:** use one pinned PCRE.NET 1.6.0 reference table in the Expressions test project with Grep's exact byte/UTF options. Independently expected byte spans must agree with both the reference and the internal matcher; separate tests lock unsupported syntax to compile diagnostics. The inventory connects each Grep test to a fixture or a remaining consumer concern.

**Tech Stack:** C# 13, xUnit, test-only PCRE.NET 1.6.0, net8/net9/net10.

**Spec:** `doc/PCRE-DESIGN.md`, `Icod.Grep/tests/Grep.Tests/src/PcreTests.cs` at the current Grep baseline.

## Global constraints

- Do not change any consumer, public API, package version, or `Icod.CommandFramework` implementation.
- Preserve original source-byte offsets and Latin1/UTF-8 pattern encoding; no runtime reference-engine fallback.
- Run the entire Staging CI matrix on six OS/architecture targets; no local .NET SDK in this workspace.
- No subagents.

## Task 1: freeze consumer-facing engine fixtures

**Files:** create `tests/Pcre/PcreGrepConsumerFixtureTests.cs`.

- [ ] Add exact pattern/subject/options/start/byte-span expectations for Grep's supported `-P` patterns, including a negative case, NUL/high bytes, malformed UTF-8, and nonzero offsets.
- [ ] Compare each independently expected result to the pinned PCRE2 wrapper and to the internal matcher.
- [ ] Assert the current compiler explicitly rejects selected pending syntax and malformed groups.

## Task 2: inventory handoff and verification

**Files:** create `doc/PCRE-GREP-CONSUMER-FIXTURES.md`; update `doc/PCRE-DESIGN.md`.

- [ ] Map every current `PcreTests.cs` case to engine fixtures or consumer-only behavior.
- [ ] Identify supported, rejected and pending syntax with concrete examples; link the Grep performance workload sources.
- [ ] Run six-target Staging CI including full suite, frozen 214-case corpus and package-only consumer; self-review diff and exact head.
