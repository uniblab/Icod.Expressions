using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using PCRE;

namespace Icod.Expressions.Tests.Pcre;

// Test-only native reference ABI. No managed width-16 matcher is implemented.
internal sealed record PcreNative16Protocol(
	string Kind, int ErrorCode, uint ErrorOffset, string Captures, string Mark, string Names,
	nuint JitSize, uint Options
);

internal static class PcreNative16Oracle {
	private const int MaximumUnits = 4096;

	internal static PcreNative16Protocol Execute( ushort[] pattern, ushort[] subject, int mode, uint offset ) =>
		ExecuteCore( pattern, subject, mode, offset, false, 0, 0 );

	internal static PcreNative16Protocol ExecuteLiteral( ushort[] pattern, ushort[] subject, bool utf, uint offset,
		int defaults = 0, int mask = 0 ) => ExecuteCore( pattern, subject, utf ? 1 : 0, offset, true, defaults, mask );

	internal static PcreNative16Protocol ExecuteSequence( ushort[] pattern, ushort[] subject, bool utf, uint offset,
		int defaults = 0, int mask = 0 ) => ExecuteCore( pattern, subject, utf ? 1 : 0, offset, false, defaults, mask );

	private static PcreNative16Protocol ExecuteCore( ushort[] pattern, ushort[] subject, int mode, uint offset,
		bool literal, int defaults, int mask ) {
		ArgumentNullException.ThrowIfNull( pattern );
		ArgumentNullException.ThrowIfNull( subject );
		if ( mode < 0 || ( mode & ~7 ) != 0 ) { throw new ArgumentOutOfRangeException( nameof( mode ) ); }
		if ( defaults is < 0 or > 63 ) { throw new ArgumentOutOfRangeException( nameof( defaults ) ); }
		if ( mask is < 0 or > 63 ) { throw new ArgumentOutOfRangeException( nameof( mask ) ); }
		if ( pattern.Length > MaximumUnits ) { throw new ArgumentOutOfRangeException( nameof( pattern ) ); }
		if ( subject.Length > MaximumUnits ) { throw new ArgumentOutOfRangeException( nameof( subject ) ); }
		if ( IntPtr.Size != 8 || !AbiLayout().SequenceEqual( new[] { 64, 40, 80, 16, 24, 56, 32, 32, 56, 64, 8 } ) ) {
			throw new PlatformNotSupportedException( "The pinned native reference ABI requires the qualified 64-bit layout." );
		}
		using var module = new NativeModule();
		using var patternMemory = NativeMemory.ForUnits( pattern );
		using var subjectMemory = NativeMemory.ForUnits( subject );
		var input = new CompileInput {
			Pattern = patternMemory.Pointer, Length = (uint)pattern.Length,
			Flags = ( ( mode & 1 ) != 0 ? 0x80000u : 0u ) | ( ( mode & 2 ) != 0 ? 0x20000u : 0u )
				| ( literal ? 0x02000000u : 0u ) | ( ( defaults & 1 ) != 0 ? 0x80000000u : 0u )
				| ( ( defaults & 2 ) != 0 ? 0x20000000u : 0u ),
			JitFlags = 1, Newline = 2, Bsr = 1, NestLimit = 512, MaxPatternLength = MaximumUnits,
			ExtraOptions = ( mode & 4 ) != 0 ? 64u : 0u, MaxLookbehind = 255,
			MaxCompiledLength = 1_048_576, OptimizationCount = 0, Optimizations = 0
		};
		var compiled = new CompileResult { Code = 0, Error = 0, ErrorOffset = 0, Captures = 0, Names = 0, EntrySize = 0, NameTable = 0 };
		try {
			module.Compile( ref input, ref compiled );
			if ( compiled.Code == 0 ) {
				return new( "E", compiled.Error, compiled.ErrorOffset, "", "", "", 0, 0 );
			}
			if ( compiled.Error != 0 || compiled.Captures > 256 || compiled.Names > 256 ) {
				throw new InvalidOperationException( "Unexpected native compile metadata." );
			}
			using var info = new NativeMemory( 8 );
			CheckInfo( module, compiled.Code, 0, info.Pointer );
			var options = unchecked( (uint)Marshal.ReadInt32( info.Pointer ) );
			CheckInfo( module, compiled.Code, 10, info.Pointer );
			var jitSize = checked( (nuint)unchecked( (ulong)Marshal.ReadInt64( info.Pointer ) ) );
			using var vector = new NativeMemory( checked( ( (int)compiled.Captures + 1 ) * 2 * IntPtr.Size ) );
			var matchInput = new MatchInput {
				Code = compiled.Code, Subject = subjectMemory.Pointer, Length = (uint)subject.Length,
				Start = offset, Options = (uint)PcreNativeMatchPolicy.Options( defaults, mask ),
				Settings = new MatchSettings { Limit = 1_000_000, Depth = 1024, Heap = 65536, OffsetLimit = 0, JitStack = 0 },
				Vector = vector.Pointer, Callout = 0, CalloutData = 0
			};
			var matched = new MatchResult { Code = 0, Mark = 0 };
			module.Match( ref matchInput, ref matched );
			if ( matched.Code < 0 && matched.Code != -1 ) {
				return new( "X", matched.Code, 0, "", "", "", jitSize, options );
			}
			if ( matched.Code == 0 ) { throw new InvalidOperationException( "Native capture vector is unexpectedly too small." ); }
			var names = ReadNames( compiled, pattern.Length );
			if ( matched.Code == -1 ) { return new( "N", 0, 0, "", "", names, jitSize, options ); }
			var captures = new List<string>();
			for ( var i = 0; i <= compiled.Captures; i++ ) {
				var start = unchecked( (ulong)Marshal.ReadInt64( vector.Pointer, checked( i * 16 ) ) );
				if ( start == UInt64.MaxValue ) { captures.Add( "-" ); continue; }
				var end = unchecked( (ulong)Marshal.ReadInt64( vector.Pointer, checked( i * 16 + 8 ) ) );
				if ( start > (ulong)subject.Length || end > (ulong)subject.Length ) {
					throw new InvalidOperationException( "Native capture endpoint exceeds the original subject." );
				}
				captures.Add( String.Concat( start.ToString( CultureInfo.InvariantCulture ), ":",
					( (long)end - (long)start ).ToString( CultureInfo.InvariantCulture ) ) );
			}
			return new( "M", 0, 0, String.Join( ",", captures ), ReadWords( matched.Mark, pattern.Length + 1 ), names, jitSize, options );
		}
		finally {
			if ( compiled.Code != 0 ) { module.FreeCode( compiled.Code ); }
		}
	}

