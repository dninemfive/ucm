using d9.utl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d9.ucm;

public static class CompetitionManager
{
    public static IEnumerable<string> Names => CompetitionsByName.Keys;
    public static IEnumerable<Competition> Competitions => CompetitionsByName.Values;
    private static Dictionary<string, Competition>? _competitionsByName = null;
    public static IReadOnlyDictionary<string, Competition> CompetitionsByName
    {
        get
        {
            if (_competitionsByName is null)
                throw new InvalidOperationException("Can't get competitions before loading them!");
            return _competitionsByName!;
        }
    }
    private static bool _loaded = false;
    public static void Load()
    {
        if (_loaded)
            return;
        Utils.Log("Loading competitions...");
        _loaded = true;
        _competitionsByName = new();
        foreach (Competition c in MauiProgram.TEMP_COMP_LOCATION.LoadAll<Competition>())
        {
            _competitionsByName[c.Name] = c;
        }
        Utils.Log($"Loaded {_competitionsByName.Count} items.");
    }
}
