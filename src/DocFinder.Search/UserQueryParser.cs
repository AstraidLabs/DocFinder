using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DocFinder.Domain;

namespace DocFinder.Search;

public static class UserQueryParser
{
    private static readonly Regex _tokenRegex = new(@"(?<key>\w+):(?<value>""[^""]+""|\S+)", RegexOptions.Compiled);

    public static UserQuery Parse(string input)
    {
        var filters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        DateTime? from = null;
        DateTime? to = null;
        var text = input;

        foreach (Match match in _tokenRegex.Matches(input))
        {
            var key = match.Groups["key"].Value;
            var raw = match.Groups["value"].Value;
            var value = raw.Trim('"');

            switch (key.ToLowerInvariant())
            {
                case "from":
                    if (DateTime.TryParse(value, out var f)) from = f;
                    break;
                case "to":
                    if (DateTime.TryParse(value, out var t)) to = t;
                    break;
                default:
                    filters[key] = value;
                    break;
            }

            text = text.Replace(match.Value, string.Empty);
        }

        var free = text.Trim();
        return new UserQuery(free, false, filters, from, to);
    }
}
