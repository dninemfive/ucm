using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using d9.utl;

namespace d9.ucm;
public static class UrlRuleManager
{
    public static IEnumerable<UrlRule> UrlRules => _byName.Values;
    public static IReadOnlyDictionary<string, UrlRule> ByName => _byName;
    private static readonly Dictionary<string, UrlRule> _byName = new();
    static UrlRuleManager()
    {
        foreach(UrlRule urlRule in MauiProgram.TEMP_RULE_LOCATION.LoadAll<UrlRule>())
        {
            if (_byName.ContainsKey(urlRule.Name))
                Utils.Log($"Duplicate urlRules with name {urlRule.Name}!");
            _byName[urlRule.Name] = urlRule;
        }
        Utils.Log($"Loaded UrlRules:");
        foreach ((string name, UrlRule urlRule) in _byName.OrderBy(x => x.Key))
            Utils.Log($"\t{name,-16}\t{urlRule}");
    }
}
