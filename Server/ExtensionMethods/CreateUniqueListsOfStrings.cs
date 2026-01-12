using System.Globalization;
using System.Text.RegularExpressions;

namespace GERMAG.Server.ExtensionMethods;

public static partial class GetDocumentationString
{
    public static List<String> ToUniqueStringList(this List<string> dokumentationString)
    {
        List<string> distinctList = dokumentationString.Distinct().ToList();

        return distinctList;
    }
}
