using d9.utl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace d9.ucm;
public static class Extensions
{
    public static string? FileHash(this string path) => Hash(File.ReadAllBytes(path));
    public static async Task<string?> FileHashAsync(this string path) => await Task.Run(path.FileHash);
    public static string? Hash(this byte[] bytes) => JsonSerializer.Serialize(SHA512.HashData(bytes)).Replace("\"", "");
    public static async Task<string?> HashAsync(this byte[] bytes) => await Task.Run(bytes.Hash);
    public static string FileExtension(this string path)
    {
        if(path.Contains("http"))
        {
            foreach (char delim in "#?")
                path = path.Split(delim)[0];
        }
        return Path.GetExtension(path).ToLower();
    }
    // need list of supported extensions again smh (not available afaik)
    // private static readonly Dictionary<string, Func<string, View>> _viewGenerators = new()
    public static View? BestAvailableView(this string path) => path.FileExtension() switch
    {
        // https://devblogs.microsoft.com/dotnet/announcing-dotnet-maui-communitytoolkit-mediaelement/
        ".mov" or ".mp4" or ".webm" => null,
        // ".json" or ".txt" => new Label() { Text = File.ReadAllText(path) },
        ".xcf" or ".pdf" or ".zip" => null,
        _ => new Image() { Source = path, IsAnimationPlaying = true, Aspect = Aspect.AspectFit }
    };
    public static bool ExtensionIsSupported(this string path)
        => path.FileExtension() is not (".mov" or ".mp4" or ".webm" or ".xcf" or ".pdf" or ".zip");
    public static double StandardDeviation(this IEnumerable<double> vals)
    {
        if (!vals.Any())
            return 0;
        double sum = 0;
        double mu = vals.Average();
        foreach (double val in vals)
            sum += Math.Pow(val - mu, 2);
        return Math.Sqrt(sum / (vals.Count() - 1));
    }
    public static double HalfIntervalSize(double z, double s, int n) => z * (s / Math.Sqrt(n));
    public static (double lower, double upper) ConfidenceInterval(double x_bar, double z, double s, int n) =>
        (x_bar - HalfIntervalSize(z, s, n), x_bar + HalfIntervalSize(z, s, n));

    public static string ReplaceAny(this string str, params string[] tokens)
    {
        if (tokens.Length < 2)
            throw new Exception();
        string last = tokens.Last();
        foreach (string token in tokens.SkipLast(1))
            str = str.Replace(token, last);
        return str;
    }
    public static string? DirectoryName(this string? path) => Path.GetDirectoryName(path);
    public static string? FileName(this string? path) => Path.GetFileName(path);
    // https://stackoverflow.com/a/76251267
    public static double ScrollSpace(this ScrollView sv)
        => sv.ContentSize.Height - sv.Height;
    public static IEnumerable<T> LoadAll<T>(this string srcFolder, Func<(string path, T item), bool>? validator = null)
    {
        int ct = 0;
        foreach (string path in Directory.EnumerateFiles(srcFolder))
        {
            if (!(path.FileExtension() == ".json" || (path.FileExtension() == ".secret" && path.Contains(".json.secret"))))
                continue;
            T? item = default;
            try 
            {
                item = JsonSerializer.Deserialize<T>(File.ReadAllText(path));                
            } 
            catch(Exception e)
            {
                Utils.Log($"Error loading {typeof(T).Name} from `{path}`:\t\n{e.GetType().Name}: {e.Message}");
            }
            if (item is not null && (validator is null || validator((path, item))))
            {
                ct++;
                yield return item;
            }

        }
        Utils.Log($"Loaded {ct} {typeof(T).Name}s from {srcFolder}.");
    }
    public static Competition.Rating? RatingAs(this Item item, string? name)
        => Competition.Named(name)?.RatingOf(item);
    public static string? UriScheme(this string? location)
        => Uri.TryCreate(location, UriKind.Absolute, out Uri? uri) ? uri?.Scheme : null;
    public static Type? ToType(this string typeName) => Type.GetType(typeName)
                                                     ?? Type.GetType($"System.{typeName}")
                                                     ?? Type.GetType($"d9.ucm.{typeName}");
    public static string AsBulletedList(this IEnumerable<object> objects, string sep = "\n- ") => objects.Count() switch
    {
        < 0 => throw new Exception("wtf how"),
        0 => "",
        1 => $"{sep}{objects.First()}",
        _ => $"{sep}{objects.Aggregate((x, y) => $"{x}{sep}{y}")}"
    };
    public static async Task<byte[]?> GetBytesAsync(this string location, LocationType locationType)
    {
        try
        {
            return locationType switch
            {
                LocationType.Url => await MauiProgram.HttpClient.GetByteArrayAsync(location),
                LocationType.Path => await File.ReadAllBytesAsync(location),
                _ => throw new InvalidOperationException($"Attempted to get bytes for location {location} with unrecognized location type {locationType}.")
            };
        }
        catch (Exception e)
        {
            Utils.Log($"{e.GetType().Name} when attempting to get bytes for {location}: {e.Message}");
            return null;
        }
    }
    public static IEnumerable<(int a, int b)> Factors(this int z)
    {
        Utils.Log($"Factor pairs of {z}:");
        double sqrt = Math.Sqrt(z);
        for (int a = 1; a < sqrt; a++)
        {
            int b = z / a;
            if (a * b == z)
            {
                Utils.Log($"{a}, {b}");
                yield return (a, b);
            }
        }
    }
    public static string TagNormalize(this string str) => str.ToLower().Trim();
    public static IEnumerable<SearchToken> Tokenize(this string str) => SearchToken.Tokenize(str);
    public static async Task SavePageTo(this string url, string path) 
        => await File.WriteAllTextAsync(path, 
                                        await MauiProgram.HttpClient.GetStringAsync(url));
    public static string? ItemBetween(this string str, string start, string end)
    {
        string[] split = str.Split(start);
        if (split.Length < 2)
            return null;
        string[] split2 = split[1].Split(end);
        if(!split2.Any())
            return null;
        return split2.First();
    }
    private static (string, string)[] unicodeEscapeSequences = new (string, string)[]
    {
        ("002F", "/"),
        ("003C", "<"),
        ("003E", ">")
    };
    public static string Unescape(this string s)
    {
        foreach ((string seq, string replacement) in unicodeEscapeSequences) {
            s = s.Replace($"\\\\u{seq}", replacement);
        }
        return s.Replace("\\", "");
    }
    public static Dictionary<K, V> ToDictionary<K, V>(this IEnumerable<(K k, V v)> kvps)
        where K : notnull
    {
        Dictionary<K, V> result = new();
        foreach ((K k, V v) in kvps)
            result[k] = v;
        return result;
    
    }
    public static int Wrap(this int n, int min, int max) => (n >= min, n < max) switch
    {
        (true, true) => n,
        (_, false) => min,
        (false, _) => max - 1
    };
    public static T Percentile<T>(this IEnumerable<T> numbers, double percentile)
        where T : INumberBase<T>
    {
        IEnumerable<T> sorted = numbers.Order();
        int target = (int)(sorted.Count() * percentile + 0.5);
        int ct = 0;
        foreach(T number in sorted)
        {
            if (ct == target)
                return number;
            ct++;
        }
        return sorted.Last();
    }
}
public enum LocationType
{
    Path,
    Url,
    Invalid
}