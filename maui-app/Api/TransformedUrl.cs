using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d9.ucm;
public class TransformedUrl
{
    public UrlTransformerDef Def { get; private set; }
    public string Name => Def.Name;
    public UrlInfoSet InfoSet { get; private set; }
    public IReadOnlyDictionary<string, string> Urls { get; private set; }
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
        foreach ((string key, UrlPattern pattern) in def.Urls)
        {
            string? value = pattern.For(infoSet);
            if (value is null)
                return null;
            urls[key] = value;
        }
        return new(def, infoSet, urls);
    }
    public string Raw => Urls["raw"];
    public string Canonical => Urls["canonical"];
    public string Id => InfoSet["id"]!;
    public string CacheFolder => Path.Join(MauiProgram.TEMP_BASE_FOLDER, "cache", new Uri(Raw).Host);
    public string CacheFilePath => Path.Join(CacheFolder, Id);
    public ApiDef Api => Def.Api;
    public string? ApiUrl
    {
        get
        {
            _ = Urls.TryGetValue(Api.ApiUrlKey, out string? apiUrl);
            return apiUrl;
        }
    }
}