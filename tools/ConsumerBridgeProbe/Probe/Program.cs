using System;
using System.IO;
using Icod.CommandFramework.Diagnostics;
using Icod.CommandFramework.RegularExpressions;

var provider = typeof(GnuBasicRegularExpressionProvider).Assembly;
var framework = typeof(CommandContext).Assembly;
var expectedPath = Path.Combine(AppContext.BaseDirectory, "Icod.Expressions.dll");
if (provider.GetName().Name != "Icod.Expressions"
    || framework.GetName().Name != "Icod.CommandFramework"
    || provider.Location != expectedPath) {
    Console.Error.WriteLine($"Unexpected assembly origins: regex={provider.Location}; framework={framework.Location}");
    return 1;
}
Console.WriteLine("CoreUtils expr bridge assembly identities verified.");
return 0;
