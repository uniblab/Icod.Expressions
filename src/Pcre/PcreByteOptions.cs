namespace Icod.Expressions.Pcre;

/// <summary>Provisional internal limits for one compiled byte pattern.</summary>
internal sealed record PcreByteOptions {
	internal const int DefaultMatchWorkLimit = 1_000_000;

	public int MatchWorkLimit { get; init; } = DefaultMatchWorkLimit;
	public bool Caseless { get; init; }
	/// <summary>Initialize restricted comparisons without enabling caseless matching.</summary>
	public bool CaselessRestricted { get; init; }
	/// <summary>Select global Turkish casing in effective UTF mode without enabling caseless matching.</summary>
	public bool TurkishCasing { get; init; }
	/// <summary>Let dot match newline characters.</summary>
	public bool DotAll { get; init; }
	/// <summary>Let line anchors recognize internal newlines.</summary>
	public bool Multiline { get; init; }
	/// <summary>Require non-multiline dollar anchors to match the subject end.</summary>
	public bool DollarEndOnly { get; init; }
	/// <summary>Permit multiline caret anchors after a final newline.</summary>
	public bool AltCircumflex { get; init; }
	/// <summary>Capture only named groups unless a local option restores unnamed captures.</summary>
	public bool NoAutoCapture { get; init; }
	/// <summary>Invert the default greedy or lazy preference of repeat quantifiers.</summary>
	public bool Ungreedy { get; init; }
	/// <summary>Permit multiple capture groups to share a name.</summary>
	public bool DuplicateNames { get; init; }
	/// <summary>Let references to existing unset captures match empty without changing participation.</summary>
	public bool MatchUnsetBackreference { get; init; }
	/// <summary>Ignore pattern whitespace and comments outside character classes.</summary>
	public bool Extended { get; init; }
	/// <summary>Also ignore unquoted spaces and tabs in character classes; implies extended parsing.</summary>
	public bool ExtendedMore { get; init; }
	/// <summary>Require word boundaries around an implicit noncapturing pattern group.</summary>
	public bool MatchWord { get; init; }
	/// <summary>Require a complete line; takes precedence over word matching.</summary>
	public bool MatchLine { get; init; }
	public bool Utf8 { get; init; }
	public bool MatchInvalidUtf { get; init; }
	/// <summary>Prohibit UTF mode, including pattern directives and implicit tolerant UTF.</summary>
	public bool NeverUtf { get; init; }
	public bool Ucp { get; init; }
	/// <summary>Prohibit UCP mode selected by the caller or pattern prefix.</summary>
	public bool NeverUcp { get; init; }
	/// <summary>Prohibit real code-unit escape tokens without restricting inert source text.</summary>
	public bool NeverBackslashC { get; init; }
	/// <summary>Reject manual callout tokens before validating their arguments.</summary>
	public bool NeverCallout { get; init; }
	/// <summary>Permit match-start resets inside assertions, including reversed reported endpoints.</summary>
	public bool AllowLookaroundBackslashK { get; init; }
	/// <summary>Enable PCRE2 UTS#18 nested brackets and doubled set operators.</summary>
	public bool AltExtendedClass { get; init; }
	/// <summary>Use alternate backslash-u, U and x grammar with complete adjacent hex digits.</summary>
	public bool AltBsux { get; init; }
	/// <summary>Recognize braced-u hex scalars and imply alternate u, U and x grammar.</summary>
	public bool ExtraAltBsux { get; init; }
	/// <summary>Interpret literal escapes, quotes and extended trivia in control-verb names.</summary>
	public bool AltVerbNames { get; init; }
	/// <summary>Allow escaped surrogate code points in UTF patterns without relaxing strict input validation.</summary>
	public bool AllowSurrogateEscapes { get; init; }
	/// <summary>Recover eligible bad escapes as their escaped character, retaining following source grammar.</summary>
	public bool BadEscapeIsLiteral { get; init; }
	public bool AsciiBsD { get; init; }
	/// <summary>Restrict backslash-s whitespace predicates to ASCII.</summary>
	public bool AsciiBsS { get; init; }
	/// <summary>Restrict backslash-w and word-boundary predicates to ASCII.</summary>
	public bool AsciiBsW { get; init; }
	/// <summary>Restrict ordinary POSIX character classes to ASCII.</summary>
	public bool AsciiPosix { get; init; }
	/// <summary>Restrict POSIX digit and xdigit classes to ASCII.</summary>
	public bool AsciiDigit { get; init; }
	/// <summary>Use Python's three-digit octal rule instead of preceding-capture disambiguation.</summary>
	public bool PythonOctal { get; init; }
	/// <summary>Reject a bare backslash-zero escape while retaining multi-digit and braced octals.</summary>
	public bool NoBackslashZero { get; init; }
	/// <summary>Interpret the backslash-r escape as LF while retaining other CR spellings.</summary>
	public bool EscapedCrIsLf { get; init; }
	/// <summary>Allow an initial closing bracket to terminate an empty character class.</summary>
	public bool AllowEmptyClass { get; init; }
	/// <summary>Recognize CR, LF, and CRLF as newlines, as in Grep's pinned PCRE2 build.</summary>
	public bool AnyCrLfNewline { get; init; }
	/// <summary>Restrict backslash-R sequences to CR, LF, and CRLF; independent of the newline convention.</summary>
	public bool BsrAnyCrLf { get; init; }

	/// <summary>Test-only progress hook used to cancel a running match deterministically.</summary>
	internal Action<int>? WorkObserver { get; init; }
}
