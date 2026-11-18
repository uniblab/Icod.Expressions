namespace Icod.Expressions.Tests.Pcre;

// Test-only native reference ABI. No managed width-16 matcher is implemented.
internal sealed record PcreNative16Protocol(
	string Kind, int ErrorCode, uint ErrorOffset, string Captures, string Mark, string Names,
	nuint JitSize, uint Options
);

internal static class PcreNative16Oracle {
	internal static PcreNative16Protocol Execute( ushort[] pattern, ushort[] subject, int mode, uint offset ) =>
		throw new NotImplementedException( "Direct native width-16 oracle is not implemented yet." );
	internal static int[] AbiLayout() => throw new NotImplementedException();
}