	internal static int[] AbiLayout() => new[] {
		Marshal.SizeOf<CompileInput>(), Marshal.SizeOf<CompileResult>(), Marshal.SizeOf<MatchInput>(),
		Marshal.SizeOf<MatchResult>(), Marshal.SizeOf<MatchSettings>(),
		Offset<CompileInput>( nameof( CompileInput.Optimizations ) ),
		Offset<CompileResult>( nameof( CompileResult.NameTable ) ),
		Offset<MatchInput>( nameof( MatchInput.Settings ) ), Offset<MatchInput>( nameof( MatchInput.Vector ) ),
		Offset<MatchInput>( nameof( MatchInput.Callout ) ), Offset<MatchResult>( nameof( MatchResult.Mark ) )
	};
	private static int Offset<T>( string name ) where T : struct => checked( (int)Marshal.OffsetOf<T>( name ) );
	private static void CheckInfo( NativeModule module, nint code, uint key, nint data ) {
		if ( module.Info( code, key, data ) != 0 ) { throw new InvalidOperationException( "Native pattern-info query failed." ); }
	}
	private static string ReadNames( CompileResult compiled, int patternLength ) {
		if ( compiled.Names == 0 ) { return ""; }
		if ( compiled.NameTable == 0 || compiled.EntrySize < 2 || compiled.EntrySize > patternLength + 2 ) {
			throw new InvalidOperationException( "Invalid native name table." );
		}
		var entries = new List<string>();
		for ( var i = 0u; i < compiled.Names; i++ ) {
			var entry = compiled.NameTable + checked( (int)( i * compiled.EntrySize * 2 ) );
			var number = unchecked( (ushort)Marshal.ReadInt16( entry ) );
			if ( number == 0 || number > compiled.Captures ) { throw new InvalidOperationException( "Invalid native named slot." ); }
			entries.Add( String.Concat( ReadWords( entry + 2, checked( (int)compiled.EntrySize - 1 ) ), "=",
				number.ToString( CultureInfo.InvariantCulture ) ) );
		}
		return String.Join( ";", entries );
	}
	private static string ReadWords( nint pointer, int maximumUnits ) {
		if ( pointer == 0 ) { return ""; }
		var result = new StringBuilder();
		for ( var i = 0; i < maximumUnits; i++ ) {
			var unit = unchecked( (ushort)Marshal.ReadInt16( pointer, checked( i * 2 ) ) );
			if ( unit == 0 ) { return result.ToString(); }
			result.Append( unit.ToString( "X4", CultureInfo.InvariantCulture ) );
		}
		throw new InvalidOperationException( "Native word string has no bounded terminator." );
	}

