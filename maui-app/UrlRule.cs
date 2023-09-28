using d9.utl;
using Microsoft.Extensions.Primitives;
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
    public string Name { get; private set; }
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
    [JsonConstructor]
    public UrlRule(string name, string matchRegex, string idRegex, string prefix, string suffix, Dictionary<string, string> headers)
    {
        Name = name;
        MatchRegex = matchRegex;
        IdRegex = idRegex;
        Prefix = prefix;
        Suffix = suffix;
        Headers = headers;
    }
    public bool Supports(string url)
        => Regex.IsMatch(url, MatchRegex);
    public string UrlFor(string url) 
        => $"{Prefix}{Regex.Match(url, IdRegex)}{Suffix}";
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
    public PendingItem PendingItemFor(string url)
    {
        throw new NotImplementedException();
    }
}
