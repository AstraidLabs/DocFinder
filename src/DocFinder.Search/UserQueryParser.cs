using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using DocFinder.Domain;

namespace DocFinder.Search;

public static class UserQueryParser
{
    private static readonly Regex _tokenRegex = new(@"(?<key>\w+):(?<value>""[^""]+""|\S+)", RegexOptions.Compiled);

    public static UserQuery Parse(string input)
    {
        var filters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        DateTimeOffset? from = null;
        DateTimeOffset? to = null;
        var sb = new StringBuilder();
        var lastIndex = 0;

        foreach (Match match in _tokenRegex.Matches(input))
        {
            var key = match.Groups["key"].Value;
            var raw = match.Groups["value"].Value;
            var value = raw.Trim('"');

            switch (key.ToLowerInvariant())
            {
                case "from":
                    if (DateTimeOffset.TryParse(value, out var f)) from = f.ToUniversalTime();
                    break;
                case "to":
                    if (DateTimeOffset.TryParse(value, out var t)) to = t.ToUniversalTime();
                    break;
                default:
                    filters[key] = value;
                    break;
            }

            sb.Append(input, lastIndex, match.Index - lastIndex);
            lastIndex = match.Index + match.Length;
        }

        sb.Append(input, lastIndex, input.Length - lastIndex);
        var free = sb.ToString().Trim();

        return new UserQuery(free)
        {
            Filters = filters,
            FromUtc = from,
            ToUtc = to
        };
    }
}
