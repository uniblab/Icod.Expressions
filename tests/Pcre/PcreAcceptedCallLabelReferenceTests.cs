using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Native JIT accepted-call label retention depends on lexical local-quit capability.</summary>
public sealed class PcreAcceptedCallLabelReferenceTests {
	[Theory]
	[InlineData( "(*MARK:M)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(*ACCEPT:N)|ab))", "0:3,-", "4E" )]
	[InlineData( "(*MARK:M)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(*MARK:N)(*ACCEPT)|ab))", "0:3,-", "4E" )]
	[InlineData( "(*MARK:M)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(*MARK:N)|ab))", "0:3,-", "4D" )]
	[InlineData( "(*MARK:M)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(*PRUNE:N)(*ACCEPT)|ab))", "0:3,-", "4D" )]
	[InlineData( "(*MARK:M)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(*ACCEPT:N)(*PRUNE)))", "0:3,-", "4D" )]
	[InlineData( "(*MARK:M)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(*ACCEPT:N)(?:b(*PRUNE)){0}))", "0:3,-", "4D" )]
	[InlineData( "(*MARK:M)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(*ACCEPT:N)(?(DEFINE)(*PRUNE))))", "0:3,-", "4D" )]
	[InlineData( "(*MARK:M)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(?=(*MARK:N))(*ACCEPT)))", "0:3,-", "4E" )]
	[InlineData( "(*MARK:M)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(?=(*ACCEPT:N))))", "0:3,-", "4D" )]
	[InlineData( "(*MARK:M)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(?&y)(*ACCEPT:N))(?<y>(*PRUNE:P)))", "0:3,-,-", "4E" )]
	[InlineData( "\\A(?&x)c|abc(?(DEFINE)(?<x>a(*ACCEPT:N)|ab))", "0:3,-", "4E" )]
	[InlineData( "(*MARK:M)\\A(?&x)c|abc(?(DEFINE)(?<x>a(*ACCEPT:N)|ab))", "0:3,-", "" )]
	[InlineData( "\\A(?!(?&x)c)abc(?(DEFINE)(?<x>a(*ACCEPT:N)))", "0:3,-", "4E" )]
	[InlineData( "\\A(?(?=(?&x)c)X|abc)(?(DEFINE)(?<x>a(*ACCEPT:N)))", "0:3,-", "4E" )]
	public void AcceptedCallLabelsRespectLexicalQuitAndOlderCallerCheckpoints( string pattern, string spans, string markHex ) {
		var subject = "abc"u8.ToArray();
		foreach ( var utf in new[] { false, true } ) {
			var options = new PcreByteOptions { Utf8 = utf, Ucp = utf };
			var reference = Reference( pattern, options ); AssertLiteral( reference, subject, spans );
			Assert.Equal( markHex, Convert.ToHexString( reference.Match( subject ).Mark ) );
			Compare( reference, subject, 0, Compile( pattern, options ).Match( subject ), pattern );
		}
	}

	private static ReadOnlyMemory<byte> Mark( PcreByteMatch match ) => match.MarkName;

	private static void AssertLiteral( PcreRegex8Bit reference, byte[] subject, string spans ) {
		var match = reference.Match( subject );
		Assert.Equal( spans.Length > 0, match.Success );
		if ( !match.Success ) { return; }
		var groups = spans.Split( ',' );
		Assert.Equal( groups.Length, match.Groups.Count );
		for ( var group = 0; group < groups.Length; group++ ) {
			var capture = match.Groups[group];
			Assert.Equal( groups[group], capture.Success ? $"{capture.Index}:{capture.Length}" : "-" );
		}
	}

	private static void Compare( PcreRegex8Bit reference, byte[] subject, int offset, PcreByteMatchResult actual, string context ) {
		var expected = reference.Match( subject, offset );
		Assert.True( actual.IsSuccess, $"{context}: {actual.Diagnostic?.Message}" );
		Assert.True( ( expected.Success ? expected.Index : -1 ) == ( actual.Match?.ByteIndex ?? -1 ), context );
		if ( !expected.Success ) { return; }
		Assert.Equal( expected.Mark.ToArray(), Mark( actual.Match! ).ToArray() );
		Assert.Equal( expected.Length, actual.Match!.ByteLength );
		Assert.Equal( expected.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < expected.Groups.Count; group++ ) {
			var capture = expected.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null, actual.Match.Captures[group] );
		}
	}

	private static PcreRegex8Bit Reference( string pattern, PcreByteOptions options ) => Reference( Encoding.UTF8.GetBytes( pattern ), options );
	private static PcreRegex8Bit Reference( byte[] pattern, PcreByteOptions options ) => new( pattern, Encoding.UTF8, new PcreRegexSettings {
		Options = PcreOptions.Compiled | ( options.Caseless ? PcreOptions.Caseless : PcreOptions.None ) | ( options.Utf8 || options.MatchInvalidUtf ? PcreOptions.Utf : PcreOptions.None )
			| ( options.Ucp ? PcreOptions.Ucp : PcreOptions.None ) | ( options.MatchInvalidUtf ? PcreOptions.MatchInvalidUtf : PcreOptions.None ),
		NewLine = options.AnyCrLfNewline ? PcreNewLine.AnyCrLf : PcreNewLine.Lf,
		ExtraCompileOptions = options.AsciiBsD ? PcreExtraCompileOptions.AsciiBsD : PcreExtraCompileOptions.None,
		BackslashR = options.BsrAnyCrLf ? PcreBackslashR.AnyCrLf : PcreBackslashR.Unicode,
		ParensLimit = 512
	} );

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions options ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
