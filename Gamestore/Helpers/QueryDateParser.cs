using System.Globalization;
using System.Text.RegularExpressions;
using GameStore.Shared.Exceptions;

namespace Gamestore.WebAPI.Helpers;

/// <summary>
/// Parses optional date query parameters. Besides ISO dates it accepts the JavaScript Date.toString() format
/// ("Mon Nov 27 2023 00:00:00 GMT+0200 (...)") that the front-end sends for the orders history range.
/// </summary>
public static partial class QueryDateParser
{
    private const DateTimeStyles UtcStyles = DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal;

    public static DateTime? ParseStartDate(string? value)
    {
        return Parse(value);
    }

    /// <summary>
    /// Returns an exclusive upper bound: a date without time includes the whole day.
    /// </summary>
    public static DateTime? ParseEndDate(string? value)
    {
        var date = Parse(value);

        return date?.TimeOfDay == TimeSpan.Zero ? date.Value.AddDays(1) : date;
    }

    private static DateTime? Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, UtcStyles, out var date))
        {
            return date;
        }

        var match = JavaScriptDate().Match(value);

        return match.Success && DateTime.TryParseExact(match.Groups["date"].Value, "MMM d yyyy", CultureInfo.InvariantCulture, UtcStyles, out date)
            ? date
            : throw new BadRequestException($"Invalid date '{value}'.");
    }

    [GeneratedRegex(@"^\s*[A-Za-z]{3}\s+(?<date>[A-Za-z]{3}\s+\d{1,2}\s+\d{4})")]
    private static partial Regex JavaScriptDate();
}
