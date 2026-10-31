using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Explicit properties classify all 256 byte values without enabling UTF or UCP.</summary>
public sealed class PcreBytePropertyReferenceTests {
	[Theory]
	[InlineData( "C", "FFFFFFFF000000000000000000000080FFFFFFFF002000000000000000000000", "FFFFFFFF000000000000000000000080FFFFFFFF002000000000000000000000" )]
	[InlineData( "Cc", "FFFFFFFF000000000000000000000080FFFFFFFF000000000000000000000000", "FFFFFFFF000000000000000000000080FFFFFFFF000000000000000000000000" )]
	[InlineData( "Cf", "0000000000000000000000000000000000000000002000000000000000000000", "0000000000000000000000000000000000000000002000000000000000000000" )]
	[InlineData( "Cn", "0000000000000000000000000000000000000000000000000000000000000000", "0000000000000000000000000000000000000000000000000000000000000000" )]
	[InlineData( "Co", "0000000000000000000000000000000000000000000000000000000000000000", "0000000000000000000000000000000000000000000000000000000000000000" )]
	[InlineData( "Cs", "0000000000000000000000000000000000000000000000000000000000000000", "0000000000000000000000000000000000000000000000000000000000000000" )]
	[InlineData( "L", "0000000000000000FEFFFF07FEFFFF070000000000042004FFFF7FFFFFFF7FFF", "0000000000000000FEFFFF07FEFFFF070000000000042004FFFF7FFFFFFF7FFF" )]
	[InlineData( "Ll", "000000000000000000000000FEFFFF07000000000000200000000080FFFF7FFF", "0000000000000000FEFFFF07FEFFFF070000000000002000FFFF7FFFFFFF7FFF" )]
	[InlineData( "Lm", "0000000000000000000000000000000000000000000000000000000000000000", "0000000000000000000000000000000000000000000000000000000000000000" )]
	[InlineData( "Lo", "0000000000000000000000000000000000000000000400040000000000000000", "0000000000000000000000000000000000000000000400040000000000000000" )]
	[InlineData( "Lt", "0000000000000000000000000000000000000000000000000000000000000000", "0000000000000000FEFFFF07FEFFFF070000000000002000FFFF7FFFFFFF7FFF" )]
	[InlineData( "Lu", "0000000000000000FEFFFF07000000000000000000000000FFFF7F7F00000000", "0000000000000000FEFFFF07FEFFFF070000000000002000FFFF7FFFFFFF7FFF" )]
	[InlineData( "LC", "0000000000000000FEFFFF07FEFFFF070000000000002000FFFF7FFFFFFF7FFF", "0000000000000000FEFFFF07FEFFFF070000000000002000FFFF7FFFFFFF7FFF" )]
	[InlineData( "L&", "0000000000000000FEFFFF07FEFFFF070000000000002000FFFF7FFFFFFF7FFF", "0000000000000000FEFFFF07FEFFFF070000000000002000FFFF7FFFFFFF7FFF" )]
	[InlineData( "M", "0000000000000000000000000000000000000000000000000000000000000000", "0000000000000000000000000000000000000000000000000000000000000000" )]
	[InlineData( "Mc", "0000000000000000000000000000000000000000000000000000000000000000", "0000000000000000000000000000000000000000000000000000000000000000" )]
	[InlineData( "Me", "0000000000000000000000000000000000000000000000000000000000000000", "0000000000000000000000000000000000000000000000000000000000000000" )]
	[InlineData( "Mn", "0000000000000000000000000000000000000000000000000000000000000000", "0000000000000000000000000000000000000000000000000000000000000000" )]
	[InlineData( "N", "000000000000FF0300000000000000000000000000000C720000000000000000", "000000000000FF0300000000000000000000000000000C720000000000000000" )]
	[InlineData( "Nd", "000000000000FF03000000000000000000000000000000000000000000000000", "000000000000FF03000000000000000000000000000000000000000000000000" )]
	[InlineData( "Nl", "0000000000000000000000000000000000000000000000000000000000000000", "0000000000000000000000000000000000000000000000000000000000000000" )]
	[InlineData( "No", "000000000000000000000000000000000000000000000C720000000000000000", "000000000000000000000000000000000000000000000C720000000000000000" )]
	[InlineData( "P", "00000000EEF7008C010000B800000028000000008208C0880000000000000000", "00000000EEF7008C010000B800000028000000008208C0880000000000000000" )]
	[InlineData( "Pc", "0000000000000000000000800000000000000000000000000000000000000000", "0000000000000000000000800000000000000000000000000000000000000000" )]
	[InlineData( "Pd", "0000000000200000000000000000000000000000000000000000000000000000", "0000000000200000000000000000000000000000000000000000000000000000" )]
	[InlineData( "Pe", "0000000000020000000000200000002000000000000000000000000000000000", "0000000000020000000000200000002000000000000000000000000000000000" )]
	[InlineData( "Pf", "0000000000000000000000000000000000000000000000080000000000000000", "0000000000000000000000000000000000000000000000080000000000000000" )]
	[InlineData( "Pi", "0000000000000000000000000000000000000000000800000000000000000000", "0000000000000000000000000000000000000000000800000000000000000000" )]
	[InlineData( "Po", "00000000EED4008C0100001000000000000000008200C0800000000000000000", "00000000EED4008C0100001000000000000000008200C0800000000000000000" )]
	[InlineData( "Ps", "0000000000010000000000080000000800000000000000000000000000000000", "0000000000010000000000080000000800000000000000000000000000000000" )]
	[InlineData( "S", "00000000100800700000004001000050000000007CD313010000800000008000", "00000000100800700000004001000050000000007CD313010000800000008000" )]
	[InlineData( "Sc", "00000000100000000000000000000000000000003C0000000000000000000000", "00000000100000000000000000000000000000003C0000000000000000000000" )]
	[InlineData( "Sk", "0000000000000000000000400100000000000000008110010000000000000000", "0000000000000000000000400100000000000000008110010000000000000000" )]
	[InlineData( "Sm", "0000000000080070000000000000005000000000001002000000800000008000", "0000000000080070000000000000005000000000001002000000800000008000" )]
	[InlineData( "So", "0000000000000000000000000000000000000000404201000000000000000000", "0000000000000000000000000000000000000000404201000000000000000000" )]
	[InlineData( "Z", "0000000001000000000000000000000000000000010000000000000000000000", "0000000001000000000000000000000000000000010000000000000000000000" )]
	[InlineData( "Zl", "0000000000000000000000000000000000000000000000000000000000000000", "0000000000000000000000000000000000000000000000000000000000000000" )]
	[InlineData( "Zp", "0000000000000000000000000000000000000000000000000000000000000000", "0000000000000000000000000000000000000000000000000000000000000000" )]
	[InlineData( "Zs", "0000000001000000000000000000000000000000010000000000000000000000", "0000000001000000000000000000000000000000010000000000000000000000" )]
	[InlineData( "Any", "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF" )]
	[InlineData( "Xan", "000000000000FF03FEFFFF07FEFFFF070000000000042C76FFFF7FFFFFFF7FFF", "000000000000FF03FEFFFF07FEFFFF070000000000042C76FFFF7FFFFFFF7FFF" )]
	[InlineData( "Xps", "003E000001000000000000000000000020000000010000000000000000000000", "003E000001000000000000000000000020000000010000000000000000000000" )]
	[InlineData( "Xsp", "003E000001000000000000000000000020000000010000000000000000000000", "003E000001000000000000000000000020000000010000000000000000000000" )]
	[InlineData( "Xwd", "000000000000FF03FEFFFF87FEFFFF070000000000042C76FFFF7FFFFFFF7FFF", "000000000000FF03FEFFFF87FEFFFF070000000000042C76FFFF7FFFFFFF7FFF" )]
	[InlineData( "Xuc", "0000000010000000010000000100000000000000FFFFFFFFFFFFFFFFFFFFFFFF", "0000000010000000010000000100000000000000FFFFFFFFFFFFFFFFFFFFFFFF" )]
	public void EveryByteUsesPinnedPropertyMembershipBeforeEachComplement( string name, string normal, string insensitive ) {
		foreach ( var caseless in new[] { false, true } ) {
			var mask = Convert.FromHexString( caseless ? insensitive : normal );
			var forms = new (string Body, bool Positive)[] {
				($@"\p{{{name}}}", true), ($@"\P{{{name}}}", false),
				($@"\p{{^{name}}}", false), ($@"\P{{^{name}}}", true),
				($@"[\p{{{name}}}]", true), ($@"[^\p{{{name}}}]", false),
				($@"[\P{{{name}}}]", false), ($@"[^\P{{{name}}}]", true)
			};
			foreach ( var (body, positive) in forms ) {
				var pattern = String.Concat( caseless ? "(?i)" : "", @"\A", body, @"\z" );
				var compiled = Compile( pattern );
				var reference = Reference( pattern );
				for ( var value = 0; value < 256; value++ ) {
					var subject = new[] { (byte)value };
					var want = ( ( mask[value >> 3] & ( 1 << ( value & 7 ) ) ) != 0 ) == positive;
					Assert.Equal( want, reference.Match( subject ).Success );
					var actual = compiled.Match( subject );
					Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
					Assert.Equal( want, actual.Match is not null );
					if ( want ) {
						Assert.Equal( new PcreByteCapture( 0, 1 ), actual.Match!.Captures[0] );
					}
				}
			}
		}
	}

