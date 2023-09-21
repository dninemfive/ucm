using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace d9.ucm;
public class UrlRule
{
    [JsonInclude]
    public string MatchRegex { get; private set; }
    [JsonInclude]
    public string IdRegex { get; private set; }
    [JsonInclude]
    public string Prefix { get; private set; }
    [JsonInclude]
    public string Suffix { get; private set; }
    [JsonConstructor]
    public UrlRule(string matchRegex, string idRegex, string prefix, string suffix)
    {
        MatchRegex = matchRegex;
        IdRegex = idRegex;
        Prefix = prefix;
        Suffix = suffix;
    }
    public bool Supports(string url)
        => Regex.IsMatch(url, MatchRegex);
    public string ApiUrl(string url)
        => $"{Prefix}{Regex.Match(url, IdRegex)}{Suffix}";
}
