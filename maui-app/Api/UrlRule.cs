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
    public abstract class UrlBuilder<T, U>
        where U : StringAlias
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
        public abstract U For(T url);
    }
    public class PageUrlBuilder : UrlBuilder<string, PageUrl>
    {
        public PageUrlBuilder(string prefix, string suffix) : base(prefix, suffix) { }
        public override PageUrl For(string url)
            => new($"{Prefix}{BestIdFor(url)}{Suffix}");
    }
    public class ApiUrlBuilder : UrlBuilder<PageUrl, ApiUrl>
    {
        public ApiUrlBuilder(string prefix, string suffix) : base(prefix, suffix) { }
        public override ApiUrl For(PageUrl url)
            => new($"{Prefix}{BestIdFor(url)}{Suffix}");
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
    public PageUrlBuilder CanonicalUrl { get; private set; }
    [JsonInclude]
    public ApiUrlBuilder ApiUrl { get; private set; }
    [JsonInclude]
    public Dictionary<string, string> Headers { get; private set; }
    [JsonInclude]
    public JsonApiDef Api { get; private set; }
    [JsonConstructor]
    public UrlRule(string domain, 
                   string type, 
                   string matchRegex, 
                   string idRegex, 
                   PageUrlBuilder canonicalUrl,
                   ApiUrlBuilder apiUrl, 
                   Dictionary<string, string> headers, 
                   JsonApiDef api)
    {
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
    public string IdFor(PageUrl url) => Regex.Match(url.Value, IdRegex).Value;
    public static string? BestIdFor(string url) => BestFor(url)?.IdFor(url);
    public static string? BestIdFor(PageUrl url) => BestFor(url)?.IdFor(url);
    public static IEnumerable<UrlRule> Matching(string s) => UrlRuleManager.UrlRules.Where(x => x.Supports(s));
    // todo: allow the user to decide
    private static readonly Dictionary<string, UrlRule?> _bestFor = new();

    public async Task<IEnumerable<string>?> TagsFor(ApiUrl? apiUrl)
        => await Api.GetTagsAsync(apiUrl);
    public async Task<string?> FileUrlFor(ApiUrl apiUrl)
        => await Api.GetFileUrlAsync(apiUrl);
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
    public static UrlRule? BestFor(PageUrl? url) => BestFor(url?.Value);
    public static async Task<string?> BestFileUrlFor(PageUrl url)
    {
        UrlRule? bestRule = BestFor(url);
        ApiUrl? apiUrl = bestRule?.ApiUrl.For(url);
        if (bestRule is null || apiUrl is null)
            return null;
        return await bestRule.FileUrlFor(apiUrl);
    }

    public static async Task<ItemSource?> BestItemSourceFor(string url)
    {
        UrlRule? urlRule = BestFor(url);
        return urlRule is not null ? new(urlRule.Name, url, (await urlRule.TagsFor(new ucm.ApiUrl(url)))?.ToArray() ?? Array.Empty<string>()) : null;
    }
    public static PageUrl? BestCanonicalUrlFor(string url)
    {
        UrlRule? best = BestFor(url);
        return best?.CanonicalUrl.For(url);
    }
    //    => BestFor(url)?.CanonicalUrl.For(url);
    public static ApiUrl? BestApiUrlFor(PageUrl url)
        => BestFor(url)?.ApiUrl.For(url);
    #endregion static
}
