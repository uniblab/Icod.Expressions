using System.Buffers;
using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed partial class PcreNoAutoPossessPrefixReferenceTests {
	private const string Directive = "(*NO_AUTO_POSSESS)";
	[Theory]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329", 16, true, -1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329", 17, true, -1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329", 21, true, -1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329", 24, true, -1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941", 16, true, -1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941", 17, true, -1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941", 21, true, -1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941", 24, true, -1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A55544629282A4E4F5F4155544F5F504F535345535329C3A9", 16, true, -1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A55544629282A4E4F5F4155544F5F504F535345535329C3A9", 17, true, -1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A55544629282A4E4F5F4155544F5F504F535345535329C3A9", 21, true, -1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A55544629282A4E4F5F4155544F5F504F535345535329C3A9", 24, true, -1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F5353455353290041", 16, true, -1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F5353455353290041", 17, true, -1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F5353455353290041", 21, true, -1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F5353455353290041", 24, true, -1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532928", 0, false, -1L, 114, 19, "InvalidSyntax", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532928", 0, true, -1L, 114, 19, "InvalidSyntax", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532928", 1, false, -1L, 114, 19, "InvalidSyntax", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532928", 1, true, -1L, 114, 19, "InvalidSyntax", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532928", 5, false, -1L, 114, 19, "InvalidSyntax", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532928", 5, true, -1L, 114, 19, "InvalidSyntax", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F5353455353295B", 0, false, -1L, 106, 19, "InvalidSyntax", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F5353455353295B", 0, true, -1L, 106, 19, "InvalidSyntax", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F5353455353295B", 1, false, -1L, 106, 19, "InvalidSyntax", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F5353455353295B", 1, true, -1L, 106, 19, "InvalidSyntax", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F5353455353295B", 5, false, -1L, 106, 19, "InvalidSyntax", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F5353455353295B", 5, true, -1L, 106, 19, "InvalidSyntax", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F5353455353295C", 0, false, -1L, 101, 19, "InvalidSyntax", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F5353455353295C", 0, true, -1L, 101, 19, "InvalidSyntax", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F5353455353295C", 1, false, -1L, 101, 19, "InvalidSyntax", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F5353455353295C", 1, true, -1L, 101, 19, "InvalidSyntax", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F5353455353295C", 5, false, -1L, 101, 19, "InvalidSyntax", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F5353455353295C", 5, true, -1L, 101, 19, "InvalidSyntax", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329417B322C317D", 0, false, -1L, 104, 23, "InvalidSyntax", 19 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329417B322C317D", 0, true, -1L, 104, 23, "InvalidSyntax", 19 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329417B322C317D", 1, false, -1L, 104, 23, "InvalidSyntax", 19 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329417B322C317D", 1, true, -1L, 104, 23, "InvalidSyntax", 19 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329417B322C317D", 5, false, -1L, 104, 23, "InvalidSyntax", 19 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329417B322C317D", 5, true, -1L, 104, 23, "InvalidSyntax", 19 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329282A4E4F4155544F504F535345535329", 0, false, -1L, 160, 33, "UnsupportedSyntax", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329282A4E4F4155544F504F535345535329", 0, true, -1L, 160, 33, "UnsupportedSyntax", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329282A4E4F4155544F504F535345535329", 1, false, -1L, 160, 33, "UnsupportedSyntax", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329282A4E4F4155544F504F535345535329", 1, true, -1L, 160, 33, "UnsupportedSyntax", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329282A4E4F4155544F504F535345535329", 5, false, -1L, 160, 33, "UnsupportedSyntax", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329282A4E4F4155544F504F535345535329", 5, true, -1L, 160, 33, "UnsupportedSyntax", 18 )]
	[InlineData( "41282A4E4F5F4155544F5F504F535345535329", 0, false, -1L, 160, 18, "UnsupportedSyntax", 1 )]
	[InlineData( "41282A4E4F5F4155544F5F504F535345535329", 0, true, -1L, 160, 18, "UnsupportedSyntax", 1 )]
	[InlineData( "41282A4E4F5F4155544F5F504F535345535329", 1, false, -1L, 160, 18, "UnsupportedSyntax", 1 )]
	[InlineData( "41282A4E4F5F4155544F5F504F535345535329", 1, true, -1L, 160, 18, "UnsupportedSyntax", 1 )]
	[InlineData( "283F6929282A4E4F5F4155544F5F504F535345535329", 0, false, -1L, 160, 21, "UnsupportedSyntax", 4 )]
	[InlineData( "283F6929282A4E4F5F4155544F5F504F535345535329", 0, true, -1L, 160, 21, "UnsupportedSyntax", 4 )]
	[InlineData( "283F6929282A4E4F5F4155544F5F504F535345535329", 1, false, -1L, 160, 21, "UnsupportedSyntax", 4 )]
	[InlineData( "283F6929282A4E4F5F4155544F5F504F535345535329", 1, true, -1L, 160, 21, "UnsupportedSyntax", 4 )]
	[InlineData( "20282A4E4F5F4155544F5F504F535345535329", 0, false, -1L, 160, 18, "UnsupportedSyntax", 1 )]
	[InlineData( "20282A4E4F5F4155544F5F504F535345535329", 0, true, -1L, 160, 18, "UnsupportedSyntax", 1 )]
	[InlineData( "20282A4E4F5F4155544F5F504F535345535329", 1, false, -1L, 160, 18, "UnsupportedSyntax", 1 )]
	[InlineData( "20282A4E4F5F4155544F5F504F535345535329", 1, true, -1L, 160, 18, "UnsupportedSyntax", 1 )]
	[InlineData( "283F3A282A4E4F5F4155544F5F504F53534553532929", 0, false, -1L, 160, 20, "UnsupportedSyntax", 3 )]
	[InlineData( "283F3A282A4E4F5F4155544F5F504F53534553532929", 0, true, -1L, 160, 20, "UnsupportedSyntax", 3 )]
	[InlineData( "283F3A282A4E4F5F4155544F5F504F53534553532929", 1, false, -1L, 160, 20, "UnsupportedSyntax", 3 )]
	[InlineData( "283F3A282A4E4F5F4155544F5F504F53534553532929", 1, true, -1L, 160, 20, "UnsupportedSyntax", 3 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941282A4E4F5F4155544F5F504F535345535329", 0, false, -1L, 160, 36, "UnsupportedSyntax", 19 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941282A4E4F5F4155544F5F504F535345535329", 0, true, -1L, 160, 36, "UnsupportedSyntax", 19 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941282A4E4F5F4155544F5F504F535345535329", 1, false, -1L, 160, 36, "UnsupportedSyntax", 19 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941282A4E4F5F4155544F5F504F535345535329", 1, true, -1L, 160, 36, "UnsupportedSyntax", 19 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329283F6929282A4E4F5F4155544F5F504F535345535329", 0, false, -1L, 160, 39, "UnsupportedSyntax", 22 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329283F6929282A4E4F5F4155544F5F504F535345535329", 0, true, -1L, 160, 39, "UnsupportedSyntax", 22 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329283F6929282A4E4F5F4155544F5F504F535345535329", 1, false, -1L, 160, 39, "UnsupportedSyntax", 22 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329283F6929282A4E4F5F4155544F5F504F535345535329", 1, true, -1L, 160, 39, "UnsupportedSyntax", 22 )]
	[InlineData( "282A4D41524B3A4E29282A4E4F5F4155544F5F504F535345535329", 0, false, -1L, 160, 26, "UnsupportedSyntax", 9 )]
	[InlineData( "282A4D41524B3A4E29282A4E4F5F4155544F5F504F535345535329", 0, true, -1L, 160, 26, "UnsupportedSyntax", 9 )]
	[InlineData( "282A4D41524B3A4E29282A4E4F5F4155544F5F504F535345535329", 1, false, -1L, 160, 26, "UnsupportedSyntax", 9 )]
	[InlineData( "282A4D41524B3A4E29282A4E4F5F4155544F5F504F535345535329", 1, true, -1L, 160, 26, "UnsupportedSyntax", 9 )]
	[InlineData( "282A4E4F5F4155544F5F504F5353455353", 0, false, -1L, 160, 17, "InvalidSyntax", 1 )]
	[InlineData( "282A4E4F5F4155544F5F504F5353455353", 0, true, -1L, 160, 17, "InvalidSyntax", 1 )]
	[InlineData( "282A4E4F5F4155544F5F504F5353455353", 1, false, -1L, 160, 17, "InvalidSyntax", 1 )]
	[InlineData( "282A4E4F5F4155544F5F504F5353455353", 1, true, -1L, 160, 17, "InvalidSyntax", 1 )]
	[InlineData( "282A6E6F5F6175746F5F706F737365737329", 0, false, -1L, 195, 18, "UnsupportedSyntax", 0 )]
	[InlineData( "282A6E6F5F6175746F5F706F737365737329", 0, true, -1L, 195, 18, "UnsupportedSyntax", 0 )]
	[InlineData( "282A6E6F5F6175746F5F706F737365737329", 1, false, -1L, 195, 18, "UnsupportedSyntax", 0 )]
	[InlineData( "282A6E6F5F6175746F5F706F737365737329", 1, true, -1L, 195, 18, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553533A5829", 0, false, -1L, 160, 17, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553533A5829", 0, true, -1L, 160, 17, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553533A5829", 1, false, -1L, 160, 17, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553533A5829", 1, true, -1L, 160, 17, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4E4F4155544F504F535345535329", 0, false, -1L, 160, 15, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4E4F4155544F504F535345535329", 0, true, -1L, 160, 15, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4E4F4155544F504F535345535329", 1, false, -1L, 160, 15, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4E4F4155544F504F535345535329", 1, true, -1L, 160, 15, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329FF", 1, false, -1L, -23, 18, "InvalidUtf8Pattern", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329FF", 1, true, -1L, -23, 18, "InvalidUtf8Pattern", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329FF", 5, false, -1L, -23, 18, "InvalidUtf8Pattern", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329FF", 5, true, -1L, -23, 18, "InvalidUtf8Pattern", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329FF", 17, false, -1L, -23, 18, "InvalidUtf8Pattern", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329FF", 17, true, -1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941FF", 1, false, -1L, -23, 19, "InvalidUtf8Pattern", 19 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941FF", 1, true, -1L, -23, 19, "InvalidUtf8Pattern", 19 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941FF", 5, false, -1L, -23, 19, "InvalidUtf8Pattern", 19 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941FF", 5, true, -1L, -23, 19, "InvalidUtf8Pattern", 19 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941FF", 17, false, -1L, -23, 19, "InvalidUtf8Pattern", 19 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941FF", 17, true, -1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329EDA080", 1, false, -1L, -16, 18, "InvalidUtf8Pattern", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329EDA080", 1, true, -1L, -16, 18, "InvalidUtf8Pattern", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329EDA080", 5, false, -1L, -16, 18, "InvalidUtf8Pattern", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329EDA080", 5, true, -1L, -16, 18, "InvalidUtf8Pattern", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329EDA080", 17, false, -1L, -16, 18, "InvalidUtf8Pattern", 18 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329EDA080", 17, true, -1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329282A55544629FF", 0, false, -1L, -23, 24, "InvalidUtf8Pattern", 24 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329282A55544629FF", 0, true, -1L, -23, 24, "InvalidUtf8Pattern", 24 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329282A5554462941FF", 0, false, -1L, -23, 25, "InvalidUtf8Pattern", 25 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329282A5554462941FF", 0, true, -1L, -23, 25, "InvalidUtf8Pattern", 25 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329282A5554462941", 64, false, -1L, 174, 24, "InvalidSyntax", 24 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329282A5554462941", 64, true, -1L, 174, 24, "InvalidSyntax", 24 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329282A5554462941", 65, false, -1L, 174, 24, "InvalidSyntax", 24 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329282A5554462941", 65, true, -1L, 174, 24, "InvalidSyntax", 24 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329282A5543502941", 128, false, -1L, 175, 24, "InvalidSyntax", 24 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329282A5543502941", 128, true, -1L, 175, 24, "InvalidSyntax", 24 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329282A5543502941", 16512, false, -1L, 175, 24, "InvalidSyntax", 24 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329282A5543502941", 16512, true, -1L, 175, 24, "InvalidSyntax", 24 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329", 0, false, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329", 0, true, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329", 1, false, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329", 1, true, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329", 16, false, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329", 16, true, 1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329", 17, false, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329", 17, true, 1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329", 0, false, 17L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329", 0, true, 17L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329", 1, false, 17L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329", 1, true, 17L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329", 16, false, 17L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329", 16, true, 17L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329", 17, false, 17L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329", 17, true, 17L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329", 16, true, 18L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329", 17, true, 18L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329", 16, true, 19L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329", 17, true, 19L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329", 16, true, 4294967295L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F535345535329", 17, true, 4294967295L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941", 0, false, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941", 0, true, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941", 1, false, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941", 1, true, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941", 16, false, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941", 16, true, 1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941", 17, false, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941", 17, true, 1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941", 0, false, 18L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941", 0, true, 18L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941", 1, false, 18L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941", 1, true, 18L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941", 16, false, 18L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941", 16, true, 18L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941", 17, false, 18L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941", 17, true, 18L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941", 16, true, 19L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941", 17, true, 19L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941", 16, true, 20L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941", 17, true, 20L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941", 16, true, 4294967295L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F4155544F5F504F53534553532941", 17, true, 4294967295L, 192, 0, "InvalidSyntax", 0 )]
	public void PrefixPreservesDiagnosticPriority( string hex, int mode, bool enabled, long maximum, int nativeCode, int nativeOffset, string code, int offset ) {
		var source = Convert.FromHexString( hex );
		var native = Assert.Throws<PcrePatternException>( () => Reference( source, mode, enabled, 0, maximum ) );
		Assert.Equal( nativeCode, (int)native.ErrorCode );
		Assert.EndsWith( $" at offset {nativeOffset}.", native.Message );
		var actual = PcreByteCompiler.Compile( source, Options( mode, enabled, 0, 0, maximum ) );
		Assert.Null( actual.Pattern );
		Assert.Equal( code, actual.Diagnostic?.Code.ToString() );
		Assert.Equal( offset, actual.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( 0, false )]
	[InlineData( 0, true )]
	[InlineData( 1, false )]
	[InlineData( 1, true )]
	public void CallerBudgetsBeforePrefixIntegration( int mode, bool enabled ) {
		var source = "A"u8.ToArray();
		var native = Reference( source, mode, enabled );
		AssertNative( native, source, 0, 0, "0:1", "" );
		var steps = new List<int>();
		var options = Options( mode, enabled ) with { WorkObserver = steps.Add };
		Compare( native, source, 0, 0, Compile( source, options ).Match( source ), "caller work" );
		AssertBudget( source, source, options, native, 0, steps );
	}

	[Theory]
	[InlineData( 0, false, 1 )]
	[InlineData( 0, true, 1 )]
	[InlineData( 1, false, 1 )]
	[InlineData( 1, true, 1 )]
	[InlineData( 0, false, 256 )]
	[InlineData( 0, true, 256 )]
	[InlineData( 1, false, 256 )]
	[InlineData( 1, true, 256 )]
	public void PrefixChainPreservesWorkBudgets( int mode, bool enabled, int count ) {
		var source = Encoding.UTF8.GetBytes( String.Concat( String.Concat( Enumerable.Repeat( Directive, count ) ), "A" ) );
		var subject = "A"u8.ToArray();
		var native = Reference( source, mode, enabled );
		AssertNative( native, subject, 0, 0, "0:1", "" );
		var steps = new List<int>();
		var plain = new List<int>();
		var options = Options( mode, enabled ) with { WorkObserver = steps.Add };
		Compare( native, subject, 0, 0, Compile( source, options ).Match( subject ), "prefix work" );
		Compare( native, subject, 0, 0, Compile( subject, Options( mode, true ) with { WorkObserver = plain.Add } ).Match( subject ), "body work" );
		Assert.Equal( plain, steps );
		AssertBudget( source, subject, options, native, 0, steps );
	}

	[Theory]
	[InlineData( 1024, false )]
	[InlineData( 1024, true )]
	[InlineData( 1025, false )]
	[InlineData( 1025, true )]
	[InlineData( 1029, false )]
	[InlineData( 1029, true )]
	public void PrefixRetryBudgetsAndCancellation( int mode, bool enabled ) {
		var source = Encoding.UTF8.GetBytes( String.Concat( Directive, @"(?=(A\K))A?" ) );
		var subject = "A"u8.ToArray();
		var native = Reference( source, mode, enabled );
		AssertNative( native, subject, 0, 1, "1:-1,0:1", "" );
		var steps = new List<int>();
		var options = Options( mode, enabled, 1 ) with { WorkObserver = steps.Add };
		Compare( native, subject, 0, 1, Compile( source, options ).Match( subject ), "prefix retry" );
		AssertBudget( source, subject, options, native, 1, steps );
	}

	private static void AssertBudget( byte[] source, byte[] subject, PcreByteOptions options, PcreRegex8Bit native, int empty, List<int> steps ) {
		Assert.Equal( Enumerable.Range( 1, steps.Count ), steps );
		var workCount = steps.Count;
		Compare( native, subject, 0, empty, Compile( source, options with { MatchWorkLimit = workCount } ).Match( subject ), "exact work" );
		var exhausted = Compile( source, options with { MatchWorkLimit = workCount - 1 } ).Match( subject );
		Assert.Null( exhausted.Match );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, exhausted.Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( source, options with { WorkObserver = count => { if ( count == 2 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
	}

	[Theory]
	[InlineData( 1024, false )]
	[InlineData( 1024, true )]
	[InlineData( 1025, false )]
	[InlineData( 1025, true )]
	[InlineData( 1029, false )]
	[InlineData( 1029, true )]
	public void CompiledPatternOwnsPrefixViewAndMark( int mode, bool enabled ) {
		var pattern = Encoding.UTF8.GetBytes( String.Concat( Directive, @"(*MARK:N)(?=(A\K))A?" ) );
		var source = new byte[pattern.Length + 2];
		Array.Fill( source, (byte)0xFF );
		pattern.CopyTo( source, 1 );
		var subject = "A"u8.ToArray();
		var native = Reference( pattern, mode, enabled );
		AssertNative( native, subject, 0, 1, "1:-1,0:1", "4E" );
		var options = Options( mode, enabled, 1 );
		var actual = PcreByteCompiler.Compile( source.AsMemory( 1, pattern.Length ), options );
		Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
		var compiled = Assert.IsType<PcreCompiledBytePattern>( actual.Pattern );
		Array.Fill( source, (byte)'x' );
		typeof( PcreByteOptions ).GetProperty( "NoAutoPossess" )!.SetValue( options, false );
		Compare( native, subject, 0, 1, compiled.Match( subject ), "owned prefix view/mark" );
	}

	[Theory]
	[InlineData( 0, false )]
	[InlineData( 0, true )]
	[InlineData( 1, false )]
	[InlineData( 1, true )]
	[InlineData( 2, false )]
	[InlineData( 2, true )]
	public void PrefixKeepsFullCapacities( int selection, bool enabled ) {
		var body = selection switch {
			0 => new string( 'A', 4095 ),
			1 => String.Concat( String.Concat( Enumerable.Repeat( "()", 256 ) ), "A" ),
			_ => String.Concat( String.Concat( Enumerable.Repeat( "(?:", 256 ) ), "A", new string( ')', 256 ) )
		};
		var source = Encoding.ASCII.GetBytes( String.Concat( Directive, body ) );
		var subject = selection == 0 ? Encoding.ASCII.GetBytes( body ) : "A"u8.ToArray();
		var spans = selection == 0 ? "0:4095" : selection == 1 ? String.Concat( "0:1,", String.Join( ",", Enumerable.Repeat( "0:0", 256 ) ) ) : "0:1";
		var native = Reference( source, 0, enabled );
		AssertNative( native, subject, 0, 0, spans, "" );
		Compare( native, subject, 0, 0, Compile( source, Options( 0, enabled ) ).Match( subject ), "prefix capacities" );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void PrefixDoesNotRelaxInstructionLimit( bool enabled ) {
		var source = Encoding.ASCII.GetBytes( String.Concat( Directive, new string( 'A', 4096 ) ) );
		var subject = Encoding.ASCII.GetBytes( new string( 'A', 4096 ) );
		AssertNative( Reference( source, 0, enabled ), subject, 0, 0, "0:4096", "" );
		var actual = PcreByteCompiler.Compile( source, Options( 0, enabled ) );
		Assert.Null( actual.Pattern );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, actual.Diagnostic?.Code );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void CallerValidationPriorityBeforePrefixIntegration( bool enabled ) {
		var source = "A"u8.ToArray();
		var options = Options( 1, enabled );
		var native = Reference( source, 1, enabled );
		AssertNative( native, source, 0, 0, "0:1", "" );
		var compiled = Compile( source, options );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Subject, compiled.Match( new byte[] { 0xFF } ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset, compiled.Match( "é"u8.ToArray(), 1 ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset, compiled.Match( new byte[] { 0xFF }, -1 ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () => compiled.Match( new byte[] { 0xFF }, -1, cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( source, options, cancellation.Token ) );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void RejectedPrefixMemoryIsNeverRead( bool enabled ) {
		var source = Encoding.ASCII.GetBytes( String.Concat( Directive, "A" ) );
		var length = Assert.Throws<PcrePatternException>( () => Reference( source, 0, enabled, 0, 1 ) );
		Assert.Equal( 188, (int)length.ErrorCode );
		Assert.EndsWith( " at offset 0.", length.Message );
		var conflict = Assert.Throws<PcrePatternException>( () => Reference( source, 48, enabled, 0, 1 ) );
		Assert.Equal( 192, (int)conflict.ErrorCode );
		Assert.EndsWith( " at offset 0.", conflict.Message );
		using var memory = new UnreadablePatternMemory();
		var options = Options( 0, enabled, 0, 0, 1 );
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( memory.Pattern, options, cancellation.Token ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidWorkLimit, PcreByteCompiler.Compile( memory.Pattern, options with { MatchWorkLimit = 0 } ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, PcreByteCompiler.Compile( memory.Pattern, options with { Literal = true, Multiline = true } ).Diagnostic?.Code );
		var actual = PcreByteCompiler.Compile( memory.Pattern, options );
		Assert.Null( actual.Pattern );
		Assert.Equal( PcreByteDiagnosticCode.PatternLengthLimitExceeded, actual.Diagnostic?.Code );
		Assert.False( memory.ReadRequested );
	}
	private sealed class UnreadablePatternMemory : MemoryManager<byte> {
		internal bool ReadRequested { get; private set; }
		internal ReadOnlyMemory<byte> Pattern => CreateMemory( Directive.Length + 1 );
		public override Span<byte> GetSpan() {
			ReadRequested = true;
			throw new InvalidOperationException( "rejected prefix memory must not be read" );
		}
		public override MemoryHandle Pin( int elementIndex = 0 ) => throw new NotSupportedException();
		public override void Unpin() { }
		protected override void Dispose( bool disposing ) { }
	}

	private static void AssertProtocols( string hex, int mode, bool enabled, int empty, int policy, long maximum, string protocols ) {
		var source = Convert.FromHexString( hex );
		var cases = protocols.Split( ';' ).Select( Protocol.Parse ).ToArray();
		var native = Reference( source, mode, enabled, policy, maximum );
		foreach ( var item in cases ) { AssertNative( native, item.Subject, item.Offset, empty, item.Spans, item.Mark ); }
		var compiled = Compile( source, Options( mode, enabled, empty, policy, maximum ) );
		foreach ( var item in cases ) { Compare( native, item.Subject, item.Offset, empty, compiled.Match( item.Subject, item.Offset ), $"{hex}/{mode}/{enabled}/{empty}/{policy}/{maximum}/{Convert.ToHexString( item.Subject )}/{item.Offset}" ); }
	}
	private readonly record struct Protocol( byte[] Subject, int Offset, string Spans, string Mark ) {
		internal static Protocol Parse( string value ) {
			var parts = value.Split( '/' );
			return new( Convert.FromHexString( parts[0] ), Int32.Parse( parts[1] ), parts[2], parts[3] );
		}
	}
	private static PcreByteOptions Options( int mode, bool enabled, int empty = 0, int policy = 0, long maximum = -1 ) => new() {
		Utf8 = ( mode & 1 ) != 0, MatchInvalidUtf = ( mode & 4 ) != 0, Caseless = ( mode & 8 ) != 0,
		Literal = ( mode & 16 ) != 0, Multiline = ( mode & 32 ) != 0, NeverUtf = ( mode & 64 ) != 0,
		NeverUcp = ( mode & 128 ) != 0, NeverBackslashC = ( mode & 256 ) != 0, NeverCallout = ( mode & 512 ) != 0,
		AllowLookaroundBackslashK = ( mode & 1024 ) != 0, Extended = ( mode & 2048 ) != 0,
		MatchWord = ( mode & 4096 ) != 0, MatchLine = ( mode & 8192 ) != 0, Ucp = ( mode & 16384 ) != 0,
		NotEmpty = ( empty & 1 ) != 0, NotEmptyAtStart = ( empty & 2 ) != 0,
		Anchored = ( policy & 1 ) != 0, EndAnchored = ( policy & 2 ) != 0, FirstLine = ( policy & 4 ) != 0,
		NoAutoPossess = enabled, MaxPatternLength = maximum < 0 ? null : (uint)maximum
	};
	private static PcreRegex8Bit Reference( byte[] source, int mode, bool enabled, int policy = 0, long maximum = -1 ) => new( source, Encoding.UTF8, new PcreRegexSettings {
		Options = PcreOptions.Compiled | (PcreOptions)( ( enabled ? 0x4000u : 0u ) | ( ( mode & 1 ) != 0 ? 0x80000u : 0u )
			| ( ( mode & 4 ) != 0 ? 0x4000000u : 0u ) | ( ( mode & 8 ) != 0 ? 8u : 0u ) | ( ( mode & 16 ) != 0 ? 0x2000000u : 0u )
			| ( ( mode & 32 ) != 0 ? 0x400u : 0u ) | ( ( mode & 64 ) != 0 ? 0x1000u : 0u ) | ( ( mode & 128 ) != 0 ? 0x800u : 0u )
			| ( ( mode & 256 ) != 0 ? 0x100000u : 0u ) | ( ( mode & 2048 ) != 0 ? 0x80u : 0u ) | ( ( mode & 16384 ) != 0 ? 0x20000u : 0u )
			| ( ( policy & 1 ) != 0 ? 0x80000000u : 0u ) | ( ( policy & 2 ) != 0 ? 0x20000000u : 0u ) | ( ( policy & 4 ) != 0 ? 0x100u : 0u ) ),
		ExtraCompileOptions = (PcreExtraCompileOptions)( ( ( mode & 1024 ) != 0 ? 64 : 0 ) | ( ( mode & 512 ) != 0 ? 0x8000 : 0 ) | ( ( mode & 4096 ) != 0 ? 4 : 0 ) | ( ( mode & 8192 ) != 0 ? 8 : 0 ) ),
		MaxPatternLength = maximum < 0 ? null : (uint)maximum, ParensLimit = 512, NewLine = (PcreNewLine)2, BackslashR = (PcreBackslashR)1
	} );
	private static PcreCompiledBytePattern Compile( byte[] source, PcreByteOptions options ) {
		var actual = PcreByteCompiler.Compile( source, options );
		Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( actual.Pattern );
	}
	private static PcreMatchOptions MatchOptions( int empty ) => (PcreMatchOptions)( empty * 4 );
	private static void AssertNative( PcreRegex8Bit native, byte[] subject, int offset, int empty, string spans, string mark ) {
		var match = native.Match( subject, offset, MatchOptions( empty ) );
		Assert.Equal( spans.Length > 0, match.Success );
		if ( !match.Success ) { return; }
		Assert.Equal( mark, Convert.ToHexString( match.Mark.ToArray() ) );
		var groups = spans.Split( ',' );
		Assert.Equal( groups.Length, match.Groups.Count );
		for ( var group = 0; group < groups.Length; group++ ) {
			var capture = match.Groups[group];
			Assert.Equal( groups[group], capture.Success ? $"{capture.Index}:{capture.EndIndex - capture.Index}" : "-" );
		}
	}
	private static void Compare( PcreRegex8Bit native, byte[] subject, int offset, int empty, PcreByteMatchResult actual, string detail ) {
		var expected = native.Match( subject, offset, MatchOptions( empty ) );
		Assert.True( actual.IsSuccess, $"{detail}: {actual.Diagnostic?.Message}" );
		Assert.True( ( expected.Success ? expected.Index : -1 ) == ( actual.Match?.ByteIndex ?? -1 ), $"{detail}: expected index {( expected.Success ? expected.Index : -1 )}, actual {actual.Match?.ByteIndex ?? -1}" );
		if ( !expected.Success ) { return; }
		Assert.Equal( expected.EndIndex - expected.Index, actual.Match!.ByteLength );
		Assert.Equal( expected.Mark.ToArray(), actual.Match.MarkName.ToArray() );
		Assert.Equal( expected.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < expected.Groups.Count; group++ ) {
			var capture = expected.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.EndIndex - capture.Index ) : null, actual.Match.Captures[group] );
		}
	}
}

