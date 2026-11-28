using Icod.Expressions.PackageSmoke;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordPackageConsumerTests {
	public static IEnumerable<object[]> Cases() => PcreWordSmokeCases.All.Select( item => new object[] { item.Id } );

	[Theory]
	[MemberData( nameof( Cases ) )]
	public void WordConsumerPreservesNativeOutcomesAndDeclaredDiagnostics( int id ) {
		var item = PcreWordSmokeCases.All.Single( value => value.Id == id );
		if ( item.Expected.Kind is "M" or "N" ) {
			var source = item.Pattern.Select( unit => (ushort)unit ).ToArray();
			var subject = item.Subject.Select( unit => (ushort)unit ).ToArray();
			var native = item.Literal
				? PcreNative16Oracle.ExecuteLiteral( source, subject, item.Options.Utf16, (uint)item.Start )
				: PcreNative16Oracle.ExecuteSequencePolicies( source, subject, item.Options.Utf16, (uint)item.Start,
					item.Options.Policy.EndAnchored ? 2 : item.Options.Policy.NotEmpty ? 4 : 0,
					item.Options.Ucp, item.Options.AllowLookaroundBackslashK )[0];
			Assert.Equal( item.Expected.Kind, native.Kind );
			Assert.Equal( item.Expected.Captures, native.Captures );
			Assert.Equal( item.Expected.Mark, native.Mark );
		}
		Assert.Equal( item.Expected, PcreWordSmoke.Run( item ) );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void SubjectViewsAndOwnedResultsSurviveReuse( bool utf ) {
		PcreWordSmoke.VerifyObservedView( utf );
		PcreWordSmoke.VerifyReuse( utf );
	}

	[Theory]
	[InlineData( false, false )]
	[InlineData( false, true )]
	[InlineData( true, false )]
	[InlineData( true, true )]
	public void PreCancellationPropagatesFromTheConsumer( bool utf, bool compile ) =>
		Assert.Throws<OperationCanceledException>( () => PcreWordSmoke.VerifyCancellation( utf, compile ) );

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void InterruptedConsumerMatchCanBeReused( bool utf ) => PcreWordSmoke.VerifyInterruptedMatch( utf );
}
