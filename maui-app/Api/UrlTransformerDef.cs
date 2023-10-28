using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace d9.ucm;
// todo: yell at the user if they don't define certain properties, such as:
// - ItemInfo: "id"
// - Urls: "canonical"
public class UrlTransformerDef
{
    [JsonInclude]
    public string Domain { get; private set; }
    [JsonInclude]
    public string Type { get; private set; }
    [JsonIgnore]
    public string Name => $"{Domain} {Type}";
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
    [JsonInclude]
    public Dictionary<string, string> ApiArgs { get; private set; }
    [JsonIgnore]
    public ApiDef Api { get; private set; }
    [JsonConstructor]
    public UrlTransformerDef(string domain,
                             string type,
                             string matchRegex,
                             Dictionary<string, string> itemInfo,
                             Dictionary<string, UrlPattern> urls,
                             Dictionary<string, string> headers,
                             Dictionary<string, string> apiArgs)
    {
        Domain = domain;
        Type = type;
        MatchRegex = matchRegex;
        ItemInfo = itemInfo;
        Urls = urls;
        Headers = headers;
        ApiArgs = apiArgs;
        Type apiType = ApiDef.Types[apiArgs["Type"]];
        ApiDef? apiDef = (ApiDef?)(apiType.GetConstructor(new Type[] { typeof(Dictionary<string, string>) })?.Invoke(new object?[] { apiArgs }));
        if (apiDef is null) Utils.Log($"Failed to get Api using args {apiArgs}");
        Api = apiDef!;
    }
    public bool Matches(string url) => Regex.IsMatch(url, MatchRegex);
    [JsonIgnore]
    public static IEnumerable<UrlTransformerDef> List => UrlTransformerDefs.ByName.Values;
    public static bool operator ==(UrlTransformerDef? a, UrlTransformerDef? b) => a?.Domain == b?.Domain && a?.Type == b?.Type;
    public static bool operator !=(UrlTransformerDef a, UrlTransformerDef b) => !(a == b);
    public override bool Equals(object? obj)
        => obj is UrlTransformerDef def && this == def;
    public override int GetHashCode()
        => HashCode.Combine(Domain.GetHashCode(), Type.GetHashCode());
}