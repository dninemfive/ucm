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
    public static View? BestAvailableView(this string path) => Path.GetExtension(path).ToLower() switch
    {
        // https://devblogs.microsoft.com/dotnet/announcing-dotnet-maui-communitytoolkit-mediaelement/
        ".mov" or ".mp4" or ".webm" => null,
        ".json" or ".txt" => new Label() { Text = File.ReadAllText(path) },
        ".xcf" or ".pdf" => null,
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
}