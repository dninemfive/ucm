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
    public static IEnumerable<string> EnumerateFilesRecursive(this string folder)
    {
        foreach (string s in Directory.EnumerateFiles(folder))
            yield return s;
        foreach (string s in Directory.EnumerateDirectories(folder))
            foreach (string t in s.EnumerateFilesRecursive())
                yield return t;
    };
}