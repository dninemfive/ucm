using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace d9.ucm;
public class ApiDef2
{
    [JsonInclude]
    public string Domain { get; private set; }
    [JsonInclude]
    public string Name { get; private set; }
    [JsonInclude]
    public string MatchRegex { get; private set; }
    // gets info from the url, e.g. a post id after a specific string
    public Dictionary<string, string> ItemInfo;
    // patterns which generate useful URLs, such as API endpoints or canonical URLs
    public Dictionary<string, UrlPattern> Urls;
    // headers to include when making requests to this API
    public Dictionary<string, string> Headers;
    public bool Matches(string url) => Regex.IsMatch(url, MatchRegex);
    [JsonIgnore]
    public static IEnumerable<ApiDef2> List;
}
public static class ApiDefs
{
    public static IEnumerable<ApiDef2> Matching(string url)
        => ApiDef2.List.Where(x => x.Matches(url));
}
public class ApiSummary
{
    public ApiDef2 Def { get; private set; }
    public string Name => $"{Def.Domain} {Def.Name}";
    public ApiInfoSet InfoSet;
    public IReadOnlyDictionary<string, string> Urls;
    private ApiSummary(ApiDef2 def, ApiInfoSet infoSet, Dictionary<string, string> urls)
    {
        Def = def;
        InfoSet = infoSet;
        Urls = urls;
    }
    public static ApiSummary? For(string url, ApiDef2 def)
    {
        if (!def.Matches(url))
            return null;
        ApiInfoSet? infoSet = ApiInfoSet.For(url, def.ItemInfo);
        if (infoSet is null)
            return null;
        Dictionary<string, string> urls = new();
        foreach((string key, UrlPattern pattern) in def.Urls)
        {
            string? value = pattern.For(infoSet);
            if (value is null)
                return null;
            urls[key] = value;
        }
        return new(def, infoSet, urls);
    }
}