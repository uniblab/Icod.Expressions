using System;
using System.IO;
using Icod.CommandFramework.Diagnostics;
using Icod.CommandFramework.RegularExpressions;
using Icod.CommandFramework.Text;

var provider = typeof(GnuBasicRegularExpressionProvider).Assembly;
var framework = typeof(CommandContext).Assembly;
var textReader = typeof(TextUnitReader).Assembly;
var decodingMode = typeof(TextDecodingMode).Assembly;
var invalidEncodingPolicy = typeof(InvalidEncodingPolicy).Assembly;
var expectedPath = Path.Combine(AppContext.BaseDirectory, "Icod.Expressions.dll");
if (provider.GetName().Name != "Icod.Expressions"
    || framework.GetName().Name != "Icod.CommandFramework"
    || textReader != framework
    || decodingMode != provider
    || invalidEncodingPolicy != provider
    || provider.Location != expectedPath) {
    Console.Error.WriteLine($"Unexpected assembly origins: regex={provider.Location}; framework={framework.Location}; textReader={textReader.Location}; decodingMode={decodingMode.Location}; invalidEncodingPolicy={invalidEncodingPolicy.Location}");
    return 1;
}
Console.WriteLine("Consumer bridge regex and shared Text identities verified.");
return 0;
