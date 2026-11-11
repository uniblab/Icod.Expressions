# UTS#18 extended character classes

Implement the PCRE2 10.48 ALT_EXTENDED_CLASS compile option internally as
PcreByteOptions.AltExtendedClass, defaultfalse. All brackets outside Perl
(?[...]) gain nested brackets, ||/&&/--/~~ union/intersection/subtraction/XOR,
and implicit union binding tighter than explicit operations. Explicit
operators at one level must all be the same; evaluate left-associatively.
Leading ^ complements the whole bracket. Whitespace follows the surrounding
x/xx policy; quotes preserve literals. Perl leaves remain ordinary brackets.

Use the existing immutable postfix terms, byte projection and UTF evaluator.
A separate bounded heap-frame frontend retains each contiguous raw item run
as one leaf (essential to native Any/property bitmap behavior), combines runs
and nested classes by implicit union, then lowers explicit operations and
class complement. Nesting includes the outer class and is bounded at15;
expression terms share the4096 compilation budget and existing charged match
work/cancellation. Plain brackets without nested classes or doubled operators
return through the existing reader to preserve source/optimizer metadata.

This is the selected implementation approach under standing continuation
authorization. Textual rewriting would risk source offsets and grammar scope;
a separate matcher would duplicate validated algebra and resource handling.
No public API, new package, native/host fallback or GNU behavior change.
Preserve frozen35 production blobs, frozen214/120 and fixed PR212 test data.
UTS syntax alone is this increment; script runs and other roadmap gates follow.

Native masks cover235 expressions across8 byte/UTF/UCP/caseless profiles and
all256 values, plus12 expressions across4 profiles and7592 shared Unicode
boundary scalars. Live PCRE.NET additionally checks every mask. Focused tests
qualify activation, Perl leaf isolation, quotes/ranges/POSIX, captures/repeats/
behind, malformed UTF, original byte offsets and depth/shared compile/work
limits. Expected RED is the missing opt-in option, with baseline46069 intact.
