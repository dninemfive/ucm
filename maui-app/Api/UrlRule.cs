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
        public string? For(string? id) => id is null ? null : $"{Prefix}{id}{Suffix}";
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
    public string IdKey { get; private set; }
    [JsonInclude]
    public int IdType { get; private set; }
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
                   string idKey,
                   int idType,
                   UrlBuilder canonicalUrl,
                   UrlBuilder apiUrl, 
                   Dictionary<string, string> headers, 
                   JsonApiDef api)
    {
        Domain = domain;
        Type = type;
        MatchRegex = matchRegex;
        IdKey = idKey;
        IdType = idType;
        CanonicalUrl = canonicalUrl;
        ApiUrl = apiUrl;
        Headers = headers;
        Api = api;
    }
    public bool Supports(string url)
        => Regex.IsMatch(url, MatchRegex);
    public override string ToString() => Name;
    public string? IdFor(string url) => IdGetter.IdFor(url, (IdGetter.Type)IdType, IdKey);
    public static string? BestIdFor(string url) => BestFor(url)?.IdFor(url);
    public static IEnumerable<UrlRule> Matching(string s) => UrlRuleManager.UrlRules.Where(x => x.Supports(s));
    // todo: allow the user to decide
    private static readonly Dictionary<string, UrlRule?> _bestFor = new();

    public async Task<IEnumerable<string>?> TagsFor(UrlSet urlSet)
        => await Api.GetTagsAsync(urlSet);
    public async Task<string?> FileUrlFor(UrlSet urlSet)
        => await Api.GetFileUrlAsync(urlSet);
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
        string? id = best?.IdFor(url);
        return best?.CanonicalUrl.For(id);
    }
    #endregion static
}
