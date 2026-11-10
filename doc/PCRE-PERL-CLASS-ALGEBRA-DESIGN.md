# Perl extended character-class algebra

Implement PCRE2 10.48 Perl `(?[...])` over authoritative byte and UTF-8 subjects.
This is an internal single-scalar atom. UTS#18's separately activated grammar
follows in a subsequent increment on the same representation. Ordinary bracket
syntax and GNU behavior retain their current policies.

Operands are escaped scalars, shorthand/property classes, direct POSIX items,
and ordinary bracket leaves. Unary `!` binds first, then `&`, then the equally
binding left-associative `|`, `+`, `-`, `^`. Parentheses group expressions without
captures. Spaces/tabs are ignored locally, including bracket leaves; newlines
and `#` outside leaves are invalid. Closing `])` is adjacent. Only empty quote
sections and bare `\E` are ignored outside leaves. Native nesting limit15 counts
the outer class, grouping parentheses and ordinary brackets.

Use immutable postfix terms and bounded heap parser stacks. Apply case closure
to each operand before algebra. In UTF mode reuse pinned Unicode predicates and
folded ranges, emit one expression instruction, and use one lazily allocated
scratch stack per Match call. Charge every term and interval/predicate probe to
shared work/cancellation. No recursion, per-atom allocation, host properties or
native production fallback. Charge terms to the shared4096 compilation budget.

Byte mode projects to a bitmap at compile time. Native byte classes containing
Any and Unicode properties have observable bitmap hoisting behavior. Track two
sets per subtree (legacy property-only and logical) plus bitmap-needed flags
for positive and negative context. Complement swaps context flags; subtraction
complements its right context; other binary operators preserve both contexts.
Use logical membership when the root requires a bitmap, legacy membership
otherwise. Native standalone probes qualify1960 combinations independently.
Retain explicit-newline provenance; audit lookbehind width and conservative
start proofs. Runtime branch/capture/repeat state is unchanged.

Reference fixtures contain308 expressions ×8 byte/UTF/UCP/caseless profiles ×256
values, plus12 expressions ×4 profiles ×7592 Unicode category boundary points.
Each expected mask is exported by native PCRE2 10.48 JIT and checked against live
PCRE.NET as well as managed results. Focused tests cover grammar, captures,
repeats, behind, malformed UTF, nesting, shared compile and exact work limits.
