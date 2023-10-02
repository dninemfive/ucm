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
    public static string? BestUrlFor(string url)
        => BestFor(url)?.UrlFor(url);
    public HttpRequestMessage RequestMessageFor(string url, HttpMethod? method = null)
    {
        if(!Supports(url))
        {
            throw new ArgumentException($"UrlRule {Name} does not match url {url}!", nameof(url));
        }
        Utils.Log($"`{Name}`.RequestMessageFor({url}, {method.PrintNull()})");
        HttpRequestMessage result = new(method ?? HttpMethod.Get, UrlFor(url));
        foreach((string key, string value) in Headers.OrderBy(x => x.Key))
        {
            result.Headers.Add(key, value);
        }
        return result;
    }    
    public string IdFor(string url) => Regex.Match(url, IdRegex).Value;
    public static string? BestIdFor(string url) => BestFor(url)?.IdFor(url);
    public static IEnumerable<UrlHandler> Matching(string s) => UrlRuleManager.UrlRules.Where(x => x.Supports(s));
    // todo: allow the user to decide
    private static readonly Dictionary<string, UrlHandler?> _bestFor = new();
    public static UrlHandler? BestFor(string s)
    {
        UrlHandler? result;
        if (_bestFor.TryGetValue(s, out result))
            return result;
        IEnumerable<UrlHandler> results = Matching(s);
        result = results.Any() ? results.First() : null;
        _bestFor[s] = result;
        return result;
    }
    public IEnumerable<Tag> TagsFor(string url)
    {
        // todo: implement
        yield break;
    }
    public static ItemSource? ItemSourceFor(string url)
    {
        UrlHandler? urlRule = BestFor(url);
        return urlRule is not null ? new(urlRule.Name, url, urlRule.TagsFor(url).ToArray()) : null;
    }
    public async Task<string?> FileUrlFor(string url)
        => await new JsonApiHandler().FileUrlAsync(url);
    public static async Task<string?> BestFileUrlFor(string url)
        => await (BestFor(url)?.FileUrlFor(url) ?? Task.FromResult<string?>(null));
}
