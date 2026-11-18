using System.Text;
using Icod.Expressions.PackageSmoke;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcrePackageConsumerTests {
	public static IEnumerable<object[]> Matching() => PcreSmokeCases.All.Where( c => c.Id <= 32 ).Select( c => new object[] { c.Id } );
	public static IEnumerable<object[]> Diagnostics() => PcreSmokeCases.All.Where( c => c.Id > 32 ).Select( c => new object[] { c.Id } );

	[Theory]
	[MemberData( nameof( Matching ) )]
	public void ConsumerMatchesFrozenNativeByteProtocols( int id ) {
		var item = PcreSmokeCases.All.Single( c => c.Id == id );
		var reference = new PcreRegex8Bit( Convert.FromHexString( item.Pattern ), new UTF8Encoding( false, true ), new PcreRegexSettings {
			Options = PcreOptions.Compiled | ( ( item.Mode & 1 ) != 0 ? PcreOptions.Utf : PcreOptions.None )
				| ( ( item.Mode & 2 ) != 0 ? PcreOptions.Ucp : PcreOptions.None ),
			NewLine = (PcreNewLine)2, BackslashR = (PcreBackslashR)1, ParensLimit = 512
		} );
		var match = reference.Match( Convert.FromHexString( item.Subject ), item.Start );
		var spans = new List<string>();
		if ( match.Success ) {
			for ( var i = 0; i < match.Groups.Count; i++ ) {
				var group = match.Groups[i];
				spans.Add( group.Success ? $"{group.Index}:{group.EndIndex - group.Index}" : "-" );
			}
		}
		var captures = String.Join( ",", spans );
		Assert.Equal( item.Expected.Kind == "M", match.Success );
		Assert.Equal( item.Expected.Captures, captures );
		Assert.Equal( item.Expected.Mark, match.Success ? Convert.ToHexString( match.Mark ) : "" );
		Assert.Equal( item.Expected, PcreSmoke.Run( item ) );
	}

	[Theory]
	[MemberData( nameof( Diagnostics ) )]
	public void ConsumerKeepsDeclaredDiagnosticsAndResourceBoundaries( int id ) {
		var item = PcreSmokeCases.All.Single( c => c.Id == id );
		Assert.Equal( item.Expected, PcreSmoke.Run( item ) );
	}

	[Fact]
	public void ObservedSubjectViewRemainsAuthoritative() => PcreSmoke.VerifyObservedView();

	[Theory]
	[InlineData( true )]
	[InlineData( false )]
	public void CancellationPropagatesFromCompileAndMatch( bool compile ) =>
		Assert.Throws<OperationCanceledException>( () => PcreSmoke.VerifyCancellation( compile ) );
}
