using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pins equivalent-count exits, retained assertion evaluations and capture restoration.</summary>
public sealed class PcreUtfNegativeAssertionExitReferenceTests {
	[Theory]
	[InlineData( @"(?!\BB){0,3}([\x09-\x0c\x{100}])A", "0D0A41", false, "1:2,1:1" )]
	[InlineData( @"(?!\BB){0,3}([\x09-\x0c\x{100}])A", "0B41", false, "0:2,0:1" )]
	[InlineData( @"(?!\BB){0,3}([\x09-\x0c\x{100}])A", "420B4142", false, "1:2,1:1" )]
	[InlineData( @"(?!\BB){0,3}([\x09-\x0c\x{100}])A", "42420B4142", false, "2:2,2:1" )]
	[InlineData( @"(?!(\BB)Z){0,3}([\x09-\x0c\x{100}])A", "0D0A41", false, "1:2,-,1:1" )]
	[InlineData( @"(?!(\BB)Z){0,3}([\x09-\x0c\x{100}])A", "0B41", false, "0:2,-,0:1" )]
	[InlineData( @"(?!(\BB)Z){0,3}([\x09-\x0c\x{100}])A", "420B4142", false, "1:2,-,1:1" )]
	[InlineData( @"(?!(\BB)Z){0,3}([\x09-\x0c\x{100}])A", "42420B4142", false, "2:2,-,2:1" )]
	[InlineData( @"(?!(?=(B))BZ){0,3}([\x09-\x0c\x{100}])A", "0D0A41", false, "1:2,-,1:1" )]
	[InlineData( @"(?!(?=(B))BZ){0,3}([\x09-\x0c\x{100}])A", "0B41", false, "0:2,-,0:1" )]
	[InlineData( @"(?!(?=(B))BZ){0,3}([\x09-\x0c\x{100}])A", "420B4142", false, "1:2,-,1:1" )]
	[InlineData( @"(?!(?=(B))BZ){0,3}([\x09-\x0c\x{100}])A", "42420B4142", false, "2:2,-,2:1" )]
	[InlineData( @"\A(B)(?!(\bB)Z){0,3}([\x09-\x0c\x{100}])A\1?", "0D0A41", false, "" )]
	[InlineData( @"\A(B)(?!(\bB)Z){0,3}([\x09-\x0c\x{100}])A\1?", "0B41", false, "" )]
	[InlineData( @"\A(B)(?!(\bB)Z){0,3}([\x09-\x0c\x{100}])A\1?", "420B4142", false, "0:4,0:1,-,1:1" )]
	[InlineData( @"\A(B)(?!(\bB)Z){0,3}([\x09-\x0c\x{100}])A\1?", "42420B4142", false, "" )]
	[InlineData( @"\A(?:(?|(B)|(?!(C)Z){0,3})){2}([\x09-\x0c\x{100}])A\1?", "0D0A41", false, "" )]
	[InlineData( @"\A(?:(?|(B)|(?!(C)Z){0,3})){2}([\x09-\x0c\x{100}])A\1?", "0B41", false, "0:2,-,0:1" )]
	[InlineData( @"\A(?:(?|(B)|(?!(C)Z){0,3})){2}([\x09-\x0c\x{100}])A\1?", "420B4142", false, "0:4,0:1,1:1" )]
	[InlineData( @"\A(?:(?|(B)|(?!(C)Z){0,3})){2}([\x09-\x0c\x{100}])A\1?", "42420B4142", false, "0:5,1:1,2:1" )]
	[InlineData( @"\A(?<x>B)(?!(?&x)Z){0,3}([\x09-\x0c\x{100}])A\k<x>?", "0D0A41", false, "" )]
	[InlineData( @"\A(?<x>B)(?!(?&x)Z){0,3}([\x09-\x0c\x{100}])A\k<x>?", "0B41", false, "" )]
	[InlineData( @"\A(?<x>B)(?!(?&x)Z){0,3}([\x09-\x0c\x{100}])A\k<x>?", "420B4142", false, "0:4,0:1,1:1" )]
	[InlineData( @"\A(?<x>B)(?!(?&x)Z){0,3}([\x09-\x0c\x{100}])A\k<x>?", "42420B4142", false, "" )]
	[InlineData( @"\A(?<x>B)(?!(?<=(B))C){0,3}([\x09-\x0c\x{100}])A(?(2)B|)", "0D0A41", false, "" )]
	[InlineData( @"\A(?<x>B)(?!(?<=(B))C){0,3}([\x09-\x0c\x{100}])A(?(2)B|)", "0B41", false, "" )]
	[InlineData( @"\A(?<x>B)(?!(?<=(B))C){0,3}([\x09-\x0c\x{100}])A(?(2)B|)", "420B4142", false, "0:3,0:1,-,1:1" )]
	[InlineData( @"\A(?<x>B)(?!(?<=(B))C){0,3}([\x09-\x0c\x{100}])A(?(2)B|)", "42420B4142", false, "" )]
	[InlineData( @"\A(B)(?!(\BB)Z){0,3}+([\x09-\x0c\x{100}])A\1?", "0D0A41", false, "" )]
	[InlineData( @"\A(B)(?!(\BB)Z){0,3}+([\x09-\x0c\x{100}])A\1?", "0B41", false, "" )]
	[InlineData( @"\A(B)(?!(\BB)Z){0,3}+([\x09-\x0c\x{100}])A\1?", "420B4142", false, "0:4,0:1,-,1:1" )]
	[InlineData( @"\A(B)(?!(\BB)Z){0,3}+([\x09-\x0c\x{100}])A\1?", "42420B4142", false, "" )]
	[InlineData( @"(?!\BB){0,3}?([\x09-\x0c\x{100}])A", "0D0A41", false, "1:2,1:1" )]
	[InlineData( @"(?!\BB){0,3}?([\x09-\x0c\x{100}])A", "0B41", false, "0:2,0:1" )]
	[InlineData( @"(?!\BB){0,3}?([\x09-\x0c\x{100}])A", "420B4142", false, "1:2,1:1" )]
	[InlineData( @"(?!\BB){0,3}?([\x09-\x0c\x{100}])A", "42420B4142", false, "2:2,2:1" )]
	[InlineData( @"(?!\BB){1,3}([\x09-\x0c\x{100}])A", "0D0A41", false, "1:2,1:1" )]
	[InlineData( @"(?!\BB){1,3}([\x09-\x0c\x{100}])A", "0B41", false, "0:2,0:1" )]
	[InlineData( @"(?!\BB){1,3}([\x09-\x0c\x{100}])A", "420B4142", false, "1:2,1:1" )]
	[InlineData( @"(?!\BB){1,3}([\x09-\x0c\x{100}])A", "42420B4142", false, "2:2,2:1" )]
	[InlineData( @"(?!\BB){0,1}([\x09-\x0c\x{100}])A", "0D0A41", false, "1:2,1:1" )]
	[InlineData( @"(?!\BB){0,1}([\x09-\x0c\x{100}])A", "0B41", false, "0:2,0:1" )]
	[InlineData( @"(?!\BB){0,1}([\x09-\x0c\x{100}])A", "420B4142", false, "1:2,1:1" )]
	[InlineData( @"(?!\BB){0,1}([\x09-\x0c\x{100}])A", "42420B4142", false, "2:2,2:1" )]
	[InlineData( @"(?!B){0,3}([\x09-\x0c\x{100}])A", "0D0A41", false, "1:2,1:1" )]
	[InlineData( @"(?!B){0,3}([\x09-\x0c\x{100}])A", "0B41", false, "0:2,0:1" )]
	[InlineData( @"(?!B){0,3}([\x09-\x0c\x{100}])A", "420B4142", false, "1:2,1:1" )]
	[InlineData( @"(?!B){0,3}([\x09-\x0c\x{100}])A", "42420B4142", false, "2:2,2:1" )]
	[InlineData( @"(?U)(?!\BB){0,3}?([\x09-\x0c\x{100}])A", "0D0A41", false, "1:2,1:1" )]
	[InlineData( @"(?U)(?!\BB){0,3}?([\x09-\x0c\x{100}])A", "0B41", false, "0:2,0:1" )]
	[InlineData( @"(?U)(?!\BB){0,3}?([\x09-\x0c\x{100}])A", "420B4142", false, "1:2,1:1" )]
	[InlineData( @"(?U)(?!\BB){0,3}?([\x09-\x0c\x{100}])A", "42420B4142", false, "2:2,2:1" )]
	[InlineData( @"(?U)(?!\BB){0,3}([\x09-\x0c\x{100}])A", "0D0A41", false, "1:2,1:1" )]
	[InlineData( @"(?U)(?!\BB){0,3}([\x09-\x0c\x{100}])A", "0B41", false, "0:2,0:1" )]
	[InlineData( @"(?U)(?!\BB){0,3}([\x09-\x0c\x{100}])A", "420B4142", false, "1:2,1:1" )]
	[InlineData( @"(?U)(?!\BB){0,3}([\x09-\x0c\x{100}])A", "42420B4142", false, "2:2,2:1" )]
	[InlineData( @"(?!\BB){0,3}([\x09-\x0c\x{100}])A", "0D0A41", true, "1:2,1:1" )]
	[InlineData( @"(?!\BB){0,3}([\x09-\x0c\x{100}])A", "0B41", true, "0:2,0:1" )]
	[InlineData( @"(?!\BB){0,3}([\x09-\x0c\x{100}])A", "420B4142", true, "1:2,1:1" )]
	[InlineData( @"(?!\BB){0,3}([\x09-\x0c\x{100}])A", "42420B4142", true, "2:2,2:1" )]
	[InlineData( @"(?!(\BB)Z){0,3}([\x09-\x0c\x{100}])A", "0D0A41", true, "1:2,-,1:1" )]
	[InlineData( @"(?!(\BB)Z){0,3}([\x09-\x0c\x{100}])A", "0B41", true, "0:2,-,0:1" )]
	[InlineData( @"(?!(\BB)Z){0,3}([\x09-\x0c\x{100}])A", "420B4142", true, "1:2,-,1:1" )]
	[InlineData( @"(?!(\BB)Z){0,3}([\x09-\x0c\x{100}])A", "42420B4142", true, "2:2,-,2:1" )]
	[InlineData( @"(?!(?=(B))BZ){0,3}([\x09-\x0c\x{100}])A", "0D0A41", true, "1:2,-,1:1" )]
	[InlineData( @"(?!(?=(B))BZ){0,3}([\x09-\x0c\x{100}])A", "0B41", true, "0:2,-,0:1" )]
	[InlineData( @"(?!(?=(B))BZ){0,3}([\x09-\x0c\x{100}])A", "420B4142", true, "1:2,-,1:1" )]
	[InlineData( @"(?!(?=(B))BZ){0,3}([\x09-\x0c\x{100}])A", "42420B4142", true, "2:2,-,2:1" )]
	[InlineData( @"\A(B)(?!(\bB)Z){0,3}([\x09-\x0c\x{100}])A\1?", "0D0A41", true, "" )]
	[InlineData( @"\A(B)(?!(\bB)Z){0,3}([\x09-\x0c\x{100}])A\1?", "0B41", true, "" )]
	[InlineData( @"\A(B)(?!(\bB)Z){0,3}([\x09-\x0c\x{100}])A\1?", "420B4142", true, "0:4,0:1,-,1:1" )]
	[InlineData( @"\A(B)(?!(\bB)Z){0,3}([\x09-\x0c\x{100}])A\1?", "42420B4142", true, "" )]
	[InlineData( @"\A(?:(?|(B)|(?!(C)Z){0,3})){2}([\x09-\x0c\x{100}])A\1?", "0D0A41", true, "" )]
	[InlineData( @"\A(?:(?|(B)|(?!(C)Z){0,3})){2}([\x09-\x0c\x{100}])A\1?", "0B41", true, "0:2,-,0:1" )]
	[InlineData( @"\A(?:(?|(B)|(?!(C)Z){0,3})){2}([\x09-\x0c\x{100}])A\1?", "420B4142", true, "0:4,0:1,1:1" )]
	[InlineData( @"\A(?:(?|(B)|(?!(C)Z){0,3})){2}([\x09-\x0c\x{100}])A\1?", "42420B4142", true, "0:5,1:1,2:1" )]
	[InlineData( @"\A(?<x>B)(?!(?&x)Z){0,3}([\x09-\x0c\x{100}])A\k<x>?", "0D0A41", true, "" )]
	[InlineData( @"\A(?<x>B)(?!(?&x)Z){0,3}([\x09-\x0c\x{100}])A\k<x>?", "0B41", true, "" )]
	[InlineData( @"\A(?<x>B)(?!(?&x)Z){0,3}([\x09-\x0c\x{100}])A\k<x>?", "420B4142", true, "0:4,0:1,1:1" )]
	[InlineData( @"\A(?<x>B)(?!(?&x)Z){0,3}([\x09-\x0c\x{100}])A\k<x>?", "42420B4142", true, "" )]
	[InlineData( @"\A(?<x>B)(?!(?<=(B))C){0,3}([\x09-\x0c\x{100}])A(?(2)B|)", "0D0A41", true, "" )]
	[InlineData( @"\A(?<x>B)(?!(?<=(B))C){0,3}([\x09-\x0c\x{100}])A(?(2)B|)", "0B41", true, "" )]
	[InlineData( @"\A(?<x>B)(?!(?<=(B))C){0,3}([\x09-\x0c\x{100}])A(?(2)B|)", "420B4142", true, "0:3,0:1,-,1:1" )]
	[InlineData( @"\A(?<x>B)(?!(?<=(B))C){0,3}([\x09-\x0c\x{100}])A(?(2)B|)", "42420B4142", true, "" )]
	[InlineData( @"\A(B)(?!(\BB)Z){0,3}+([\x09-\x0c\x{100}])A\1?", "0D0A41", true, "" )]
	[InlineData( @"\A(B)(?!(\BB)Z){0,3}+([\x09-\x0c\x{100}])A\1?", "0B41", true, "" )]
	[InlineData( @"\A(B)(?!(\BB)Z){0,3}+([\x09-\x0c\x{100}])A\1?", "420B4142", true, "0:4,0:1,-,1:1" )]
	[InlineData( @"\A(B)(?!(\BB)Z){0,3}+([\x09-\x0c\x{100}])A\1?", "42420B4142", true, "" )]
	[InlineData( @"(?!\BB){0,3}?([\x09-\x0c\x{100}])A", "0D0A41", true, "1:2,1:1" )]
	[InlineData( @"(?!\BB){0,3}?([\x09-\x0c\x{100}])A", "0B41", true, "0:2,0:1" )]
	[InlineData( @"(?!\BB){0,3}?([\x09-\x0c\x{100}])A", "420B4142", true, "1:2,1:1" )]
	[InlineData( @"(?!\BB){0,3}?([\x09-\x0c\x{100}])A", "42420B4142", true, "2:2,2:1" )]
	[InlineData( @"(?!\BB){1,3}([\x09-\x0c\x{100}])A", "0D0A41", true, "1:2,1:1" )]
	[InlineData( @"(?!\BB){1,3}([\x09-\x0c\x{100}])A", "0B41", true, "0:2,0:1" )]
	[InlineData( @"(?!\BB){1,3}([\x09-\x0c\x{100}])A", "420B4142", true, "1:2,1:1" )]
	[InlineData( @"(?!\BB){1,3}([\x09-\x0c\x{100}])A", "42420B4142", true, "2:2,2:1" )]
	[InlineData( @"(?!\BB){0,1}([\x09-\x0c\x{100}])A", "0D0A41", true, "1:2,1:1" )]
	[InlineData( @"(?!\BB){0,1}([\x09-\x0c\x{100}])A", "0B41", true, "0:2,0:1" )]
	[InlineData( @"(?!\BB){0,1}([\x09-\x0c\x{100}])A", "420B4142", true, "1:2,1:1" )]
	[InlineData( @"(?!\BB){0,1}([\x09-\x0c\x{100}])A", "42420B4142", true, "2:2,2:1" )]
	[InlineData( @"(?!B){0,3}([\x09-\x0c\x{100}])A", "0D0A41", true, "1:2,1:1" )]
	[InlineData( @"(?!B){0,3}([\x09-\x0c\x{100}])A", "0B41", true, "0:2,0:1" )]
	[InlineData( @"(?!B){0,3}([\x09-\x0c\x{100}])A", "420B4142", true, "1:2,1:1" )]
	[InlineData( @"(?!B){0,3}([\x09-\x0c\x{100}])A", "42420B4142", true, "2:2,2:1" )]
	[InlineData( @"(?U)(?!\BB){0,3}?([\x09-\x0c\x{100}])A", "0D0A41", true, "1:2,1:1" )]
	[InlineData( @"(?U)(?!\BB){0,3}?([\x09-\x0c\x{100}])A", "0B41", true, "0:2,0:1" )]
	[InlineData( @"(?U)(?!\BB){0,3}?([\x09-\x0c\x{100}])A", "420B4142", true, "1:2,1:1" )]
	[InlineData( @"(?U)(?!\BB){0,3}?([\x09-\x0c\x{100}])A", "42420B4142", true, "2:2,2:1" )]
	[InlineData( @"(?U)(?!\BB){0,3}([\x09-\x0c\x{100}])A", "0D0A41", true, "1:2,1:1" )]
	[InlineData( @"(?U)(?!\BB){0,3}([\x09-\x0c\x{100}])A", "0B41", true, "0:2,0:1" )]
	[InlineData( @"(?U)(?!\BB){0,3}([\x09-\x0c\x{100}])A", "420B4142", true, "1:2,1:1" )]
	[InlineData( @"(?U)(?!\BB){0,3}([\x09-\x0c\x{100}])A", "42420B4142", true, "2:2,2:1" )]
	[InlineData( @"(?=(B|)){0,3}([\x09-\x0c\x{100}])A\1?", "0B41", false, "0:2,0:0,0:1" )]
	[InlineData( @"(?=(B|)){0,3}([\x09-\x0c\x{100}])A\1?", "420B4142", false, "1:2,1:0,1:1" )]
	[InlineData( @"(?=(B|)){0,3}([\x09-\x0c\x{100}])A\1?", "42420B4142", false, "2:2,2:0,2:1" )]
	[InlineData( @"(?=(\1B|B)){0,3}([\x09-\x0c\x{100}])A", "0B41", false, "0:2,-,0:1" )]
	[InlineData( @"(?=(\1B|B)){0,3}([\x09-\x0c\x{100}])A", "420B4142", false, "1:2,-,1:1" )]
	[InlineData( @"(?=(\1B|B)){0,3}([\x09-\x0c\x{100}])A", "42420B4142", false, "2:2,-,2:1" )]
	[InlineData( @"(?=(B|)){0,3}([\x09-\x0c\x{100}])A\1?", "0B41", true, "0:2,0:0,0:1" )]
	[InlineData( @"(?=(B|)){0,3}([\x09-\x0c\x{100}])A\1?", "420B4142", true, "1:2,1:0,1:1" )]
	[InlineData( @"(?=(B|)){0,3}([\x09-\x0c\x{100}])A\1?", "42420B4142", true, "2:2,2:0,2:1" )]
	[InlineData( @"(?=(\1B|B)){0,3}([\x09-\x0c\x{100}])A", "0B41", true, "0:2,-,0:1" )]
	[InlineData( @"(?=(\1B|B)){0,3}([\x09-\x0c\x{100}])A", "420B4142", true, "1:2,-,1:1" )]
	[InlineData( @"(?=(\1B|B)){0,3}([\x09-\x0c\x{100}])A", "42420B4142", true, "2:2,-,2:1" )]
	public void NegativeExitChoicesPreservePinnedNativeCaptures( string body, string hex, bool ucp, string spans ) {
		var pattern = String.Concat( "(*ANYCRLF)", body );
		var subject = Convert.FromHexString( hex );
		foreach ( var tolerant in new[] { false, true } ) {
			var reference = Reference( pattern, ucp, tolerant );
			var expected = reference.Match( subject );
			Assert.Equal( spans.Length > 0, expected.Success );
			if ( expected.Success ) {
				var groups = spans.Split( ',' );
				Assert.Equal( groups.Length, expected.Groups.Count );
				for ( var group = 0; group < groups.Length; group++ ) {
					var capture = expected.Groups[group];
					Assert.Equal( groups[group], capture.Success ? $"{capture.Index}:{capture.Length}" : "-" );
				}
			}
			Compare( reference, subject, 0, Compile( pattern, new() { Utf8 = true, Ucp = ucp,
				MatchInvalidUtf = tolerant } ).Match( subject ), pattern );
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void EquivalentNegativeExitsKeepDenseMissWorkWithinScaledDefaultBudget( bool ucp ) {
		const string pattern = @"(*ANYCRLF)(?!\BB){0,3}([\x09-\x0c\x{100}])A";
		var subject = Encoding.ASCII.GetBytes( String.Concat( Enumerable.Repeat( "\r\n", 2_000 ) ) );
		var work = 0;
		var contiguous = true;
		var result = Compile( pattern, new() { Utf8 = true, Ucp = ucp,
			WorkObserver = next => { contiguous &= next == work + 1; work = next; } } ).Match( subject );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Null( result.Match );
		Compare( Reference( pattern, ucp ), subject, 0, result, pattern );
		Assert.True( contiguous );
		// One fifth of the original 10,000-pair/default-million-work fixture.
		Assert.InRange( work, 1, 200_000 );
	}

	[Theory]
	[InlineData( 3 )]
	[InlineData( 33 )]
	[InlineData( 199 )]
	[InlineData( 499 )]
	[InlineData( 500 )]
	public void GreedyNegativeCopiesStillEvaluateAndExhaustWork( int count ) {
		var pattern = String.Concat( @"(*ANYCRLF)(?!\BB){0,", count, @"}([\x09-\x0c\x{100}])A" );
		var subject = Convert.FromHexString( "0B41" );
		var work = 0;
		var result = Compile( pattern, new() { Utf8 = true, WorkObserver = next => { work = next; } } ).Match( subject );
		Compare( Reference( pattern, compiled: false ), subject, 0, result, pattern );
		Assert.True( work > count ); // Actual assertion copies must not be erased.
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( pattern, new() { Utf8 = true, MatchWorkLimit = 5 } ).Match( subject ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( pattern, new() { Utf8 = true,
			WorkObserver = next => { if ( next == 5 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( subject, cancellationToken: cancellation.Token ) );
	}

	[Fact]
	public void GreedyNegativeCopiesRetainExpansionRejection() {
		const string pattern = @"(?!\BB){0,65535}([\x09-\x0c\x{100}])A";
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded,
			PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), new() { Utf8 = true } ).Diagnostic?.Code );
	}

	private static void Compare( PcreRegex8Bit reference, byte[] subject, int offset, PcreByteMatchResult actual, string context ) {
		var expected = reference.Match( subject, offset );
		Assert.True( actual.IsSuccess, $"{context}: {actual.Diagnostic?.Message}" );
		Assert.True( ( expected.Success ? expected.Index : -1 ) == ( actual.Match?.ByteIndex ?? -1 ),
			$"{context}: native={( expected.Success ? expected.Index : -1 )}, managed={actual.Match?.ByteIndex ?? -1}" );
		if ( !expected.Success ) { return; }
		Assert.Equal( expected.Length, actual.Match!.ByteLength );
		Assert.Equal( expected.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < expected.Groups.Count; group++ ) {
			var capture = expected.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null, actual.Match.Captures[group] );
		}
	}

	private static PcreRegex8Bit Reference( string pattern, bool ucp = false, bool tolerant = false, bool compiled = true ) => new(
		Encoding.ASCII.GetBytes( pattern ), Encoding.UTF8, new PcreRegexSettings {
			Options = ( compiled ? PcreOptions.Compiled : PcreOptions.None ) | PcreOptions.Utf | ( ucp ? PcreOptions.Ucp : PcreOptions.None )
				| ( tolerant ? PcreOptions.MatchInvalidUtf : PcreOptions.None ), ParensLimit = 512 } );

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions options ) {
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