	[StructLayout( LayoutKind.Sequential )]
	private struct CompileInput {
		public nint Pattern;
		public uint Length, Flags, JitFlags, Newline, Bsr, NestLimit, MaxPatternLength, ExtraOptions,
			MaxLookbehind, MaxCompiledLength, OptimizationCount;
		public nint Optimizations;
	}
	[StructLayout( LayoutKind.Sequential )]
	private struct CompileResult {
		public nint Code;
		public int Error;
		public uint ErrorOffset, Captures, Names, EntrySize;
		public nint NameTable;
	}
	[StructLayout( LayoutKind.Sequential )]
	private struct MatchSettings {
		public uint Limit, Depth, Heap, OffsetLimit;
		public nint JitStack;
	}
	[StructLayout( LayoutKind.Sequential )]
	private struct MatchInput {
		public nint Code, Subject;
		public uint Length, Start, Options;
		public MatchSettings Settings;
		public nint Vector, Callout, CalloutData;
	}
	[StructLayout( LayoutKind.Sequential )]
	private struct MatchResult {
		public int Code;
		public nint Mark;
	}
	[UnmanagedFunctionPointer( CallingConvention.Cdecl )]
	private delegate void CompileFunction( ref CompileInput input, ref CompileResult result );
	[UnmanagedFunctionPointer( CallingConvention.Cdecl )]
	private delegate void MatchFunction( ref MatchInput input, ref MatchResult result );
	[UnmanagedFunctionPointer( CallingConvention.Cdecl )]
	private delegate void FreeFunction( nint code );
	[UnmanagedFunctionPointer( CallingConvention.Cdecl )]
	private delegate int InfoFunction( nint code, uint key, nint result );

	private sealed class NativeModule : IDisposable {
		private readonly nint handle;
		internal readonly CompileFunction Compile;
		internal readonly MatchFunction Match;
		internal readonly FreeFunction FreeCode;
		internal readonly InfoFunction Info;
		internal NativeModule() {
			handle = NativeLibrary.Load( "PCRE.NET.Native", typeof( PcreRegex ).Assembly, null );
			try {
				Compile = Export<CompileFunction>( "pcrenet_compile_16" );
				Match = Export<MatchFunction>( "pcrenet_match_16" );
				FreeCode = Export<FreeFunction>( "pcrenet_code_free_16" );
				Info = Export<InfoFunction>( "pcrenet_pattern_info_16" );
			}
			catch { NativeLibrary.Free( handle ); throw; }
		}
		private T Export<T>( string name ) where T : Delegate => Marshal.GetDelegateForFunctionPointer<T>( NativeLibrary.GetExport( handle, name ) );
		public void Dispose() => NativeLibrary.Free( handle );
	}
	private sealed class NativeMemory : SafeHandleZeroOrMinusOneIsInvalid {
		internal NativeMemory( int bytes ) : base( true ) {
			SetHandle( Marshal.AllocHGlobal( Math.Max( bytes, 8 ) ) );
		}
		internal nint Pointer => DangerousGetHandle();
		internal static NativeMemory ForUnits( ushort[] values ) {
			var memory = new NativeMemory( checked( ( values.Length + 1 ) * 2 ) );
			try {
				for ( var i = 0; i < values.Length; i++ ) { Marshal.WriteInt16( memory.Pointer, checked( i * 2 ), unchecked( (short)values[i] ) ); }
				Marshal.WriteInt16( memory.Pointer, checked( values.Length * 2 ), 0 );
				return memory;
			}
			catch { memory.Dispose(); throw; }
		}
		protected override bool ReleaseHandle() { Marshal.FreeHGlobal( handle ); return true; }
	}
}
