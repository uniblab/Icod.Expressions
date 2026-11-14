using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed partial class PcreVariableLookbehindLimitReferenceTests {
	[Theory]
	[InlineData( "283F3C3D413F2942", 0, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D413F2942", 1, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D413F2942", 5, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C21413F2942", 0, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C21413F2942", 1, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C21413F2942", 5, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D2942", 0, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D2942", 1, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D2942", 5, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D2942", 0, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D2942", 1, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D2942", 5, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C21417B312C327D2942", 0, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C21417B312C327D2942", 1, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C21417B312C327D2942", 5, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C21417B312C327D2942", 0, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C21417B312C327D2942", 1, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C21417B312C327D2942", 5, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D28417B312C337D292942", 0, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D28417B312C337D292942", 1, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D28417B312C337D292942", 5, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D28417B312C337D292942", 0, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D28417B312C337D292942", 1, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D28417B312C337D292942", 5, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D28417B312C337D292942", 0, 2u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D28417B312C337D292942", 1, 2u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D28417B312C337D292942", 5, 2u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C2128417B312C337D292942", 0, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C2128417B312C337D292942", 1, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C2128417B312C337D292942", 5, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C2128417B312C337D292942", 0, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C2128417B312C337D292942", 1, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C2128417B312C337D292942", 5, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C2128417B312C337D292942", 0, 2u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C2128417B312C337D292942", 1, 2u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C2128417B312C337D292942", 5, 2u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D7C4141412942", 0, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D7C4141412942", 1, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D7C4141412942", 5, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D7C4141412942", 0, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D7C4141412942", 1, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D7C4141412942", 5, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D7C4141412942", 0, 2u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D7C4141412942", 1, 2u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D7C4141412942", 5, 2u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D7C414141412942", 0, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D7C414141412942", 1, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D7C414141412942", 5, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D7C414141412942", 0, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D7C414141412942", 1, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D7C414141412942", 5, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D7C414141412942", 0, 2u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D7C414141412942", 1, 2u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D7C414141412942", 5, 2u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D7C414141412942", 0, 3u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D7C414141412942", 1, 3u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D7C414141412942", 5, 3u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B3235342C3235357D2942", 0, 254u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B3235342C3235357D2942", 1, 254u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B3235342C3235357D2942", 5, 254u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B3235352C3235367D2942", 0, 254u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B3235352C3235367D2942", 1, 254u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B3235352C3235367D2942", 5, 254u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B3235352C3235367D2942", 0, 255u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B3235352C3235367D2942", 1, 255u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B3235352C3235367D2942", 5, 255u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C21417B3235352C3235367D2942", 0, 254u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C21417B3235352C3235367D2942", 1, 254u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C21417B3235352C3235367D2942", 5, 254u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C21417B3235352C3235367D2942", 0, 255u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C21417B3235352C3235367D2942", 1, 255u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C21417B3235352C3235367D2942", 5, 255u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B3239392C3330307D2942", 0, 254u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B3239392C3330307D2942", 1, 254u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B3239392C3330307D2942", 5, 254u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B3239392C3330307D2942", 0, 255u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B3239392C3330307D2942", 1, 255u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B3239392C3330307D2942", 5, 255u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B3239392C3330307D2942", 0, 256u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B3239392C3330307D2942", 1, 256u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B3239392C3330307D2942", 5, 256u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B3531312C3531327D2942", 0, 254u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B3531312C3531327D2942", 1, 254u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B3531312C3531327D2942", 5, 254u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B3531312C3531327D2942", 0, 255u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B3531312C3531327D2942", 1, 255u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B3531312C3531327D2942", 5, 255u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B3531312C3531327D2942", 0, 256u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B3531312C3531327D2942", 1, 256u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B3531312C3531327D2942", 5, 256u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B3531312C3531327D2942", 0, 300u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B3531312C3531327D2942", 1, 300u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B3531312C3531327D2942", 5, 300u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D41283F3C3D413F292942", 0, 0u, 200, 5, "UnsupportedSyntax", 5 )]
	[InlineData( "283F3C3D41283F3C3D413F292942", 1, 0u, 200, 5, "UnsupportedSyntax", 5 )]
	[InlineData( "283F3C3D41283F3C3D413F292942", 5, 0u, 200, 5, "UnsupportedSyntax", 5 )]
	[InlineData( "283F3D283F3C3D417B312C327D29422942", 0, 0u, 200, 3, "UnsupportedSyntax", 3 )]
	[InlineData( "283F3D283F3C3D417B312C327D29422942", 1, 0u, 200, 3, "UnsupportedSyntax", 3 )]
	[InlineData( "283F3D283F3C3D417B312C327D29422942", 5, 0u, 200, 3, "UnsupportedSyntax", 3 )]
	[InlineData( "283F3D283F3C3D417B312C327D29422942", 0, 1u, 200, 3, "UnsupportedSyntax", 3 )]
	[InlineData( "283F3D283F3C3D417B312C327D29422942", 1, 1u, 200, 3, "UnsupportedSyntax", 3 )]
	[InlineData( "283F3D283F3C3D417B312C327D29422942", 5, 1u, 200, 3, "UnsupportedSyntax", 3 )]
	[InlineData( "283F3C3D417B312C327D283F3D42292942", 0, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D283F3D42292942", 1, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D283F3D42292942", 5, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D283F3D42292942", 0, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D283F3D42292942", 1, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D283F3D42292942", 5, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D242942", 0, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D242942", 1, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D242942", 5, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D242942", 0, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D242942", 1, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D242942", 5, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D283F3C3D41292942", 0, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D283F3C3D41292942", 1, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D283F3C3D41292942", 5, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D283F3C3D41292942", 0, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D283F3C3D41292942", 1, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D283F3C3D41292942", 5, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D283F3A417B312C327D292942", 0, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D283F3A417B312C327D292942", 1, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D283F3A417B312C327D292942", 5, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D283F3A417B312C327D292942", 0, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D283F3A417B312C327D292942", 1, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D283F3A417B312C327D292942", 5, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D28283F3A413F29292942", 0, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D28283F3A413F29292942", 1, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D28283F3A413F29292942", 5, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D41283F3C21423F292942", 0, 0u, 200, 5, "UnsupportedSyntax", 5 )]
	[InlineData( "283F3C3D41283F3C21423F292942", 1, 0u, 200, 5, "UnsupportedSyntax", 5 )]
	[InlineData( "283F3C3D41283F3C21423F292942", 5, 0u, 200, 5, "UnsupportedSyntax", 5 )]
	[InlineData( "283F3C3D283F3E417B312C327D292942", 0, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D283F3E417B312C327D292942", 1, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D283F3E417B312C327D292942", 5, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D283F3E417B312C327D292942", 0, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D283F3E417B312C327D292942", 1, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D283F3E417B312C327D292942", 5, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C2A417B312C327D2942", 0, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C2A417B312C327D2942", 1, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C2A417B312C327D2942", 5, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C2A417B312C327D2942", 0, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C2A417B312C327D2942", 1, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C2A417B312C327D2942", 5, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D283F283F3D4129417C4141292942", 0, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D283F283F3D4129417C4141292942", 1, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D283F283F3D4129417C4141292942", 5, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D283F283F3D4129417C4141292942", 0, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D283F283F3D4129417C4141292942", 1, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D283F283F3D4129417C4141292942", 5, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "28417B312C327D29283F3C3D5C312942", 0, 0u, 200, 8, "UnsupportedSyntax", 8 )]
	[InlineData( "28417B312C327D29283F3C3D5C312942", 1, 0u, 200, 8, "UnsupportedSyntax", 8 )]
	[InlineData( "28417B312C327D29283F3C3D5C312942", 5, 0u, 200, 8, "UnsupportedSyntax", 8 )]
	[InlineData( "28417B312C327D29283F3C3D5C312942", 0, 1u, 200, 8, "UnsupportedSyntax", 8 )]
	[InlineData( "28417B312C327D29283F3C3D5C312942", 1, 1u, 200, 8, "UnsupportedSyntax", 8 )]
	[InlineData( "28417B312C327D29283F3C3D5C312942", 5, 1u, 200, 8, "UnsupportedSyntax", 8 )]
	[InlineData( "28417B312C327D29283F3C3D283F31292942", 0, 0u, 200, 8, "UnsupportedSyntax", 8 )]
	[InlineData( "28417B312C327D29283F3C3D283F31292942", 1, 0u, 200, 8, "UnsupportedSyntax", 8 )]
	[InlineData( "28417B312C327D29283F3C3D283F31292942", 5, 0u, 200, 8, "UnsupportedSyntax", 8 )]
	[InlineData( "28417B312C327D29283F3C3D283F31292942", 0, 1u, 200, 8, "UnsupportedSyntax", 8 )]
	[InlineData( "28417B312C327D29283F3C3D283F31292942", 1, 1u, 200, 8, "UnsupportedSyntax", 8 )]
	[InlineData( "28417B312C327D29283F3C3D283F31292942", 5, 1u, 200, 8, "UnsupportedSyntax", 8 )]
	[InlineData( "283F28444546494E4529283F3C783E417B312C327D2929283F3C3D283F2678292942", 0, 0u, 200, 23, "UnsupportedSyntax", 23 )]
	[InlineData( "283F28444546494E4529283F3C783E417B312C327D2929283F3C3D283F2678292942", 1, 0u, 200, 23, "UnsupportedSyntax", 23 )]
	[InlineData( "283F28444546494E4529283F3C783E417B312C327D2929283F3C3D283F2678292942", 5, 0u, 200, 23, "UnsupportedSyntax", 23 )]
	[InlineData( "283F28444546494E4529283F3C783E417B312C327D2929283F3C3D283F2678292942", 0, 1u, 200, 23, "UnsupportedSyntax", 23 )]
	[InlineData( "283F28444546494E4529283F3C783E417B312C327D2929283F3C3D283F2678292942", 1, 1u, 200, 23, "UnsupportedSyntax", 23 )]
	[InlineData( "283F28444546494E4529283F3C783E417B312C327D2929283F3C3D283F2678292942", 5, 1u, 200, 23, "UnsupportedSyntax", 23 )]
	[InlineData( "283F3C3D28417B312C327D2929282A4D41524B3A4E2942", 0, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D28417B312C327D2929282A4D41524B3A4E2942", 1, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D28417B312C327D2929282A4D41524B3A4E2942", 5, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D28417B312C327D2929282A4D41524B3A4E2942", 0, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D28417B312C327D2929282A4D41524B3A4E2942", 1, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D28417B312C327D2929282A4D41524B3A4E2942", 5, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F6929283F3C3D617B312C327D2942", 0, 0u, 200, 4, "UnsupportedSyntax", 4 )]
	[InlineData( "283F6929283F3C3D617B312C327D2942", 1, 0u, 200, 4, "UnsupportedSyntax", 4 )]
	[InlineData( "283F6929283F3C3D617B312C327D2942", 5, 0u, 200, 4, "UnsupportedSyntax", 4 )]
	[InlineData( "283F6929283F3C3D617B312C327D2942", 0, 1u, 200, 4, "UnsupportedSyntax", 4 )]
	[InlineData( "283F6929283F3C3D617B312C327D2942", 1, 1u, 200, 4, "UnsupportedSyntax", 4 )]
	[InlineData( "283F6929283F3C3D617B312C327D2942", 5, 1u, 200, 4, "UnsupportedSyntax", 4 )]
	[InlineData( "283F6D29283F3C3D417B312C327D242942", 0, 0u, 200, 4, "UnsupportedSyntax", 4 )]
	[InlineData( "283F6D29283F3C3D417B312C327D242942", 1, 0u, 200, 4, "UnsupportedSyntax", 4 )]
	[InlineData( "283F6D29283F3C3D417B312C327D242942", 5, 0u, 200, 4, "UnsupportedSyntax", 4 )]
	[InlineData( "283F6D29283F3C3D417B312C327D242942", 0, 1u, 200, 4, "UnsupportedSyntax", 4 )]
	[InlineData( "283F6D29283F3C3D417B312C327D242942", 1, 1u, 200, 4, "UnsupportedSyntax", 4 )]
	[InlineData( "283F6D29283F3C3D417B312C327D242942", 5, 1u, 200, 4, "UnsupportedSyntax", 4 )]
	[InlineData( "283F3C3DC3A97B312C327D2942", 0, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3DC3A97B312C327D2942", 1, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3DC3A97B312C327D2942", 5, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3DC3A97B312C327D2942", 0, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3DC3A97B312C327D2942", 1, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3DC3A97B312C327D2942", 5, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3DC3A97B312C327D2942", 0, 2u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D5C4B2942", 0, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D5C4B2942", 1, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D5C4B2942", 5, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D5C4B2942", 0, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D5C4B2942", 1, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C327D5C4B2942", 5, 1u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D412A2942", 0, 0u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D412A2942", 1, 0u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D412A2942", 0, 1u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D412A2942", 1, 1u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D412A2942", 0, 255u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D412A2942", 1, 255u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D412A2942", 0, 4294967295u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D412A2942", 1, 4294967295u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D412B2942", 0, 0u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D412B2942", 1, 0u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D412B2942", 0, 1u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D412B2942", 1, 1u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D412B2942", 0, 255u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D412B2942", 1, 255u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D412B2942", 0, 4294967295u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D412B2942", 1, 4294967295u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C7D2942", 0, 0u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C7D2942", 1, 0u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C7D2942", 0, 1u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C7D2942", 1, 1u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C7D2942", 0, 255u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C7D2942", 1, 255u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C7D2942", 0, 4294967295u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B312C7D2942", 1, 4294967295u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B33323736387D417B33323736387D2942", 0, 0u, 187, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B33323736387D417B33323736387D2942", 1, 0u, 187, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B33323736387D417B33323736387D2942", 0, 1u, 187, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B33323736387D417B33323736387D2942", 1, 1u, 187, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B33323736387D417B33323736387D2942", 0, 255u, 187, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B33323736387D417B33323736387D2942", 1, 255u, 187, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B33323736387D417B33323736387D2942", 0, 4294967295u, 187, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D417B33323736387D417B33323736387D2942", 1, 4294967295u, 187, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D283F52292942", 0, 0u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D283F52292942", 1, 0u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D283F52292942", 0, 1u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D283F52292942", 1, 1u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D283F52292942", 0, 255u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D283F52292942", 1, 255u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D283F52292942", 0, 4294967295u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D283F52292942", 1, 4294967295u, 125, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4D41585F5641524C4F4F4B424548494E443D322942", 0, 0u, 160, 19, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4D41585F5641524C4F4F4B424548494E443D322942", 1, 0u, 160, 19, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4D41585F5641524C4F4F4B424548494E443D322942", 0, 1u, 160, 19, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4D41585F5641524C4F4F4B424548494E443D322942", 1, 1u, 160, 19, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4D41585F5641524C4F4F4B424548494E443D322942", 0, 255u, 160, 19, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4D41585F5641524C4F4F4B424548494E443D322942", 1, 255u, 160, 19, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4D41585F5641524C4F4F4B424548494E443D322942", 0, 4294967295u, 160, 19, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4D41585F5641524C4F4F4B424548494E443D322942", 1, 4294967295u, 160, 19, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D413F", 0, 0u, 114, 6, "InvalidSyntax", 0 )]
	[InlineData( "283F3C3D413F", 1, 0u, 114, 6, "InvalidSyntax", 0 )]
	[InlineData( "283F3C3D413F", 0, 1u, 114, 6, "InvalidSyntax", 0 )]
	[InlineData( "283F3C3D413F", 1, 1u, 114, 6, "InvalidSyntax", 0 )]
	[InlineData( "283F3C3D413F", 0, 255u, 114, 6, "InvalidSyntax", 0 )]
	[InlineData( "283F3C3D413F", 1, 255u, 114, 6, "InvalidSyntax", 0 )]
	[InlineData( "283F3C3D413F", 0, 4294967295u, 114, 6, "InvalidSyntax", 0 )]
	[InlineData( "283F3C3D413F", 1, 4294967295u, 114, 6, "InvalidSyntax", 0 )]
	[InlineData( "283F3C3D417B322C317D2942", 0, 0u, 104, 9, "InvalidSyntax", 5 )]
	[InlineData( "283F3C3D417B322C317D2942", 1, 0u, 104, 9, "InvalidSyntax", 5 )]
	[InlineData( "283F3C3D417B322C317D2942", 0, 1u, 104, 9, "InvalidSyntax", 5 )]
	[InlineData( "283F3C3D417B322C317D2942", 1, 1u, 104, 9, "InvalidSyntax", 5 )]
	[InlineData( "283F3C3D417B322C317D2942", 0, 255u, 104, 9, "InvalidSyntax", 5 )]
	[InlineData( "283F3C3D417B322C317D2942", 1, 255u, 104, 9, "InvalidSyntax", 5 )]
	[InlineData( "283F3C3D417B322C317D2942", 0, 4294967295u, 104, 9, "InvalidSyntax", 5 )]
	[InlineData( "283F3C3D417B322C317D2942", 1, 4294967295u, 104, 9, "InvalidSyntax", 5 )]
	[InlineData( "283F3C3D413F297B307D42", 0, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D413F297B307D42", 1, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D413F29282A414343455054297C42", 0, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	[InlineData( "283F3C3D413F29282A414343455054297C42", 1, 0u, 200, 0, "UnsupportedSyntax", 0 )]
	public void NativeCompileCeilingsKeepTheirOriginalDiagnostics( string hex, int mode, uint limit, int nativeCode, int nativeOffset, string code, int offset ) {
		var source = Convert.FromHexString( hex );
		var native = Assert.Throws<PcrePatternException>( () => Reference( source, mode, limit ) );
		Assert.Equal( nativeCode, (int)native.ErrorCode );
		Assert.EndsWith( $" at offset {nativeOffset}.", native.Message );
		var actual = PcreByteCompiler.Compile( source, Options( mode, limit ) );
		Assert.Null( actual.Pattern );
		Assert.Equal( Enum.Parse<PcreByteDiagnosticCode>( code ), actual.Diagnostic?.Code );
		Assert.Equal( offset, actual.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( 0, 0u )]
	[InlineData( 0, 256u )]
	[InlineData( 0, 4294967295u )]
	[InlineData( 1, 0u )]
	[InlineData( 1, 256u )]
	[InlineData( 1, 4294967295u )]
	public void ContextPreservesStableWorkBudgetsAndCancellationBeforeIntegration( int mode, uint limit ) {
		var source = "A"u8.ToArray();
		var native = Reference( source, mode, limit );
		AssertNative( native, source, 0, "0:1", "" );
		var steps = new List<int>();
		var plain = new List<int>();
		var options = Options( mode, limit ) with { WorkObserver = steps.Add };
		Compare( native, source, 0, Compile( source, options ).Match( source ), "context work" );
		Compare( native, source, 0, Compile( source, new PcreByteOptions { Utf8 = mode == 1, WorkObserver = plain.Add } ).Match( source ), "default work" );
		Assert.Equal( plain, steps );
		Assert.Equal( Enumerable.Range( 1, steps.Count ), steps );
		var workCount = steps.Count;
		Compare( native, source, 0, Compile( source, options with { MatchWorkLimit = workCount } ).Match( source ), "exact work" );
		var exhausted = Compile( source, options with { MatchWorkLimit = workCount - 1 } ).Match( source );
		Assert.Null( exhausted.Match );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, exhausted.Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( source, options with { WorkObserver = count => { if ( count == 2 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( source, cancellationToken: cancellation.Token ) );
	}

	[Theory]
	[InlineData( 256u )]
	[InlineData( 300u )]
	[InlineData( 4294967295u )]
	public void CompiledRaisedCeilingOwnsItsSourceAndContext( uint limit ) {
		var source = "(?<=A{255,256})(B)"u8.ToArray();
		var subject = Encoding.ASCII.GetBytes( String.Concat( new string( 'A', 256 ), "B" ) );
		var native = Reference( source, 0, limit );
		AssertNative( native, subject, 0, "256:1,256:1", "" );
		var options = Options( 0, limit );
		var compiled = Compile( source, options );
		Array.Fill( source, (byte)'x' );
		SetLimit( options, 0 );
		Compare( native, subject, 0, compiled.Match( subject ), "owned source/context" );
	}

	[Theory]
	[InlineData( 0, 0u )]
	[InlineData( 0, 4294967295u )]
	[InlineData( 1, 0u )]
	[InlineData( 1, 4294967295u )]
	[InlineData( 2, 0u )]
	[InlineData( 2, 4294967295u )]
	public void ContextDoesNotSpendCaptureDepthOrInstructionSlots( int selection, uint limit ) {
		var pattern = selection switch {
			0 => new string( 'A', 4095 ),
			1 => String.Concat( String.Concat( Enumerable.Repeat( "()", 256 ) ), "A" ),
			_ => String.Concat( String.Concat( Enumerable.Repeat( "(?:", 256 ) ), "A", new string( ')', 256 ) )
		};
		var source = Encoding.ASCII.GetBytes( pattern );
		var subject = selection == 0 ? source : "A"u8.ToArray();
		var spans = selection == 0 ? "0:4095" : selection == 1 ? String.Concat( "0:1,", String.Join( ",", Enumerable.Repeat( "0:0", 256 ) ) ) : "0:1";
		var native = Reference( source, 0, limit, selection == 0 );
		AssertNative( native, subject, 0, spans, "" );
		Compare( native, subject, 0, Compile( source, Options( 0, limit ) with { Literal = selection == 0, AllowLookaroundBackslashK = false } ).Match( subject ), "context capacity" );
	}

	[Theory]
	[InlineData( 0u )]
	[InlineData( 4294967295u )]
	public void ContextDoesNotRelaxTheInstructionLimit( uint limit ) {
		var source = Encoding.ASCII.GetBytes( new string( 'A', 4096 ) );
		AssertNative( Reference( source, 0, limit, true ), source, 0, "0:4096", "" );
		var actual = PcreByteCompiler.Compile( source, Options( 0, limit ) with { Literal = true, AllowLookaroundBackslashK = false } );
		Assert.Null( actual.Pattern );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, actual.Diagnostic?.Code );
	}

	[Theory]
	[InlineData( 0u )]
	[InlineData( 256u )]
	public void ContextPreservesUtfOffsetLimitAndCancellationPriority( uint limit ) {
		var source = "^A$"u8.ToArray();
		var options = Options( 1, limit );
		var compiled = Compile( source, options );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Subject, compiled.Match( new byte[] { 0xFF } ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset, compiled.Match( "é"u8.ToArray(), 1 ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset, compiled.Match( new byte[] { 0xFF }, -1 ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidWorkLimit, PcreByteCompiler.Compile( new byte[] { 0xFF }, options with { MatchWorkLimit = 0 } ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Pattern, PcreByteCompiler.Compile( new byte[] { 0xFF }, options ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () => compiled.Match( new byte[] { 0xFF }, -1, cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( source, options with { MatchWorkLimit = 0 }, cancellation.Token ) );
	}

	private static void AssertProtocols( string hex, int mode, uint limit, string protocols ) {
		var source = Convert.FromHexString( hex );
		var cases = protocols.Split( ';' ).Select( Protocol.Parse ).ToArray();
		var native = Reference( source, mode, limit );
		foreach ( var item in cases ) { AssertNative( native, item.Subject, item.Offset, item.Spans, item.Mark ); }
		var compiled = Compile( source, Options( mode, limit ) );
		foreach ( var item in cases ) { Compare( native, item.Subject, item.Offset, compiled.Match( item.Subject, item.Offset ), $"{hex}/{mode}/{limit}/{Convert.ToHexString( item.Subject )}/{item.Offset}" ); }
	}
	private readonly record struct Protocol( byte[] Subject, int Offset, string Spans, string Mark ) {
		internal static Protocol Parse( string value ) {
			var parts = value.Split( '/' );
			var subject = parts[0].StartsWith( '~' )
				? Encoding.ASCII.GetBytes( String.Concat( new string( 'A', Int32.Parse( parts[0].AsSpan( 1 ) ) ), "B" ) )
				: Convert.FromHexString( parts[0] );
			return new( subject, Int32.Parse( parts[1] ), parts[2], parts[3] );
		}
	}
	private static void SetLimit( PcreByteOptions options, uint limit ) {
		// Optional for tests-first compilation against the unchanged baseline.
		typeof( PcreByteOptions ).GetProperty( "MaxVarLookbehind" )?.SetValue( options, limit );
	}
	private static PcreByteOptions Options( int mode, uint limit ) {
		var options = new PcreByteOptions { Utf8 = ( mode & 1 ) != 0, MatchInvalidUtf = ( mode & 4 ) != 0, AllowLookaroundBackslashK = true };
		SetLimit( options, limit );
		return options;
	}
	private static PcreRegex8Bit Reference( byte[] source, int mode, uint limit, bool literal = false ) => new( source, Encoding.UTF8, new PcreRegexSettings {
		Options = PcreOptions.Compiled | (PcreOptions)( ( ( mode & 1 ) != 0 ? 0x80000u : 0u ) | ( ( mode & 4 ) != 0 ? 0x4000000u : 0u ) | ( literal ? 0x2000000u : 0u ) ),
		ExtraCompileOptions = (PcreExtraCompileOptions)( !literal && source.AsSpan().IndexOf( "\\K"u8 ) >= 0 ? 64 : 0 ),
		MaxVarLookbehind = limit, ParensLimit = 512, NewLine = (PcreNewLine)2, BackslashR = (PcreBackslashR)1
	} );
	private static PcreCompiledBytePattern Compile( byte[] source, PcreByteOptions options ) {
		var actual = PcreByteCompiler.Compile( source, options );
		Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( actual.Pattern );
	}
	private static void AssertNative( PcreRegex8Bit native, byte[] subject, int offset, string spans, string mark ) {
		var match = native.Match( subject, offset );
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
	private static void Compare( PcreRegex8Bit native, byte[] subject, int offset, PcreByteMatchResult actual, string detail ) {
		var expected = native.Match( subject, offset );
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