	[Theory]
	[InlineData( @"\A(\p{L}+)\z", "E9C0FF", "0:3,0:3" )]
	[InlineData( @"\A(\p{Lu}+)\z", "C9C0", "0:2,0:2" )]
	[InlineData( @"\A(\p{Lu}+)\z", "E9C0", "" )]
	[InlineData( @"(?i)\A(\p{Lu}+)\z", "E9C0", "0:2,0:2" )]
	[InlineData( @"(?i)\A(\p{Lt}+)\z", "E9C0", "0:2,0:2" )]
	[InlineData( @"(?i)\A(\P{Ll}+)\z", "E9C0", "" )]
	[InlineData( @"\A(\pL)(\PN)\z", "E9FF", "0:2,0:1,1:1" )]
	[InlineData( @"\A(\p{ x_w-d }+)(\p{ _- ^ _-Xwd})\z", "E94121", "0:3,0:2,2:1" )]
	[InlineData( @"\A[^\P{L}A]+\z", "E9C9", "0:2" )]
	[InlineData( @"\A[\p{Xan}\p{Xps}]+\z", "B285A020", "0:4" )]
	[InlineData( @"\A(\p{Any}{3})\z", "00FF85", "0:3,0:3" )]
	[InlineData( @"\A(\p{Xuc}+)\z", "2440A0FF", "0:4,0:4" )]
	[InlineData( @"\A(\P{Xuc}+)\z", "001F85", "0:3,0:3" )]
	[InlineData( @"(?i)\A(\p{Lu})(?-i:(\p{Lu}))\z", "E9C9", "0:2,0:1,1:1" )]
	[InlineData( @"(?i)\A(\p{Lu})(?-i:(\p{Lu}))\z", "E9E9", "" )]
	[InlineData( @"(?i)\A(\p{Ll})(?^:(\p{Ll}))\z", "C9E9", "0:2,0:1,1:1" )]
	[InlineData( @"(?ia)\A(\p{Lu}+)(\w)\z", "C9E941", "0:3,0:2,2:1" )]
	[InlineData( @"(?ir)\A(\p{Lu}+)\z", "C9E9", "0:2,0:2" )]
	[InlineData( @"\A(?:(\p{L}+)x|(\p{L}+))\z", "E9FF", "0:2,-,0:2" )]
	[InlineData( @"\A(\p{L}{1,3}?)\p{L}\z", "E9C9FF", "0:3,0:2" )]
	[InlineData( @"\A(\p{L}++)X\z", "E94158", "" )]
	[InlineData( @"\A(\p{L}+)\1\z", "E9FFE9FF", "0:4,0:2" )]
	[InlineData( @"(?i)\A(\p{Lu}+)\1\z", "C9E9", "" )]
	[InlineData( @"(?=(\p{L}{2}))\1", "E9FF", "0:2,0:2" )]
	[InlineData( @"(?!(\p{N}))\p{Any}", "E9", "0:1,-" )]
	[InlineData( @"(?*(\p{L}+))\1X", "E9E958", "0:3,0:2" )]
	[InlineData( @"(?<=(\p{L}))X", "E958", "1:1,0:1" )]
	[InlineData( @"(?<=\p{L}{1,2})X", "E9FF58", "2:1" )]
	[InlineData( @"(?<!\p{Xwd})X", "A058", "1:1" )]
	[InlineData( @"\A(?&a)\z(?(DEFINE)(?<a>\p{L}+))", "E9FF", "0:2,-" )]
	[InlineData( @"\A(?<a>\p{L}(?&a)?\p{Xps})\z", "E9FF2020", "0:4,0:4" )]
	[InlineData( @"\A(?|(\p{Lu})|(\p{Ll}))\1\z", "E9E9", "0:2,0:1" )]
	[InlineData( @"\A(?:(\p{L})|\p{N})(?(1)\p{L}|\p{N})\z", "E9FF", "0:2,0:1" )]
	[InlineData( @"\A\p{Any}\K(\p{Any})\z", "FF00", "1:1,1:1" )]
	[InlineData( @"\Q\p{L}\E", "5C707B4C7D", "0:5" )]
	[InlineData( @"(?xx)\A[ \p{L} \p{Xps} ]+\z", "E9A0", "0:2" )]
	[InlineData( @"\A(\p{Any}{2})\z", "C3A9", "0:2,0:2" )]
	[InlineData( @"\A\p{L}\z", "C3A9", "" )]
	[InlineData( @"\A\p{C}\z", "85", "0:1" )]
	[InlineData( @"\A\p{Zs}\z", "A0", "0:1" )]
	[InlineData( @"(*ANYCRLF)\p{Xps}A", "0D0A410D0A", "1:2" )]
	[InlineData( @"(*ANYCRLF)[\p{Xps}]A", "0D0A410D0A", "1:2" )]
	[InlineData( @"(*ANYCRLF)[^\P{Xps}]A", "0D0A410D0A", "1:2" )]
	[InlineData( @"(*ANYCRLF)[\p{Xps}\p{Xps}]A", "0D0A410D0A", "1:2" )]
	[InlineData( @"(*ANYCRLF)[\p{Xps} ]A", "0D0A410D0A", "1:2" )]
	[InlineData( @"(*ANYCRLF)[\p{Xps}X]A", "0D0A410D0A", "1:2" )]
	[InlineData( @"(*ANYCRLF)[\p{Cc}\p{Zs}]A", "0D0A410D0A", "1:2" )]
	[InlineData( @"(*ANYCRLF)[\p{Any}X]A", "0D0A410D0A", "1:2" )]
	[InlineData( @"(*ANYCRLF)[\P{L}]A", "0D0A410D0A", "1:2" )]
	[InlineData( @"(*ANYCRLF)(\p{Xps})A", "0D0A410D0A", "1:2,1:1" )]
	[InlineData( @"(*ANYCRLF)(?:(\p{Xps})A)", "0D0A410D0A", "1:2,1:1" )]
	[InlineData( @"(*ANYCRLF)(?:\p{Xps})(A)", "0D0A410D0A", "1:2,2:1" )]
	[InlineData( @"(*ANYCRLF)(?i)\p{Xps}a", "0D0A410D0A", "1:2" )]
	[InlineData( @"(*ANYCRLF)(?xx)[ \p{Xps} X ]A", "0D0A410D0A", "1:2" )]
	[InlineData( @"(*ANYCRLF)[^\p{L}]A", "0D0A410D0A", "1:2" )]
	[InlineData( @"(*ANYCRLF)[^\x00\x01]A", "0D0A410D0A", "" )]
	[InlineData( @"(*ANYCRLF)\p{L}A", "0D0A410D0A", "" )]
	[InlineData( @"(*ANYCRLF)[\p{Zs}]A", "0D0A410D0A", "" )]
	public void ByteSpansAndCaptureTransactionsMatchPinnedNative( string pattern, string hex, string spans ) {
		var subject = Convert.FromHexString( hex );
		var reference = Reference( pattern );
		AssertLiteral( reference, subject, spans );
		Compare( reference, subject, 0, Compile( pattern ).Match( subject ), pattern );
	}

