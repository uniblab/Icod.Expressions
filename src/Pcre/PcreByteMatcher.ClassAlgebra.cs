namespace Icod.Expressions.Pcre;

internal static partial class PcreByteMatcher {
	private static bool ContainsScalarExpression( ReadOnlyMemory<PcreScalarSetTerm> terms, int scalar, bool[] stack,
		ref int work, int matchWorkLimit, Action<int>? observer, bool turkishCasing, CancellationToken cancellationToken ) {
		var result = PcreScalarClassExpression.Evaluate( terms, scalar, stack, true, turkishCasing,
			ref work, matchWorkLimit, observer, cancellationToken );
		if ( result < 0 ) { throw new MatchLimitException( PcreByteDiagnosticCode.MatchWorkLimitExceeded, "match work limit exceeded" ); }
		return result == 1;
	}
}
