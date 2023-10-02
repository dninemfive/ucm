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
public class UrlHandler
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
    public string IdRegex { get; private set; }
    [JsonInclude]
    public string Prefix { get; private set; }
    [JsonInclude]
    public string Suffix { get; private set; }
    [JsonInclude]
    public Dictionary<string, string> Headers { get; private set; }
    [JsonInclude]
    public JsonApiDef Api { get; private set; }
    [JsonConstructor]
    public UrlHandler(string domain, 
                   string type, 
                   string matchRegex, 
                   string idRegex, 
                   string prefix, 
                   string suffix, 
                   Dictionary<string, string> headers, 
                   JsonApiDef api)
    {
        Utils.Log($"Creating UrlRule {domain} {type}...");
        Domain = domain;
        Type = type;
        MatchRegex = matchRegex;
        IdRegex = idRegex;
        Prefix = prefix;
        Suffix = suffix;
        Headers = headers;
        Api = api;
    }
    public bool Supports(string url)
        => Regex.IsMatch(url, MatchRegex);
    public string UrlFor(string url)
    {
        string result = $"{Prefix}{IdFor(url)}{Suffix}";
        return result;
    }    
    public string IdFor(string url) => Regex.Match(url, IdRegex).Value;
    public static string? BestIdFor(string url) => BestFor(url)?.IdFor(url);
    public static IEnumerable<UrlHandler> Matching(string s) => UrlRuleManager.UrlRules.Where(x => x.Supports(s));
    // todo: allow the user to decide
    private static readonly Dictionary<string, UrlHandler?> _bestFor = new();

    public async Task<IEnumerable<string>?> TagsFor(string resourceUrl)
        => await Api.GetTagsAsync(resourceUrl);
    public async Task<string?> FileUrlFor(string url)
        => await Api.GetFileUrlAsync(url);
    #region static
    public static UrlHandler? BestFor(string s)
    {
        if (_bestFor.TryGetValue(s, out UrlHandler? result))
            return result;
        IEnumerable<UrlHandler> results = Matching(s);
        result = results.Any() ? results.First() : null;
        _bestFor[s] = result;
        return result;
    }
    public static async Task<string?> BestFileUrlFor(string url)
        => await (BestFor(url)?.FileUrlFor(url) ?? Task.FromResult<string?>(null));

    public static async Task<ItemSource?> BestItemSourceFor(string url)
    {
        UrlHandler? urlRule = BestFor(url);
        return urlRule is not null ? new(urlRule.Name, url, (await urlRule.TagsFor(url))?.ToArray() ?? Array.Empty<string>()) : null;
    }
    public static string? BestUrlFor(string url)
        => BestFor(url)?.UrlFor(url);
    #endregion static
}
