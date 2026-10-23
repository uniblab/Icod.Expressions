using System.Text;
using System.Reflection;
using Icod.CommandFramework.RegularExpressions;
using Icod.CommandFramework.Text;

var regexAssembly = typeof(GnuExtendedRegularExpressionProvider).Assembly.GetName().Name;
var decodingAssembly = typeof(TextDecodingMode).Assembly.GetName().Name;
var policyAssembly = typeof(InvalidEncodingPolicy).Assembly.GetName().Name;
var readerAssembly = typeof(TextUnitReader).Assembly.GetName().Name;
if (regexAssembly != "Icod.Expressions" || decodingAssembly != regexAssembly
    || policyAssembly != regexAssembly || readerAssembly != "Icod.CommandFramework") {
    throw new InvalidOperationException("Packaged bridge did not preserve the regex and Text assembly boundaries.");
}

var expression = new GnuExtendedRegularExpressionProvider(
    PosixCLocaleRegularExpressionCharacterClassProvider.Instance).Compile("(a|aa)").Expression
    ?? throw new InvalidOperationException("Packaged regex compilation failed.");
var result = expression.Match("zaa");
if (!result.IsSuccess || !result.IsMatch || result.Match?.Index != 1
    || result.Match.Length != 2 || result.Match.Captures[0].Value != "aa") {
    throw new InvalidOperationException("Packaged regex matching changed.");
}

using var stream = new MemoryStream(Encoding.UTF8.GetBytes("é"));
var reader = new TextUnitReader(stream, TextDecodingMode.Utf8,
    InvalidEncodingPolicy.PreserveBytes, bufferSize: 1);
var unit = reader.Read();
if (unit is null || !unit.Value.ToByteArray().AsSpan().SequenceEqual("é"u8)
    || reader.Read() is not null) {
    throw new InvalidOperationException("Packaged bridge TextUnitReader changed.");
}

if (args is not ["--old-client"]) {
    throw new ArgumentException("Expected --old-client.");
}
var oldClientPath = Path.Combine(AppContext.BaseDirectory, "OldClient.dll");
var oldClient = Assembly.LoadFrom(oldClientPath);
var references = oldClient.GetReferencedAssemblies().Select(assembly => assembly.Name).ToArray();
if (!references.Contains("Icod.CommandFramework") || references.Contains("Icod.Expressions")) {
    throw new InvalidOperationException("Old client was not compiled solely against published CommandFramework.");
}
var entryPoint = oldClient.EntryPoint ?? throw new InvalidOperationException("Old client entry point is missing.");
var exitCode = entryPoint.Invoke(null, [new[] { "--forwarded" }]);
if (exitCode is not 0) {
    throw new InvalidOperationException($"Old client returned {exitCode} through the packaged bridge.");
}

Console.WriteLine("Package-only bridge and old-package-compiled client resolved transitive Expressions identities.");
