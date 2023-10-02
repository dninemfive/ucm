using d9.utl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace d9.ucm;
public static class Extensions
{
    public static string? FileHash(this string path)
    {
        if (!File.Exists(path))
            return null;
        using SHA512 sha512 = SHA512.Create();
        using FileStream fs = File.OpenRead(path);
        return JsonSerializer.Serialize(sha512.ComputeHash(fs)).Replace("\"","");
    }
    public static async Task<string?> FileHashAsync(this string path) => await Task.Run(path.FileHash);
    public static string? Hash(this byte[] bytes) => JsonSerializer.Serialize(SHA512.HashData(bytes)).Replace("\"", "");
    public static async Task<string?> HashAsync(this byte[] bytes) => await Task.Run(bytes.Hash);
    public static View? BestAvailableView(this string path) => Path.GetExtension(path).ToLower() switch
    {
        // https://devblogs.microsoft.com/dotnet/announcing-dotnet-maui-communitytoolkit-mediaelement/
        ".mov" or ".mp4" or ".webm" => null,
        // ".json" or ".txt" => new Label() { Text = File.ReadAllText(path) },
        ".xcf" or ".pdf" or ".zip" => null,
        _ => new Image() { Source = path, IsAnimationPlaying = true, Aspect = Aspect.AspectFit }
    };
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
    public static string? FileExtension(this string? path) => Path.GetExtension(path);
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
            T? item = JsonSerializer.Deserialize<T>(File.ReadAllText(path));
            if (item is not null && (validator is null || validator((path, item))))
            {
                ct++;
                yield return item;
            }
        }
        Utils.Log($"Loading {ct} {typeof(T).Name.ToLower()}s from {srcFolder}.");
    }
    public static Competition.Rating? RatingAs(this Item item, string? name)
        => Competition.Named(name)?.RatingOf(item);
    public static string? UriScheme(this string? location)
        => Uri.TryCreate(location, UriKind.Absolute, out Uri? uri) ? uri?.Scheme : null;
    public static ItemSource? ItemSource(this string location)
        => File.Exists(location) ? new("Local Filesystem", location) : UrlHandler.ItemSourceFor(location);
    public static Type? ToType(this string typeName) => Type.GetType(typeName)
                                                     ?? Type.GetType($"System.{typeName}")
                                                     ?? Type.GetType($"d9.ucm.{typeName}");
}