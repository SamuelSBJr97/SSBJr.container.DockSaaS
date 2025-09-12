using System.Globalization;

namespace SSBJr.DockSaaS.Web.Extensions;

public static class StringExtensions
{
    public static string ToTitleCase(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        var textInfo = CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(input.ToLower());
    }

    public static string TruncateWithEllipsis(this string input, int maxLength)
    {
        if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
            return input;

        return input[..maxLength] + "...";
    }

    public static string FormatCamelCase(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        return System.Text.RegularExpressions.Regex.Replace(input, "(\\B[A-Z])", " $1");
    }
}