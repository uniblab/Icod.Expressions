using System.Reflection;
using Icod.CommandFramework.RegularExpressions;
using Icod.CommandFramework.Text;

// List compiled declarations at the public extraction or internal PCRE boundary.
// Keep this tool independent of source parsing and external API-diff packages.
var pcreInternal = args.Length == 1 && args[0] == "--pcre-internal";
if ( args.Length != 0 && !pcreInternal ) {
	Console.Error.WriteLine( "Usage: RegularExpressionApiSnapshot [--pcre-internal]" );
	Environment.ExitCode = 2;
	return;
}
var assembly = typeof( IRegularExpressionProvider ).Assembly;
var nullable = new NullabilityInfoContext();
var pcreNames = new[] {
	"PcreByteCapture", "PcreByteCompileResult", "PcreByteCompiler", "PcreByteDiagnostic",
	"PcreByteDiagnosticCode", "PcreByteMatch", "PcreByteMatchResult", "PcreByteOptions",
	"PcreCaptureInfo", "PcreCompiledBytePattern", "PcreMatchPolicy"
};
var included = pcreInternal
	? pcreNames.Select( name => assembly.GetType( $"Icod.Expressions.Pcre.{name}", throwOnError: true )! ).ToArray()
	: assembly.GetExportedTypes()
	.Where( type => type.Namespace == "Icod.CommandFramework.RegularExpressions"
		|| type == typeof( TextDecodingMode )
		|| type == typeof( InvalidEncodingPolicy ) )
	.OrderBy( type => type.FullName, StringComparer.Ordinal )
	.ToArray();

if ( pcreInternal ) {
	Console.WriteLine( $"# {assembly.GetName().Name} compiled internal PCRE API" );
	Console.WriteLine( "# Consumer boundary declarations, including compiler-generated record members." );
	Console.WriteLine( "# Internal boundary members: PcreByteMatch.MarkName, PcreByteOptions.DefaultMatchWorkLimit." );
	Console.WriteLine( $"# Selected types: {included.Length}" );
	var exported = assembly.GetExportedTypes().Where( type => type.Namespace == "Icod.Expressions.Pcre" )
		.Select( type => type.FullName ).OrderBy( name => name, StringComparer.Ordinal ).ToArray();
	Console.WriteLine( $"# Public PCRE types: {( exported.Length == 0 ? "none" : string.Join( ", ", exported ) )}" );
} else {
	Console.WriteLine( $"# {assembly.GetName().Name} {assembly.GetName().Version} compiled regex API" );
	Console.WriteLine( "# Public declared members, including compiler-generated record members; inherited framework members excluded." );
	Console.WriteLine( $"# Top-level regex types: {included.Count( type => type.Namespace == "Icod.CommandFramework.RegularExpressions" && !type.IsNested )}" );
	Console.WriteLine( "# Referenced external text enums: TextDecodingMode, InvalidEncodingPolicy" );
}

