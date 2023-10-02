using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using d9.utl;

namespace d9.ucm;
public static class UrlRuleManager
{
    public static IEnumerable<UrlHandler> UrlRules => _byName.Values;
    public static IReadOnlyDictionary<string, UrlHandler> ByName => _byName;
    private static readonly Dictionary<string, UrlHandler> _byName = new();
    static UrlRuleManager()
    {
        foreach(UrlHandler urlRule in MauiProgram.TEMP_RULE_LOCATION.LoadAll<UrlHandler>())
        {
            if (_byName.ContainsKey(urlRule.Name))
                Utils.Log($"Duplicate urlRules with name {urlRule.Name}!");
            _byName[urlRule.Name] = urlRule;
        }
        Utils.Log($"Loaded UrlRules:");
        foreach (string name in UrlRules.Select(x => x.Name).Order())
            Utils.Log($"\t{name,-16}");
    }
}
