using System.Text.RegularExpressions;

namespace GERMAG.Server.ExtensionMethods;

public static partial class GetEPSGToInt
{
    public static int EPSGToInt (this string ESPGString)
    {
        var espgStringRegex = ESPGRegexNumber().Match(ESPGRegexShrink().Replace(ESPGString ?? "0", ":"));

        var espgString = espgStringRegex.Value;

        return int.Parse(espgString);
    }

    [GeneratedRegex("::")]
    private static partial Regex ESPGRegexShrink();

    [GeneratedRegex("(\\d+)")]
    private static partial Regex ESPGRegexNumber();
}