foreach ( var type in included ) {
	var kind = type.IsInterface ? "interface" : type.IsEnum ? "enum" : type.IsValueType ? "struct"
		: type.IsAbstract && type.IsSealed ? "static class" : type.IsSealed ? "sealed class"
		: type.IsAbstract ? "abstract class" : "class";
	var ancestry = new List<string>();
	if ( type.BaseType is { } baseType && baseType != typeof( object ) && baseType != typeof( ValueType ) && baseType != typeof( Enum ) ) ancestry.Add( Format( baseType ) );
	ancestry.AddRange( type.GetInterfaces().Select( iface => Format( iface ) ).OrderBy( name => name, StringComparer.Ordinal ) );
	var visibility = pcreInternal ? type.IsVisible ? "public " : "internal " : string.Empty;
	Console.WriteLine( $"{visibility}{kind} {Format( type )}{( ancestry.Count == 0 ? string.Empty : " : " + string.Join( ", ", ancestry ) )}" );
	if ( type.IsEnum ) {
		foreach ( var field in type.GetFields( BindingFlags.Public | BindingFlags.Static ).OrderBy( field => field.Name, StringComparer.Ordinal ) ) {
			Console.WriteLine( $"  {field.Name} = {Convert.ToInt64( field.GetRawConstantValue(), System.Globalization.CultureInfo.InvariantCulture )}" );
		}
		continue;
	}

	var entries = new List<string>();
	foreach ( var constructor in type.GetConstructors( BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly ) ) {
		entries.Add( $"ctor({Parameters( constructor.GetParameters() )})" );
	}
	foreach ( var property in type.GetProperties( BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly ) ) {
		var access = property.GetAccessors().Where( method => method.IsPublic ).ToArray();
		if ( access.Length == 0 ) continue;
		var getter = access.Any( method => method == property.GetMethod ) ? "get; " : string.Empty;
		var setter = property.SetMethod is { IsPublic: true } set
			? set.ReturnParameter.GetRequiredCustomModifiers().Any( modifier => modifier.FullName == "System.Runtime.CompilerServices.IsExternalInit" ) ? "init; " : "set; "
			: string.Empty;
		var index = property.GetIndexParameters();
		var name = index.Length == 0 ? property.Name : $"this[{Parameters( index )}]";
		var storage = access[0].IsStatic ? "static " : string.Empty;
		entries.Add( $"property {storage}{Format( property.PropertyType, nullable.Create( property ) )} {name} {{ {getter}{setter}}}" );
	}
	foreach ( var field in type.GetFields( BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly ) ) {
		entries.Add( $"field {( field.IsStatic ? "static " : string.Empty )}{Format( field.FieldType, nullable.Create( field ) )} {field.Name}" );
	}
	foreach ( var method in type.GetMethods( BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly ) ) {
		if ( method.IsSpecialName && ( method.Name.StartsWith( "get_", StringComparison.Ordinal ) || method.Name.StartsWith( "set_", StringComparison.Ordinal ) || method.Name.StartsWith( "add_", StringComparison.Ordinal ) || method.Name.StartsWith( "remove_", StringComparison.Ordinal ) ) ) continue;
		var generic = method.IsGenericMethod ? $"<{string.Join( ", ", method.GetGenericArguments().Select( arg => arg.Name ) )}>" : string.Empty;
		entries.Add( $"method {( method.IsStatic ? "static " : string.Empty )}{Format( method.ReturnType, nullable.Create( method.ReturnParameter ) )} {method.Name}{generic}({Parameters( method.GetParameters() )})" );
	}
	foreach ( var entry in entries.OrderBy( entry => entry, StringComparer.Ordinal ) ) Console.WriteLine( $"  {entry}" );
	if ( pcreInternal && type.Name == "PcreByteMatch" ) {
		var mark = type.GetProperty( "MarkName", BindingFlags.NonPublic | BindingFlags.Instance )!;
		if ( mark.GetMethod is not { IsAssembly: true } || mark.SetMethod is not { IsAssembly: true } ) {
			throw new InvalidOperationException( "PcreByteMatch.MarkName must retain internal accessors." );
		}
		var setter = mark.SetMethod!.ReturnParameter.GetRequiredCustomModifiers()
			.Any( modifier => modifier.FullName == "System.Runtime.CompilerServices.IsExternalInit" ) ? "init" : "set";
		Console.WriteLine( $"  internal property {Format( mark.PropertyType, nullable.Create( mark ) )} {mark.Name} {{ get; {setter}; }}" );
	}
	if ( pcreInternal && type.Name == "PcreByteOptions" ) {
		var limit = type.GetField( "DefaultMatchWorkLimit", BindingFlags.NonPublic | BindingFlags.Static )!;
		if ( !limit.IsAssembly || !limit.IsLiteral ) {
			throw new InvalidOperationException( "PcreByteOptions.DefaultMatchWorkLimit must retain its internal constant declaration." );
		}
		Console.WriteLine( $"  internal const {Format( limit.FieldType )} {limit.Name} = {limit.GetRawConstantValue()}" );
	}
	if ( pcreInternal && type.Name is "PcreByteOptions" or "PcreMatchPolicy" ) {
		var defaults = Activator.CreateInstance( type )!;
		foreach ( var property in type.GetProperties( BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly )
			.OrderBy( property => property.Name, StringComparer.Ordinal ) ) {
			var value = property.GetValue( defaults );
			var text = value is null ? "null" : value is bool flag ? flag ? "true" : "false"
				: Convert.ToString( value, System.Globalization.CultureInfo.InvariantCulture );
			Console.WriteLine( $"  default {property.Name} = {text}" );
		}
	}
}

string Parameters( ParameterInfo[] parameters ) => string.Join( ", ", parameters.Select( parameter => {
	var modifier = parameter.IsOut ? "out " : parameter.ParameterType.IsByRef ? parameter.IsIn ? "in " : "ref " : string.Empty;
	var optional = parameter.HasDefaultValue ? $" = {DefaultValue( parameter )}" : string.Empty;
	return $"{modifier}{Format( parameter.ParameterType, nullable.Create( parameter ) )} {parameter.Name}{optional}";
} ) );

static string DefaultValue( ParameterInfo parameter ) {
	if ( parameter.DefaultValue is null ) return parameter.ParameterType.IsValueType ? "default" : "null";
	if ( parameter.DefaultValue is DBNull || parameter.DefaultValue is Missing ) return "default";
	if ( parameter.ParameterType.IsEnum ) return $"{Format( parameter.ParameterType )}.{Enum.GetName( parameter.ParameterType, parameter.DefaultValue )}";
	return parameter.DefaultValue is bool value ? value ? "true" : "false"
		: parameter.DefaultValue is string text ? $"\"{text}\""
		: Convert.ToString( parameter.DefaultValue, System.Globalization.CultureInfo.InvariantCulture ) ?? "default";
}

static string Format( Type type, NullabilityInfo? info = null ) {
	if ( type.IsByRef ) return Format( type.GetElementType()!, info?.ElementType ?? info );
	if ( type.IsArray ) return $"{Format( type.GetElementType()!, info?.ElementType )}[]{Suffix( type, info )}";
	if ( type.IsGenericParameter ) return type.Name + Suffix( type, info );
	if ( type.IsGenericType && type.GetGenericTypeDefinition() == typeof( Nullable<> ) )
		return Format( type.GetGenericArguments()[0], info?.GenericTypeArguments.FirstOrDefault() ) + "?";
	var name = type.FullName switch {
		"System.Void" => "void", "System.Boolean" => "bool", "System.Byte" => "byte", "System.Char" => "char",
		"System.Int32" => "int", "System.Int64" => "long", "System.String" => "string", "System.Object" => "object",
		_ => ( type.FullName ?? type.Name ).Replace( '+', '.' )
	};
	if ( type.IsGenericType ) {
		name = name[..name.IndexOf( '`' )];
		name += $"<{string.Join( ", ", type.GetGenericArguments().Select( (arg, i) => Format( arg, info?.GenericTypeArguments.ElementAtOrDefault( i ) ) ) )}>";
	}
	return name + Suffix( type, info );
}

static string Suffix( Type type, NullabilityInfo? info ) => !type.IsValueType && info?.ReadState == NullabilityState.Nullable ? "?" : string.Empty;
