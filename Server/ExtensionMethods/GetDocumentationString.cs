using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GERMAG.Server.ExtensionMethods;

public static partial class GetDocumentationString
{
    public static String ConvertDocumentationString (this List<string> dokumentationString, int round)
    {
        if (dokumentationString.Count == 0)
        {
            return "";
        }

        var IncreasedValue = "";
        var DecreasedValues = "";
        List<double> numbers = [];

        foreach (string str in dokumentationString)
        {
            string TrimmedString = CutRegex().Replace(bisRegex().Replace(str, "-"), ".");

            Regex IsSmallerRegex = samlerRegex();
            Regex IsLargerRegex = largerRegex();

            if (IsLargerRegex.IsMatch(TrimmedString)){
                IncreasedValue = ">";
            }

            if (IsSmallerRegex.IsMatch(TrimmedString))
            {
                DecreasedValues = "<";
            }

            MatchCollection matches = TimmRegex().Matches(TrimmedString);

            foreach (Match match in matches.Cast<Match>())
            {
                string[] rangeValues = match.Value.Split('-');
                foreach (string value in rangeValues)
                {
                    if (double.TryParse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double number))
                    {
                        numbers.Add(Math.Round(number, round));
                    }
                }
            }
        }

        if (numbers.Min() == numbers.Max())
        {
            return DecreasedValues + IncreasedValue + numbers.Max();
        }
        else
        {
            return DecreasedValues + numbers.Min() + " - " + IncreasedValue + numbers.Max();
        }
    }

    [GeneratedRegex(",")]
    private static partial Regex CutRegex();
    [GeneratedRegex("bis")]
    private static partial Regex bisRegex();
    [GeneratedRegex(@"^[^<>-]*<[^<>-]*$")]
    private static partial Regex samlerRegex();
    [GeneratedRegex(@"^[^<>-]*>[^<>-]*$")]
    private static partial Regex largerRegex();
    [GeneratedRegex(@"[+-]?\s*\d+(\.\d*)?(?:\s*-\s*[+-]?\s*\d+(\.\d*)?)?")]
    private static partial Regex TimmRegex();
}
