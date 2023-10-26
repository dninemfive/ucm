using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace d9.ucm;
public class UrlSet
{
    public string RawUrl { get; private set; }
    public string? CanonUrl => UrlRule.CanonicalUrl.For(Info);
    public string? ApiUrl => UrlRule.ApiUrl.For(Info);
    public ApiInfoSet Info { get; private set; }
    public UrlRule UrlRule { get; private set; }
    private UrlSet(string rawUrl, ApiInfoSet info, UrlRule urlRule)
    {
        RawUrl = rawUrl;
        Info = info;
        UrlRule = urlRule;
    }
    public static UrlSet? From(string rawUrl)
    {
        UrlRule? urlRule = UrlRule.BestFor(rawUrl);
        if (urlRule is null)
            return null;
        ApiInfoSet? info = urlRule.InfoFor(rawUrl);
        if (info is null)
            return null;
        return new(rawUrl, info, urlRule);
    }
    public override string ToString() => $"UrlSet({CanonUrl}, {this.IsFullyValid()})";
    public string CacheFolder => UrlRule.CacheFolder;
    public string Id => Info["id"]!;
}
public static class UrlSetExt
{
    public static bool IsFullyValid(this UrlSet? set) => set is not null && set.CanonUrl is not null && set.ApiUrl is not null;
    public static string LocalPath(this string url) => new Uri(url).LocalPath;
}