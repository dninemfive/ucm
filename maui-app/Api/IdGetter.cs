using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using d9.utl;

namespace d9.ucm;
public static class IdGetter
{
    public enum Type { Path = 0, Query = 1 }
    public static string? IdFor(string url, Type type, string key)
        => type switch
        {
            Type.Path => url.ItemAfter(key),
            Type.Query => url.ParseQuery().First(key),
            _ => throw new NotImplementedException()
        };
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
