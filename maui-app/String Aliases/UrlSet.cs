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
    public string? CanonUrl => UrlRule.CanonicalUrl.For(Id);
    public string? ApiUrl => UrlRule.ApiUrl.For(Id);
    public string Id { get; private set; }
    public UrlRule UrlRule { get; private set; }
    private UrlSet(string rawUrl, string id, UrlRule urlRule)
    {
        RawUrl = rawUrl;
        Id = id;
        UrlRule = urlRule;
    }
    public static UrlSet? From(string rawUrl)
    {
        UrlRule? urlRule = UrlRule.BestFor(rawUrl);
        if (urlRule is null)
            return null;
        string? id = urlRule.IdFor(rawUrl);
        if (id is null)
            return null;
        return new(rawUrl, id, urlRule);
    }
    public override string ToString() => $"UrlSet({CanonUrl}, {this.IsFullyValid()})";
}
public static class UrlSetExt
{
    public static bool IsFullyValid(this UrlSet? set) => set is not null && set.CanonUrl is not null && set.ApiUrl is not null;
}