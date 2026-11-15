using System.Buffers;
using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed partial class PcrePatternLengthLimitReferenceTests {
	[Theory]
	[InlineData( "41", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "284129", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "284129", 0, 2L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "284129", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "284129", 1, 2L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "284129", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "284129", 5, 2L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "417C42", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "417C42", 0, 2L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "417C42", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "417C42", 1, 2L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "417C42", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "417C42", 5, 2L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "413F", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "413F", 0, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "413F", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "413F", 1, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "413F", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "413F", 5, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5E4124", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5E4124", 0, 2L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5E4124", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5E4124", 1, 2L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5E4124", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5E4124", 5, 2L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C41415C7A", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C41415C7A", 0, 4L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C41415C7A", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C41415C7A", 1, 4L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C41415C7A", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C41415C7A", 5, 4L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C47", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C47", 0, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C47", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C47", 1, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C47", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C47", 5, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3C3D417B312C327D2942", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3C3D417B312C327D2942", 0, 11L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3C3D417B312C327D2942", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3C3D417B312C327D2942", 1, 11L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3C3D417B312C327D2942", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3C3D417B312C327D2942", 5, 11L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3D412941", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3D412941", 0, 5L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3D412941", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3D412941", 1, 5L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3D412941", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3D412941", 5, 5L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3C783E41295C6B3C783E", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3C783E41295C6B3C783E", 0, 11L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3C783E41295C6B3C783E", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3C783E41295C6B3C783E", 1, 11L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3C783E41295C6B3C783E", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3C783E41295C6B3C783E", 5, 11L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F28444546494E4529283F3C783E412929283F267829", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F28444546494E4529283F3C783E412929283F267829", 0, 22L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F28444546494E4529283F3C783E412929283F267829", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F28444546494E4529283F3C783E412929283F267829", 1, 22L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F28444546494E4529283F3C783E412929283F267829", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F28444546494E4529283F3C783E412929283F267829", 5, 22L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F6D295E4124", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F6D295E4124", 0, 6L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F6D295E4124", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F6D295E4124", 1, 6L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F6D295E4124", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F6D295E4124", 5, 6L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4D41524B3A4E29284129", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4D41524B3A4E29284129", 0, 11L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4D41524B3A4E29284129", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4D41524B3A4E29284129", 1, 11L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4D41524B3A4E29284129", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4D41524B3A4E29284129", 5, 11L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "415C4B", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "415C4B", 0, 2L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "415C4B", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "415C4B", 1, 2L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "415C4B", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "415C4B", 5, 2L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4C462941", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4C462941", 0, 5L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4C462941", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4C462941", 1, 5L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4C462941", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4C462941", 5, 5L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A5554462941", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A5554462941", 0, 6L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A5554462941", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A5554462941", 1, 6L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A5554462941", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A5554462941", 5, 6L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A554350295C77", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A554350295C77", 0, 7L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A554350295C77", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A554350295C77", 1, 7L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A554350295C77", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A554350295C77", 5, 7L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C515E245C45", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C515E245C45", 0, 5L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C515E245C45", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C515E245C45", 1, 5L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C515E245C45", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C515E245C45", 5, 5L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5B41425D", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5B41425D", 0, 3L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5B41425D", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5B41425D", 1, 3L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5B41425D", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5B41425D", 5, 3L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F78292320636F6D6D656E740A41", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F78292320636F6D6D656E740A41", 0, 14L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F78292320636F6D6D656E740A41", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F78292320636F6D6D656E740A41", 1, 14L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F78292320636F6D6D656E740A41", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F78292320636F6D6D656E740A41", 5, 14L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41283F23636F6D6D656E7429", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41283F23636F6D6D656E7429", 0, 11L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41283F23636F6D6D656E7429", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41283F23636F6D6D656E7429", 1, 11L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41283F23636F6D6D656E7429", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41283F23636F6D6D656E7429", 5, 11L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3A41297B307D42", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3A41297B307D42", 0, 8L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3A41297B307D42", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3A41297B307D42", 1, 8L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3A41297B307D42", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3A41297B307D42", 5, 8L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3C3D417C41412942", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3C3D417C41412942", 0, 9L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3C3D417C41412942", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3C3D417C41412942", 1, 9L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3C3D417C41412942", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3C3D417C41412942", 5, 9L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C3A9", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C3A9", 0, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C3A9", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C3A9", 1, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C3A9", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C3A9", 5, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "F09F9880", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "F09F9880", 0, 3L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "F09F9880", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "F09F9880", 1, 3L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "F09F9880", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "F09F9880", 5, 3L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "410042", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "410042", 0, 2L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "410042", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "410042", 1, 2L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "410042", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "410042", 5, 2L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A55544629", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A55544629", 0, 5L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A55544629", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A55544629", 1, 5L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A55544629", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A55544629", 5, 5L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4C4629", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4C4629", 0, 4L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4C4629", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4C4629", 1, 4L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4C4629", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4C4629", 5, 4L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F54454D50545929", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F54454D50545929", 0, 10L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F54454D50545929", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F54454D50545929", 1, 10L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F54454D50545929", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F54454D50545929", 5, 10L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F54454D5054595F4154535441525429", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F54454D5054595F4154535441525429", 0, 18L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F54454D5054595F4154535441525429", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F54454D5054595F4154535441525429", 1, 18L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F54454D5054595F4154535441525429", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4E4F54454D5054595F4154535441525429", 5, 18L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41", 8, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41", 9, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41", 13, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5E4124", 8, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5E4124", 8, 2L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5E4124", 9, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5E4124", 9, 2L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5E4124", 13, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5E4124", 13, 2L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A5554462941", 8, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A5554462941", 8, 6L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A5554462941", 9, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A5554462941", 9, 6L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A5554462941", 13, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A5554462941", 13, 6L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C787B443830307D", 8, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C787B443830307D", 8, 7L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C787B443830307D", 9, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C787B443830307D", 9, 7L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C787B443830307D", 13, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C787B443830307D", 13, 7L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C3A9", 8, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C3A9", 8, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C3A9", 9, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C3A9", 9, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C3A9", 13, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C3A9", 13, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "F09F9880", 8, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "F09F9880", 8, 3L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "F09F9880", 9, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "F09F9880", 9, 3L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "F09F9880", 13, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "F09F9880", 13, 3L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "410042", 8, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "410042", 8, 2L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "410042", 9, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "410042", 9, 2L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "410042", 13, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "410042", 13, 2L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "28", 8, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "28", 9, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "28", 13, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C", 8, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C", 9, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C", 13, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41", 512, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41", 513, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41", 517, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41", 1024, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41", 1025, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41", 1032, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41", 520, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C3A9", 512, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C3A9", 512, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C3A9", 513, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C3A9", 513, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C3A9", 517, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C3A9", 517, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C3A9", 1024, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C3A9", 1024, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C3A9", 1025, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C3A9", 1025, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C3A9", 1032, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C3A9", 1032, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C3A9", 520, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C3A9", 520, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4D41524B3A4E29284129", 512, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4D41524B3A4E29284129", 512, 11L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4D41524B3A4E29284129", 513, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4D41524B3A4E29284129", 513, 11L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4D41524B3A4E29284129", 517, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4D41524B3A4E29284129", 517, 11L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4D41524B3A4E29284129", 1024, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4D41524B3A4E29284129", 1024, 11L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4D41524B3A4E29284129", 1025, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4D41524B3A4E29284129", 1025, 11L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4D41524B3A4E29284129", 1032, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4D41524B3A4E29284129", 1032, 11L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4D41524B3A4E29284129", 520, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A4D41524B3A4E29284129", 520, 11L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3D415C4B2941", 4096, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3D415C4B2941", 4096, 7L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3D415C4B2941", 4097, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3D415C4B2941", 4097, 7L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3D415C4B2941", 4101, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F3D415C4B2941", 4101, 7L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "28", 0, -1L, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "28", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "28", 0, 1L, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "28", 0, 2L, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "28", 0, 4294967295L, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "28", 1, -1L, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "28", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "28", 1, 1L, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "28", 1, 2L, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "28", 1, 4294967295L, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "28", 5, -1L, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "28", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "28", 5, 1L, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "28", 5, 2L, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "28", 5, 4294967295L, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 0, -1L, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5B", 0, 1L, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 0, 2L, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 0, 4294967295L, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 1, -1L, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5B", 1, 1L, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 1, 2L, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 1, 4294967295L, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 5, -1L, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5B", 5, 1L, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 5, 2L, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 5, 4294967295L, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 0, -1L, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C", 0, 1L, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 0, 2L, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 0, 4294967295L, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 1, -1L, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C", 1, 1L, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 1, 2L, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 1, 4294967295L, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 5, -1L, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C", 5, 1L, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 5, 2L, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 5, 4294967295L, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "3F", 0, -1L, 109, 1, "InvalidSyntax", 0 )]
	[InlineData( "3F", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "3F", 0, 1L, 109, 1, "InvalidSyntax", 0 )]
	[InlineData( "3F", 0, 2L, 109, 1, "InvalidSyntax", 0 )]
	[InlineData( "3F", 0, 4294967295L, 109, 1, "InvalidSyntax", 0 )]
	[InlineData( "3F", 1, -1L, 109, 1, "InvalidSyntax", 0 )]
	[InlineData( "3F", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "3F", 1, 1L, 109, 1, "InvalidSyntax", 0 )]
	[InlineData( "3F", 1, 2L, 109, 1, "InvalidSyntax", 0 )]
	[InlineData( "3F", 1, 4294967295L, 109, 1, "InvalidSyntax", 0 )]
	[InlineData( "3F", 5, -1L, 109, 1, "InvalidSyntax", 0 )]
	[InlineData( "3F", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "3F", 5, 1L, 109, 1, "InvalidSyntax", 0 )]
	[InlineData( "3F", 5, 2L, 109, 1, "InvalidSyntax", 0 )]
	[InlineData( "3F", 5, 4294967295L, 109, 1, "InvalidSyntax", 0 )]
	[InlineData( "417B322C317D", 0, -1L, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "417B322C317D", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "417B322C317D", 0, 5L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "417B322C317D", 0, 6L, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "417B322C317D", 0, 7L, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "417B322C317D", 0, 4294967295L, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "417B322C317D", 1, -1L, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "417B322C317D", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "417B322C317D", 1, 5L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "417B322C317D", 1, 6L, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "417B322C317D", 1, 7L, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "417B322C317D", 1, 4294967295L, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "417B322C317D", 5, -1L, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "417B322C317D", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "417B322C317D", 5, 5L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "417B322C317D", 5, 6L, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "417B322C317D", 5, 7L, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "417B322C317D", 5, 4294967295L, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "282A554E4B4E4F574E29", 0, -1L, 160, 9, "UnsupportedSyntax", 0 )]
	[InlineData( "282A554E4B4E4F574E29", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A554E4B4E4F574E29", 0, 9L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A554E4B4E4F574E29", 0, 10L, 160, 9, "UnsupportedSyntax", 0 )]
	[InlineData( "282A554E4B4E4F574E29", 0, 11L, 160, 9, "UnsupportedSyntax", 0 )]
	[InlineData( "282A554E4B4E4F574E29", 0, 4294967295L, 160, 9, "UnsupportedSyntax", 0 )]
	[InlineData( "282A554E4B4E4F574E29", 1, -1L, 160, 9, "UnsupportedSyntax", 0 )]
	[InlineData( "282A554E4B4E4F574E29", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A554E4B4E4F574E29", 1, 9L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A554E4B4E4F574E29", 1, 10L, 160, 9, "UnsupportedSyntax", 0 )]
	[InlineData( "282A554E4B4E4F574E29", 1, 11L, 160, 9, "UnsupportedSyntax", 0 )]
	[InlineData( "282A554E4B4E4F574E29", 1, 4294967295L, 160, 9, "UnsupportedSyntax", 0 )]
	[InlineData( "282A554E4B4E4F574E29", 5, -1L, 160, 9, "UnsupportedSyntax", 0 )]
	[InlineData( "282A554E4B4E4F574E29", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A554E4B4E4F574E29", 5, 9L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A554E4B4E4F574E29", 5, 10L, 160, 9, "UnsupportedSyntax", 0 )]
	[InlineData( "282A554E4B4E4F574E29", 5, 11L, 160, 9, "UnsupportedSyntax", 0 )]
	[InlineData( "282A554E4B4E4F574E29", 5, 4294967295L, 160, 9, "UnsupportedSyntax", 0 )]
	[InlineData( "5C59", 0, -1L, 103, 2, "UnsupportedSyntax", 0 )]
	[InlineData( "5C59", 0, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C59", 0, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C59", 0, 2L, 103, 2, "UnsupportedSyntax", 0 )]
	[InlineData( "5C59", 0, 3L, 103, 2, "UnsupportedSyntax", 0 )]
	[InlineData( "5C59", 0, 4294967295L, 103, 2, "UnsupportedSyntax", 0 )]
	[InlineData( "5C59", 1, -1L, 103, 2, "UnsupportedSyntax", 0 )]
	[InlineData( "5C59", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C59", 1, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C59", 1, 2L, 103, 2, "UnsupportedSyntax", 0 )]
	[InlineData( "5C59", 1, 3L, 103, 2, "UnsupportedSyntax", 0 )]
	[InlineData( "5C59", 1, 4294967295L, 103, 2, "UnsupportedSyntax", 0 )]
	[InlineData( "5C59", 5, -1L, 103, 2, "UnsupportedSyntax", 0 )]
	[InlineData( "5C59", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C59", 5, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C59", 5, 2L, 103, 2, "UnsupportedSyntax", 0 )]
	[InlineData( "5C59", 5, 3L, 103, 2, "UnsupportedSyntax", 0 )]
	[InlineData( "5C59", 5, 4294967295L, 103, 2, "UnsupportedSyntax", 0 )]
	[InlineData( "FF", 1, -1L, -23, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "FF", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "FF", 1, 1L, -23, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "FF", 1, 2L, -23, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "FF", 1, 4294967295L, -23, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "FF", 5, -1L, -23, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "FF", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "FF", 5, 1L, -23, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "FF", 5, 2L, -23, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "FF", 5, 4294967295L, -23, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "FF", 9, -1L, -23, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "FF", 9, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "FF", 9, 1L, -23, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "FF", 9, 2L, -23, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "FF", 9, 4294967295L, -23, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "41FF", 1, -1L, -23, 1, "InvalidUtf8Pattern", 1 )]
	[InlineData( "41FF", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41FF", 1, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41FF", 1, 2L, -23, 1, "InvalidUtf8Pattern", 1 )]
	[InlineData( "41FF", 1, 3L, -23, 1, "InvalidUtf8Pattern", 1 )]
	[InlineData( "41FF", 1, 4294967295L, -23, 1, "InvalidUtf8Pattern", 1 )]
	[InlineData( "41FF", 5, -1L, -23, 1, "InvalidUtf8Pattern", 1 )]
	[InlineData( "41FF", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41FF", 5, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41FF", 5, 2L, -23, 1, "InvalidUtf8Pattern", 1 )]
	[InlineData( "41FF", 5, 3L, -23, 1, "InvalidUtf8Pattern", 1 )]
	[InlineData( "41FF", 5, 4294967295L, -23, 1, "InvalidUtf8Pattern", 1 )]
	[InlineData( "41FF", 9, -1L, -23, 1, "InvalidUtf8Pattern", 1 )]
	[InlineData( "41FF", 9, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41FF", 9, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41FF", 9, 2L, -23, 1, "InvalidUtf8Pattern", 1 )]
	[InlineData( "41FF", 9, 3L, -23, 1, "InvalidUtf8Pattern", 1 )]
	[InlineData( "41FF", 9, 4294967295L, -23, 1, "InvalidUtf8Pattern", 1 )]
	[InlineData( "C0AF", 1, -1L, -17, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "C0AF", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C0AF", 1, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C0AF", 1, 2L, -17, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "C0AF", 1, 3L, -17, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "C0AF", 1, 4294967295L, -17, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "C0AF", 5, -1L, -17, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "C0AF", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C0AF", 5, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C0AF", 5, 2L, -17, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "C0AF", 5, 3L, -17, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "C0AF", 5, 4294967295L, -17, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "C0AF", 9, -1L, -17, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "C0AF", 9, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C0AF", 9, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "C0AF", 9, 2L, -17, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "C0AF", 9, 3L, -17, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "C0AF", 9, 4294967295L, -17, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "EDA080", 1, -1L, -16, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "EDA080", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "EDA080", 1, 2L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "EDA080", 1, 3L, -16, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "EDA080", 1, 4L, -16, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "EDA080", 1, 4294967295L, -16, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "EDA080", 5, -1L, -16, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "EDA080", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "EDA080", 5, 2L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "EDA080", 5, 3L, -16, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "EDA080", 5, 4L, -16, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "EDA080", 5, 4294967295L, -16, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "EDA080", 9, -1L, -16, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "EDA080", 9, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "EDA080", 9, 2L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "EDA080", 9, 3L, -16, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "EDA080", 9, 4L, -16, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "EDA080", 9, 4294967295L, -16, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "F4908080", 1, -1L, -15, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "F4908080", 1, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "F4908080", 1, 3L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "F4908080", 1, 4L, -15, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "F4908080", 1, 5L, -15, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "F4908080", 1, 4294967295L, -15, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "F4908080", 5, -1L, -15, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "F4908080", 5, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "F4908080", 5, 3L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "F4908080", 5, 4L, -15, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "F4908080", 5, 5L, -15, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "F4908080", 5, 4294967295L, -15, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "F4908080", 9, -1L, -15, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "F4908080", 9, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "F4908080", 9, 3L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "F4908080", 9, 4L, -15, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "F4908080", 9, 5L, -15, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "F4908080", 9, 4294967295L, -15, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "282A5554462941", 32, -1L, 174, 6, "InvalidSyntax", 6 )]
	[InlineData( "282A5554462941", 32, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A5554462941", 32, 6L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A5554462941", 32, 7L, 174, 6, "InvalidSyntax", 6 )]
	[InlineData( "282A5554462941", 32, 8L, 174, 6, "InvalidSyntax", 6 )]
	[InlineData( "282A5554462941", 32, 4294967295L, 174, 6, "InvalidSyntax", 6 )]
	[InlineData( "41", 33, -1L, 174, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 33, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41", 33, 1L, 174, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 33, 2L, 174, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 33, 4294967295L, 174, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A5543502941", 64, -1L, 175, 6, "InvalidSyntax", 6 )]
	[InlineData( "282A5543502941", 64, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A5543502941", 64, 6L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A5543502941", 64, 7L, 175, 6, "InvalidSyntax", 6 )]
	[InlineData( "282A5543502941", 64, 8L, 175, 6, "InvalidSyntax", 6 )]
	[InlineData( "282A5543502941", 64, 4294967295L, 175, 6, "InvalidSyntax", 6 )]
	[InlineData( "41", 66, -1L, 175, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 66, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41", 66, 1L, 175, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 66, 2L, 175, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 66, 4294967295L, 175, 0, "InvalidSyntax", 0 )]
	[InlineData( "5C43", 128, -1L, 183, 2, "InvalidSyntax", 0 )]
	[InlineData( "5C43", 128, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C43", 128, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C43", 128, 2L, 183, 2, "InvalidSyntax", 0 )]
	[InlineData( "5C43", 128, 3L, 183, 2, "InvalidSyntax", 0 )]
	[InlineData( "5C43", 128, 4294967295L, 183, 2, "InvalidSyntax", 0 )]
	[InlineData( "5C43", 129, -1L, 183, 2, "InvalidSyntax", 0 )]
	[InlineData( "5C43", 129, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C43", 129, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "5C43", 129, 2L, 183, 2, "InvalidSyntax", 0 )]
	[InlineData( "5C43", 129, 3L, 183, 2, "InvalidSyntax", 0 )]
	[InlineData( "5C43", 129, 4294967295L, 183, 2, "InvalidSyntax", 0 )]
	[InlineData( "283F4329", 256, -1L, 203, 3, "InvalidSyntax", 0 )]
	[InlineData( "283F4329", 256, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F4329", 256, 3L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F4329", 256, 4L, 203, 3, "InvalidSyntax", 0 )]
	[InlineData( "283F4329", 256, 5L, 203, 3, "InvalidSyntax", 0 )]
	[InlineData( "283F4329", 256, 4294967295L, 203, 3, "InvalidSyntax", 0 )]
	[InlineData( "283F4329", 257, -1L, 203, 3, "InvalidSyntax", 0 )]
	[InlineData( "283F4329", 257, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F4329", 257, 3L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F4329", 257, 4L, 203, 3, "InvalidSyntax", 0 )]
	[InlineData( "283F4329", 257, 5L, 203, 3, "InvalidSyntax", 0 )]
	[InlineData( "283F4329", 257, 4294967295L, 203, 3, "InvalidSyntax", 0 )]
	[InlineData( "41", 24, -1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 24, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 24, 1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 24, 2L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 24, 4294967295L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 40, -1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 40, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 40, 1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 40, 2L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 40, 4294967295L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 10, -1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 10, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 10, 1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 10, 2L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 10, 4294967295L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 264, -1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 264, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 264, 1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 264, 2L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 264, 4294967295L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 136, -1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 136, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 136, 1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 136, 2L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 136, 4294967295L, 192, 0, "InvalidSyntax", 0 )]
	public void NativeLengthAdmissionPreservesDiagnosticPriority( string hex, int mode, long maximum, int nativeCode, int nativeOffset, string code, int offset ) {
		var source = Convert.FromHexString( hex );
		var native = Assert.Throws<PcrePatternException>( () => Reference( source, mode, maximum ) );
		Assert.Equal( nativeCode, (int)native.ErrorCode );
		Assert.EndsWith( $" at offset {nativeOffset}.", native.Message );
		var actual = PcreByteCompiler.Compile( source, Options( mode, maximum ) );
		Assert.Null( actual.Pattern );
		Assert.Equal( code, actual.Diagnostic?.Code.ToString() );
		Assert.Equal( offset, actual.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( 0L )]
	[InlineData( 3L )]
	public void ExceededCeilingRejectsBeforeReadingCallerMemory( long maximum ) {
		var native = Assert.Throws<PcrePatternException>( () => Reference( "AAAA"u8.ToArray(), 0, maximum ) );
		Assert.Equal( 188, (int)native.ErrorCode );
		Assert.EndsWith( " at offset 0.", native.Message );
		using var memory = new UnreadablePatternMemory();
		var actual = PcreByteCompiler.Compile( memory.Pattern, Options( 0, maximum ) );
		Assert.Null( actual.Pattern );
		Assert.Equal( "PatternLengthLimitExceeded", actual.Diagnostic?.Code.ToString() );
		Assert.Equal( 0, actual.Diagnostic?.ByteOffset );
		Assert.False( memory.ReadRequested );
	}

	[Theory]
	[InlineData( 0L )]
	[InlineData( 3L )]
	public void ExistingCompilePriorityWinsWithoutReadingCallerMemory( long maximum ) {
		using var memory = new UnreadablePatternMemory();
		var options = Options( 0, maximum );
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( memory.Pattern, options, cancellation.Token ) );
		var work = PcreByteCompiler.Compile( memory.Pattern, options with { MatchWorkLimit = 0 } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidWorkLimit, work.Diagnostic?.Code );
		Assert.Null( work.Pattern );
		var literal = PcreByteCompiler.Compile( memory.Pattern, options with { Literal = true, DotAll = true } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, literal.Diagnostic?.Code );
		Assert.Null( literal.Pattern );
		Assert.False( memory.ReadRequested );
	}

	private sealed class UnreadablePatternMemory : MemoryManager<byte> {
		internal bool ReadRequested { get; private set; }
		internal ReadOnlyMemory<byte> Pattern => CreateMemory( 4 );
		public override Span<byte> GetSpan() {
			ReadRequested = true;
			throw new InvalidOperationException( "rejected memory must not be read" );
		}
		public override MemoryHandle Pin( int elementIndex = 0 ) => throw new NotSupportedException();
		public override void Unpin() { }
		protected override void Dispose( bool disposing ) { }
	}

	[Theory]
	[InlineData( 1L )]
	[InlineData( 2L )]
	[InlineData( 4294967295L )]
	public void SuppliedViewOwnsOnlyItsAcceptedSourceAndContext( long maximum ) {
		var source = new byte[] { 0xFF, (byte)'A', 0xFF };
		var native = Reference( "A"u8.ToArray(), 1, maximum );
		AssertNative( native, "A"u8.ToArray(), 0, "0:1", "" );
		var options = Options( 1, maximum );
		var actual = PcreByteCompiler.Compile( source.AsMemory( 1, 1 ), options );
		Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
		var compiled = Assert.IsType<PcreCompiledBytePattern>( actual.Pattern );
		Array.Fill( source, (byte)'x' );
		SetMaximum( options, 0 );
		Compare( native, "A"u8.ToArray(), 0, compiled.Match( "A"u8.ToArray() ), "owned accepted view" );
	}

	[Theory]
	[InlineData( 0, -1L )]
	[InlineData( 0, 1L )]
	[InlineData( 0, 4294967295L )]
	[InlineData( 1, -1L )]
	[InlineData( 1, 1L )]
	[InlineData( 1, 4294967295L )]
	public void AcceptedLengthPreservesStableWorkBudgetsAndCancellationBeforeIntegration( int mode, long maximum ) {
		var source = "A"u8.ToArray();
		var native = Reference( source, mode, maximum );
		AssertNative( native, source, 0, "0:1", "" );
		var steps = new List<int>();
		var plain = new List<int>();
		var options = Options( mode, maximum ) with { WorkObserver = steps.Add };
		Compare( native, source, 0, Compile( source, options ).Match( source ), "limited context work" );
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
	[InlineData( 0, false )]
	[InlineData( 0, true )]
	[InlineData( 1, false )]
	[InlineData( 1, true )]
	[InlineData( 2, false )]
	[InlineData( 2, true )]
	public void ExactAndUnrestrictedLengthsKeepCaptureDepthAndInstructionCapacities( int selection, bool high ) {
		var pattern = selection switch {
			0 => new string( 'A', 4095 ),
			1 => String.Concat( String.Concat( Enumerable.Repeat( "()", 256 ) ), "A" ),
			_ => String.Concat( String.Concat( Enumerable.Repeat( "(?:", 256 ) ), "A", new string( ')', 256 ) )
		};
		var source = Encoding.ASCII.GetBytes( pattern );
		var maximum = high ? 4294967295L : source.Length;
		var mode = selection == 0 ? 8 : 0;
		var subject = selection == 0 ? source : "A"u8.ToArray();
		var spans = selection == 0 ? "0:4095" : selection == 1 ? String.Concat( "0:1,", String.Join( ",", Enumerable.Repeat( "0:0", 256 ) ) ) : "0:1";
		var native = Reference( source, mode, maximum );
		AssertNative( native, subject, 0, spans, "" );
		Compare( native, subject, 0, Compile( source, Options( mode, maximum ) ).Match( subject ), "shared capacity" );
	}

	[Theory]
	[InlineData( -1L )]
	[InlineData( 4096L )]
	[InlineData( 4294967295L )]
	public void AcceptedLengthDoesNotRelaxTheInstructionLimit( long maximum ) {
		var source = Encoding.ASCII.GetBytes( new string( 'A', 4096 ) );
		AssertNative( Reference( source, 8, maximum ), source, 0, "0:4096", "" );
		var actual = PcreByteCompiler.Compile( source, Options( 8, maximum ) );
		Assert.Null( actual.Pattern );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, actual.Diagnostic?.Code );
	}

	[Theory]
	[InlineData( 1L )]
	[InlineData( 4294967295L )]
	public void AcceptedLengthRetainsUtfOffsetAndCancellationPriority( long maximum ) {
		var source = "A"u8.ToArray();
		var options = Options( 1, maximum );
		var compiled = Compile( source, options );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Subject, compiled.Match( new byte[] { 0xFF } ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset, compiled.Match( "é"u8.ToArray(), 1 ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset, compiled.Match( new byte[] { 0xFF }, -1 ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidWorkLimit, PcreByteCompiler.Compile( new byte[] { 0xFF }, options with { MatchWorkLimit = 0 } ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Pattern, PcreByteCompiler.Compile( new byte[] { 0xFF }, options ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () => compiled.Match( new byte[] { 0xFF }, -1, cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( source, Options( 1, 0 ), cancellation.Token ) );
	}

	private static void AssertProtocols( string hex, int mode, long maximum, string protocols ) {
		var source = Convert.FromHexString( hex );
		var cases = protocols.Split( ';' ).Select( Protocol.Parse ).ToArray();
		var native = Reference( source, mode, maximum );
		foreach ( var item in cases ) { AssertNative( native, item.Subject, item.Offset, item.Spans, item.Mark ); }
		var compiled = Compile( source, Options( mode, maximum ) );
		foreach ( var item in cases ) { Compare( native, item.Subject, item.Offset, compiled.Match( item.Subject, item.Offset ), $"{hex}/{mode}/{maximum}/{Convert.ToHexString( item.Subject )}/{item.Offset}" ); }
	}
	private readonly record struct Protocol( byte[] Subject, int Offset, string Spans, string Mark ) {
		internal static Protocol Parse( string value ) {
			var parts = value.Split( '/' );
			return new( Convert.FromHexString( parts[0] ), Int32.Parse( parts[1] ), parts[2], parts[3] );
		}
	}
	private static void SetMaximum( PcreByteOptions options, long maximum ) {
		// Optional only for compiling the unchanged tests-first baseline.
		typeof( PcreByteOptions ).GetProperty( "MaxPatternLength" )?.SetValue( options, maximum < 0 ? null : (object)(uint)maximum );
	}
	private static PcreByteOptions Options( int mode, long maximum ) {
		var options = new PcreByteOptions {
			Utf8 = ( mode & 1 ) != 0, Ucp = ( mode & 2 ) != 0, MatchInvalidUtf = ( mode & 4 ) != 0,
			Literal = ( mode & 8 ) != 0, Multiline = ( mode & 16 ) != 0, NeverUtf = ( mode & 32 ) != 0,
			NeverUcp = ( mode & 64 ) != 0, NeverBackslashC = ( mode & 128 ) != 0,
			NeverCallout = ( mode & 256 ) != 0, MatchLine = ( mode & 512 ) != 0,
			MatchWord = ( mode & 1024 ) != 0, AllowLookaroundBackslashK = ( mode & 4096 ) != 0
		};
		SetMaximum( options, maximum );
		return options;
	}
	private static PcreRegex8Bit Reference( byte[] source, int mode, long maximum ) {
		// PCRE.NET's native shim uses zero as "unset" for this context field.
		// For nonempty zero-limit source, adding one byte and using limit one
		// preserves native admission/option-error priority without parsing it.
		if ( maximum == 0 && source.Length > 0 ) {
			var padded = new byte[source.Length + 1];
			source.CopyTo( padded, 1 );
			source = padded;
			maximum = 1;
		}
		return new( source, Encoding.UTF8, new PcreRegexSettings {
			Options = PcreOptions.Compiled | (PcreOptions)( ( ( mode & 1 ) != 0 ? 0x80000u : 0u ) | ( ( mode & 2 ) != 0 ? 0x20000u : 0u )
				| ( ( mode & 4 ) != 0 ? 0x4000000u : 0u ) | ( ( mode & 8 ) != 0 ? 0x2000000u : 0u )
				| ( ( mode & 16 ) != 0 ? 0x400u : 0u ) | ( ( mode & 32 ) != 0 ? 0x1000u : 0u )
				| ( ( mode & 64 ) != 0 ? 0x800u : 0u ) | ( ( mode & 128 ) != 0 ? 0x100000u : 0u ) ),
			ExtraCompileOptions = (PcreExtraCompileOptions)( ( ( mode & 256 ) != 0 ? 0x8000 : 0 ) | ( ( mode & 512 ) != 0 ? 8 : 0 ) | ( ( mode & 1024 ) != 0 ? 4 : 0 ) | ( ( mode & 4096 ) != 0 ? 64 : 0 ) ),
			MaxPatternLength = maximum < 0 ? null : (uint)maximum,
			ParensLimit = 512, NewLine = (PcreNewLine)2, BackslashR = (PcreBackslashR)1
		} );
	}
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
