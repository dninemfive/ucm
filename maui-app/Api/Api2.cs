using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace d9.ucm;

public static class UrlTransformerDefs
{
    public static IReadOnlyDictionary<string, UrlTransformerDef> ByName => _byName;
    private static readonly Dictionary<string, UrlTransformerDef> _byName = new();
    static UrlTransformerDefs()
    {
        foreach (UrlTransformerDef def in MauiProgram.TEMP_RULE_LOCATION.LoadAll<UrlTransformerDef>())
        {
            if (_byName.ContainsKey(def.Name))
                Utils.Log($"Duplicate ApiDefs with name {def.Name}!");
            _byName[def.Name] = def;
        }
        Utils.Log($"Loaded UrlRules:");
        foreach (string name in UrlTransformerDef.List.Select(x => x.Name).Order())
            Utils.Log($"\t{name,-16}");
    }
    // todo: change this to indexing each ApiDef by the uri's domain. there should be one and only one valid ApiDef per url.
    //       the multiple results portion will be AcquisitionHandlers or smth which will handle figuring out whether it's 
    //       a music file or a video or whatever
    public static IEnumerable<UrlTransformerDef> Matching(string url)
        => UrlTransformerDef.List.Where(x => x.Matches(url)).OrderBy(x => $"{x.Domain} {x.Name}");
    public static UrlTransformerDef? FirstMatching(string url)
    {
        List<UrlTransformerDef> matches = Matching(url).ToList();
        if (matches.Any())
            return matches.First();
        return null;
    }
}
public class TransformedUrl
{
    public UrlTransformerDef Def { get; private set; }
    public string Name => $"{Def.Domain} {Def.Name}";
    public UrlInfoSet InfoSet;
    public IReadOnlyDictionary<string, string> Urls;
    private TransformedUrl(UrlTransformerDef def, UrlInfoSet infoSet, Dictionary<string, string> urls)
    {
        Def = def;
        InfoSet = infoSet;
        Urls = urls;
    }
    public static TransformedUrl? For(string url, UrlTransformerDef? def = null)
    {
        def ??= UrlTransformerDefs.FirstMatching(url);
        if (def is null || !def.Matches(url))
            return null;
        UrlInfoSet? infoSet = UrlInfoSet.For(url, def.ItemInfo);
        if (infoSet is null)
            return null;
        Dictionary<string, string> urls = new()
        {
            { "raw", url }
        };
        foreach((string key, UrlPattern pattern) in def.Urls)
        {
            string? value = pattern.For(infoSet);
            if (value is null)
                return null;
            urls[key] = value;
        }
        return new(def, infoSet, urls);
    }
    public string CanonicalUrl => Urls["canonical"];
}