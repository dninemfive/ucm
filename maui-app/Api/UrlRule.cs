using d9.utl;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace d9.ucm;
public class UrlRule
{    
    [JsonInclude]
    public string Domain { get; private set; }
    [JsonInclude]
    public string Type { get; private set; }
    [JsonIgnore]
    public string Name => $"{Domain} {Type}";
    [JsonInclude]
    public string MatchRegex { get; private set; }
    [JsonInclude]
    public List<InfoKeyDef> Info { get; private set; }
    [JsonInclude]
    public UrlBuilder CanonicalUrl { get; private set; }
    [JsonInclude]
    public UrlBuilder ApiUrl { get; private set; }
    [JsonInclude]
    public Dictionary<string, string> Headers { get; private set; }
    [JsonInclude]
    public ApiDef Api { get; private set; }
    [JsonConstructor]
    public UrlRule(string domain, 
                   string type, 
                   string matchRegex, 
                   List<InfoKeyDef> info,
                   UrlBuilder canonicalUrl,
                   UrlBuilder apiUrl, 
                   Dictionary<string, string> headers, 
                   ApiDef api)
    {
        Domain = domain;
        Type = type;
        MatchRegex = matchRegex;
        Info = info;
        CanonicalUrl = canonicalUrl;
        ApiUrl = apiUrl;
        Headers = headers;
        Api = api;
    }
    public bool Supports(string url)
        => Regex.IsMatch(url, MatchRegex);
    public override string ToString() => Name;
    public ApiInfoSet? InfoFor(string url) => InfoGetter.InfoFor(url, Info);
    public static IEnumerable<UrlRule> Matching(string s) => UrlRuleManager.UrlRules.Where(x => x.Supports(s));
    // todo: allow the user to decide
    private static readonly Dictionary<string, UrlRule?> _bestFor = new();

    public async Task<IEnumerable<string>?> TagsFor(UrlSet urlSet)
        => await Api.GetTagsAsync(urlSet);
    public async Task<string?> FileUrlFor(UrlSet urlSet)
        => await Api.GetFileUrlAsync(urlSet);
    [JsonIgnore]
    public string CacheFolder => Path.Join(MauiProgram.TEMP_SAVE_LOCATION, "cache", Domain);
    #region static
    public static UrlRule? BestFor(string? url)
    {
        if (url is null)
            return null;
        if (_bestFor.TryGetValue(url, out UrlRule? result))
            return result;
        IEnumerable<UrlRule> results = Matching(url);
        result = results.Any() ? results.First() : null;
        _bestFor[url] = result;
        return result;
    }
    public static string? BestCanonicalUrlFor(string url)
    {
        UrlRule? best = BestFor(url);
        ApiInfoSet? id = best?.InfoFor(url);
        return best?.CanonicalUrl.For(id);
    }
    #endregion static
}