	[Theory]
	[InlineData( "(*LF)" )]
	[InlineData( "(*ANYCRLF)" )]
	[InlineData( "(*CR)" )]
	[InlineData( "(*NUL)" )]
	[InlineData( "(*CRLF)" )]
	[InlineData( "(*ANY)" )]
	public void EveryByteOffsetRetainsClassProvenanceAndOriginalSearchStart( string prefix ) {
		string[] bodies = {
			@"\p{Any}", @"[\p{Any}]", @"\p{Xps}A", @"[\p{Xps}]A", @"\P{L}A",
			@"\p{Any}\n", @"[\p{Any}]\n", @"[\p{Xps}\n]A", @"[^\P{Xps}\n]A",
			@"(?:\p{Any}|\n)A", @"(?:\p{Xps}){0}A", @"A(?(DEFINE)(?<x>\p{Any}))",
			@"\G(\p{Any})", @"(?<=(\p{Any}))A", @"(?<=\p{L}{1,2})A", @"(\p{Any}?)|A"
		};
		foreach ( var body in bodies ) {
			var pattern = prefix + body;
			var reference = Reference( pattern );
			var compiled = Compile( pattern );
			foreach ( var hex in new[] { "", "00", "0D0A410D0A", "FF85A041", "C3A941", "E9FF41", "410D0A", "85A0" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}/{hex}/{offset}" );
				}
			}
		}
	}

	[Theory]
	[InlineData( @"\p", 0 )]
	[InlineData( @"\p{", 0 )]
	[InlineData( @"\p{}", 0 )]
	[InlineData( @"\P{^}", 0 )]
	[InlineData( @"x\p{L", 1 )]
	[InlineData( @"[\p{L}-z]", 6 )]
	[InlineData( @"[a-\p{L}]", 2 )]
	public void MalformedNamesAndNonliteralRangeEndpointsKeepOriginalByteErrors( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		Diagnostic( Encoding.ASCII.GetBytes( pattern ), PcreByteDiagnosticCode.InvalidSyntax, offset );
	}

	[Theory]
	[InlineData( @"\p{Greek}", 0 )]
	[InlineData( @"\p{Alphabetic}", 0 )]
	[InlineData( @"\p{Script=Greek}", 0 )]
	[InlineData( @"\p{gc:Any}", 0 )]
	[InlineData( @"[\p{Alphabetic}]", 1 )]
	[InlineData( @"\p{^^Xwd}", 0 )]
	public void UnimplementedPropertyFamiliesStayExplicitlyRejected( string pattern, int offset ) =>
		Diagnostic( Encoding.ASCII.GetBytes( pattern ), PcreByteDiagnosticCode.UnsupportedSyntax, offset );

	[Theory]
	[InlineData( @"\p{gc:Lu}" )]
	[InlineData( @"\p{general_category=Lu}" )]
	public void ExistingCategoryNamespaceExtensionUsesTheSharedByteDomain( string property ) {
		var compiled = Compile( String.Concat( @"\A", property, @"\z" ) );
		Assert.Equal( 1, compiled.Match( new byte[] { 0xC9 } ).Match?.ByteLength );
		Assert.Null( compiled.Match( new byte[] { 0xE9 } ).Match );
	}

	[Fact]
	public void PropertyAcceptanceDoesNotEnableUcpUtfOrHighByteLiteralFolding() {
		var bytes = new byte[] { 0xFF }.Concat( Encoding.ASCII.GetBytes( @"(\p{L})" ) ).ToArray();
		var bytePattern = PcreByteCompiler.Compile( bytes );
		Assert.True( bytePattern.IsSuccess, bytePattern.Diagnostic?.Message );
		Assert.Equal( 2, bytePattern.Pattern!.Match( new byte[] { 0xFF, 0xE9 } ).Match?.ByteLength );
		Diagnostic( bytes, PcreByteDiagnosticCode.InvalidUtf8Pattern, 0, new() { Utf8 = true } );
		Diagnostic( Encoding.ASCII.GetBytes( @"(*UCP)\p{L}" ), PcreByteDiagnosticCode.UnsupportedSyntax, 0 );
		Diagnostic( Encoding.ASCII.GetBytes( @"\p{L}" ), PcreByteDiagnosticCode.UnsupportedSyntax, 0, new() { Ucp = true } );
		Assert.Null( Compile( @"(?i)\A\xC9\z" ).Match( new byte[] { 0xE9 } ).Match );
		Assert.Null( Compile( @"(?i)\A(\p{Lu})\1\z" ).Match( new byte[] { 0xC9, 0xE9 } ).Match );
		Assert.Equal( 2, Compile( @"(*UTF)\A\p{L}\z" ).Match( "é"u8.ToArray() ).Match?.ByteLength );
		Assert.Null( Compile( @"\A\p{L}\z" ).Match( "é"u8.ToArray() ).Match );
		var compiled = Compile( @"\p{L}" );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset, compiled.Match( new byte[] { 0xE9 }, -1 ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset, compiled.Match( new byte[] { 0xE9 }, 2 ).Diagnostic?.Code );
	}

	[Fact]
	public void ProjectedPropertiesSpendOrdinaryClassWorkAndHonorBudgetsAndCancellation() {
		const string property = @"\A\p{L}+\z";
		const string ordinary = @"\A[A-Za-z\xAA\xB5\xBA\xC0-\xD6\xD8-\xF6\xF8-\xFF]+\z";
		var subject = new byte[] { 0xE9, 0xFF, 0x41 };
		var propertySteps = new List<int>();
		var ordinarySteps = new List<int>();
		Assert.Equal( 3, Compile( property, new() { WorkObserver = propertySteps.Add } ).Match( subject ).Match?.ByteLength );
		Assert.Equal( 3, Compile( ordinary, new() { WorkObserver = ordinarySteps.Add } ).Match( subject ).Match?.ByteLength );
		Assert.Equal( ordinarySteps, propertySteps );
		Assert.Equal( Enumerable.Range( 1, propertySteps.Count ), propertySteps );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( property, new() { MatchWorkLimit = propertySteps.Count - 1 } ).Match( subject ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( property, new() { WorkObserver = count => { if ( count == 4 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( property ), cancellationToken: cancellation.Token ) );
	}

	[Fact]
	public void ProjectedPropertiesRetainSharedCompileAndSavedPathLimits() {
		var nested = String.Concat( new string( '(', 257 ), @"\p{L}", new string( ')', 257 ) );
		Assert.Equal( PcreByteDiagnosticCode.PatternDepthLimitExceeded,
			PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( nested ) ).Diagnostic?.Code );
		var captures = String.Concat( Enumerable.Repeat( @"(\p{L})", 257 ) );
		Assert.Equal( PcreByteDiagnosticCode.PatternCaptureLimitExceeded,
			PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( captures ) ).Diagnostic?.Code );
		var expanded = String.Concat( Enumerable.Repeat( @"\p{L}", 4100 ) );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded,
			PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( expanded ) ).Diagnostic?.Code );
		const string recursive = @"\A(?<a>\p{L}(?&a)?\p{Xps})\z";
		Assert.Equal( PcreByteDiagnosticCode.MatchDepthLimitExceeded, Compile( recursive )
			.Match( Encoding.ASCII.GetBytes( new string( 'a', 1200 ) + new string( ' ', 1200 ) ) ).Diagnostic?.Code );
	}

	private static void Diagnostic( byte[] source, PcreByteDiagnosticCode code, int offset, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( source, options );
		Assert.Null( result.Pattern );
		Assert.Equal( code, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

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
		Assert.Equal( expected.Length, actual.Match!.ByteLength );
		Assert.Equal( expected.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < expected.Groups.Count; group++ ) {
			var capture = expected.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null, actual.Match.Captures[group] );
		}
	}

	private static PcreRegex8Bit Reference( string pattern ) => new( Encoding.ASCII.GetBytes( pattern ), Encoding.Latin1,
		new PcreRegexSettings { Options = PcreOptions.Compiled, NewLine = PcreNewLine.Lf, ParensLimit = 512 } );

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
