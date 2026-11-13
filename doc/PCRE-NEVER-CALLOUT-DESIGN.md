# Caller prohibition of manual callouts

Add default-false internal NeverCallout for PCRE2 10.48 extra-option0x8000.
A real manual `(?C` token raises InvalidSyntax at its original opening byte,
including incomplete, numeric/string, unreachable, repeated and nested forms.
Native203 reports the byte immediately after C; internal diagnostics retain the
existing opening-token convention. Reject before validating callout arguments.
Callout execution remains explicitly unsupported when the option is false.

## Approach and constraints

Use the existing atom dispatch in ParseAlternatives, after quote/trivia handling
and before ParseAtom. Snapshot the caller option once. The dispatch has the
current conditional frame, so it can retain native conditional-header priority:
a condition's leading callout needs at least one source byte after C before it
is dispatched. A truncated `(?(?C` remains the existing unsupported condition;
its native128 assertion error precedes the disabled-callout stage. Other real
callout tokens reject immediately. Source UTF/caller-mode validation and earlier
parser errors keep their priority; later unresolved reference checks do not
preempt the guard. Existing library capture/depth/expansion ceilings remain.

A global source scan would incorrectly reject inert classes, quoted source,
comments and labels. Parsing/executing callouts is a different increment and
would change the current unsupported boundary. The selected dispatch guard
reuses existing InvalidSyntax, current frame state and canonical matcher paths;
no new AST/IR/opcodes, matcher/journal/limit changes, source rewriting or APIs.
No dependencies/workflows are changed. BadEscapeIsLiteral cannot suppress this
group-syntax failure; valid reset/unset scopes cannot turn off a caller guard.

## Qualification

Pin native-first guard errors, header octets and malformed UTF/source precedence,
valid/default callout controls, scoped/conditional/unreachable contexts, prior
syntax/reference ordering, inert quotes/comments/classes/raw/decoded labels,
newlines/offsets, options/source ownership, work/cancellation and quotas.
Observe clean net8/9/10 tests-only RED and freeze the new fixture before source
changes. Qualify all18 native results and every configured supporting gate.
All repository docs precede the final qualified source head; final GREEN,
author review and base/head/tree/merge identity live on PR241 and only issue7's
roadmap footer. Reuse the linked worktree, execute inline without questions or
subagents, and do not install an SDK. User owns merge/tag/publication; Grep last.

Baseline merged240 99a7a99baf52fdcb41db73e0dcfa21dd89c68c0f,
tree0f7215505e3a92510a15a5511d74baa0e881cc1e;71,410 tests/framework,256 earlier
test files. Preserve their hashes, GNU/BRE/ERE, frozen35/214 outcomes/120methods,
C#13/net8/9/10 and UTF8+LF. Existing unsupported UTF/code-unit/surrogate-context
boundaries, exported API and wider optimizer/consumer/release gaps remain open.
