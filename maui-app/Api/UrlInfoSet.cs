using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace d9.ucm;
/// <summary>
/// A set of key-value pairs representing the information (e.g. artist name, id) which can be gained from a URL alone.
/// </summary>
public class UrlInfoSet : IEnumerable<KeyValuePair<string, string?>>
{
    private readonly Dictionary<string, string?> _dict = new();
    private UrlInfoSet(Dictionary<string, string?> dict)
    {
        _dict = dict;
    }
    public static UrlInfoSet? For(string url, List<InfoKeyDef> infoKeys)
    {
        Dictionary<string, string?>? result = new();
        foreach ((string key, string propertyName, InfoGetterType type) in infoKeys)
        {
            if (!url.Contains(key))
                return null;
            result[propertyName] = type switch
            {
                InfoGetterType.Path => ItemAfter(url, key),
                InfoGetterType.Query => url.ParseQuery().First(key),
                _ => throw new NotImplementedException()
            };
        }
        return new(result);
    }
    public static UrlInfoSet? For(string url, Dictionary<string, string> infoKeys)
    {
        Dictionary<string, string?> result = new();
        foreach((string key, string matchRegex) in infoKeys)
        {
            MatchCollection matches = Regex.Matches(url, matchRegex);
            if (!matches.Any())
                return null;
            if(matches.First().Groups.Count > 1)
            {
                result[key] = matches.First().Groups.Values.ElementAt(1).Value;
            } 
            else
            {
                result[key] = matches.First().Groups.Values.First().Value;
            }
        }
        return new(result);
    }
    private static string? ItemAfter(string url, string key)
    {
        string[] path = new Uri(url).AbsolutePath.Split($"/", StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < path.Length - 1; i++)
        {
            if (path[i] == key)
                return path[i + 1];
        }
        return null;
    }
    public string? this[string key] => _dict[key];

    public IEnumerator<KeyValuePair<string, string?>> GetEnumerator()
        => _dict.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => _dict.GetEnumerator();
}
public enum InfoGetterType { Path = 0, Query = 1 }