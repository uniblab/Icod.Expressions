# Internal PCRE UTF-8 letter property

## Intent

Support Grep's existing `^\p{L}+$` fixture within the internal strict UTF-8 matcher. This is one property slice, not a claim of general Unicode or UCP support.

## Approaches

| Approach | Tradeoff | Decision |
|---|---|---|
| Compile a static Unicode table into the package | Can pin a PCRE2-compatible Unicode version but requires source data, generation and cross-version maintenance before the internal fixture is useful. | Defer to conformance release review. |
| Use `Rune.GetUnicodeCategory` at a scalar instruction | Small, managed, allocation-free per match; Unicode database version follows the target runtime and must be compared with PCRE2 before Grep migration. | Choose for the internal foundation. |
| Use `.NET Regex` to evaluate properties | Different regex engine and a runtime fallback forbidden by the main design. | Reject. |

## Scope

- In `Utf8` mode, `\p{L}` matches Unicode categories Lu, Ll, Lt, Lm and Lo. `\P{L}` and `\p{^L}` invert that result; `\P{^L}` matches letters. The unbraced `\pL` and `\PL` spellings have the same behavior. These consume one complete scalar while match and capture spans remain source-byte offsets.
- Property atoms compose with ordered alternatives, capture/backreference handling and existing greedy/lazy quantifiers. Each attempted property evaluation is counted in the per-match work budget and honors cancellation.
- Malformed property escapes are syntax errors at the backslash byte offset. Other Unicode properties, property classes inside brackets and property matching in byte mode remain explicit `UnsupportedSyntax` diagnostics. Existing byte-mode and UTF-8 validation semantics remain intact.
- This internal step uses the .NET runtime's Unicode category data. Newer assigned characters may differ from PCRE2 10.48 or among net8/net9/net10. Freeze a Unicode data version or reconcile differences during the wider PCRE conformance gate before switching Grep.

## Next steps

Add Unicode decimal digits and UCP POSIX behavior, including Grep's ASCII `\d` override, then broader property syntax and versioned conformance fixtures. Do not change Grep or CommandFramework in this PR.
