using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using d9.utl;

namespace d9.ucm;
public static class InfoGetter
{    
    public static ApiInfoSet? InfoFor(string url, List<InfoKeyDef> items)
    {
        Dictionary<string, string?>? result = new();
        foreach((string key, string propertyName, InfoGetterType type) in items)
        {
            if (!url.Contains(key))
                return null;
            result[propertyName] = type switch
            {
                InfoGetterType.Path => url.ItemAfter(key),
                InfoGetterType.Query => url.ParseQuery().First(key),
                _ => throw new NotImplementedException()
            };
        }
        return new(result);
    }
    private static string? ItemAfter(this string url, string key)
    {
        string[] path = new Uri(url).AbsolutePath.Split($"/", StringSplitOptions.RemoveEmptyEntries);
        for(int i = 0; i < path.Length - 1; i++)
        {
            if (path[i] == key)
                return path[i + 1];
        }
        return null;
    }
}
public enum InfoGetterType { Path = 0, Query = 1 }