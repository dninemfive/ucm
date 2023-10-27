using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace d9.ucm;
public class UrlTransformerDef
{
    [JsonInclude]
    public string Domain { get; private set; }
    [JsonInclude]
    public string Name { get; private set; }
    [JsonInclude]
    public string MatchRegex { get; private set; }
    // gets info from the url, e.g. a post id after a specific string
    [JsonInclude]
    public Dictionary<string, string> ItemInfo { get; private set; }
    // patterns which generate useful URLs, such as API endpoints or canonical URLs
    [JsonInclude]
    public Dictionary<string, UrlPattern> Urls { get; private set; }
    // headers to include when making requests to this API
    [JsonInclude]
    public Dictionary<string, string> Headers { get; private set; }
    [JsonConstructor]
    public UrlTransformerDef(string domain,
                             string name,
                             string matchRegex,
                             Dictionary<string, string> itemInfo,
                             Dictionary<string, UrlPattern> urls,
                             Dictionary<string, string> headers)
    {
        Domain = domain;
        Name = name;
        MatchRegex = matchRegex;
        ItemInfo = itemInfo;
        Urls = urls;
        Headers = headers;
    }
    public bool Matches(string url) => Regex.IsMatch(url, MatchRegex);
    [JsonIgnore]
    public static IEnumerable<UrlTransformerDef> List => UrlTransformerDefs.ByName.Values;
    public static bool operator ==(UrlTransformerDef? a, UrlTransformerDef? b) => a?.Domain == b?.Domain && a?.Name == b?.Name;
    public static bool operator !=(UrlTransformerDef a, UrlTransformerDef b) => !(a == b);
    public override bool Equals(object? obj)
        => obj is UrlTransformerDef def && this == def;
    public override int GetHashCode()
        => HashCode.Combine(Domain.GetHashCode(), Name.GetHashCode());
}