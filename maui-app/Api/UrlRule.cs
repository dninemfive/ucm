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
    public class UrlBuilder
    {
        [JsonInclude]
        public string Prefix { get; private set; }
        [JsonInclude]
        public string Suffix { get; private set; }
        [JsonConstructor]
        public UrlBuilder(string prefix, string suffix)
        {
            Prefix = prefix;
            Suffix = suffix;
        }
        public string For(string id) => $"{Prefix}{id}{Suffix}";
    }
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
    public UrlBuilder CanonicalUrl { get; private set; }
    [JsonInclude]
    public UrlBuilder ApiUrl { get; private set; }
    [JsonInclude]
    public Dictionary<string, string> Headers { get; private set; }
    [JsonInclude]
    public JsonApiDef Api { get; private set; }
    [JsonConstructor]
    public UrlRule(string domain, 
                   string type, 
                   string matchRegex, 
                   string idRegex, 
                   UrlBuilder canonicalUrl,
                   UrlBuilder apiUrl, 
                   Dictionary<string, string> headers, 
                   JsonApiDef api)
    {
        Utils.Log($"Creating UrlRule {domain} {type}...");
        Domain = domain;
        Type = type;
        MatchRegex = matchRegex;
        IdRegex = idRegex;
        CanonicalUrl = canonicalUrl;
        ApiUrl = apiUrl;
        Headers = headers;
        Api = api;
    }
    public bool Supports(string url)
        => Regex.IsMatch(url, MatchRegex);
    public override string ToString() => Name;
    public string IdFor(string url) => Regex.Match(url, IdRegex).Value;
    public static string? BestIdFor(string url) => BestFor(url)?.IdFor(url);
    public static IEnumerable<UrlRule> Matching(string s) => UrlRuleManager.UrlRules.Where(x => x.Supports(s));
    // todo: allow the user to decide
    private static readonly Dictionary<string, UrlRule?> _bestFor = new();

    public async Task<IEnumerable<string>?> TagsFor(string? apiUrl)
        => await Api.GetTagsAsync(apiUrl);
    public async Task<string?> FileUrlFor(string apiUrl)
        => await Api.GetFileUrlAsync(apiUrl);
    #region static
    public static UrlRule? BestFor(string s)
    {
        if (_bestFor.TryGetValue(s, out UrlRule? result))
            return result;
        IEnumerable<UrlRule> results = Matching(s);
        result = results.Any() ? results.First() : null;
        _bestFor[s] = result;
        return result;
    }
    public static async Task<string?> BestFileUrlFor(string canonicalUrl)
    {
        UrlRule? bestRule = BestFor(canonicalUrl);
        string? apiUrl = bestRule?.ApiUrl.For(canonicalUrl);
        if (bestRule is null || apiUrl is null)
            return null;
        return await bestRule.FileUrlFor(canonicalUrl);
    }

    public static async Task<ItemSource?> BestItemSourceFor(string url)
    {
        UrlRule? urlRule = BestFor(url);
        return urlRule is not null ? new(urlRule.Name, url, (await urlRule.TagsFor(url))?.ToArray() ?? Array.Empty<string>()) : null;
    }
    public static string? BestCanonicalUrlFor(string url)
        => BestFor(url)?.CanonicalUrl.For(url);
    public static string? BestApiUrlFor(string url)
        => BestFor(url)?.ApiUrl.For(url);
    #endregion static
}
