using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace d9.ucm;
public interface IFileReference
{
    [JsonInclude]
    public string Location { get; }
    [JsonInclude]
    public byte[] Hash { get; protected set; }
}
public static class Extensions
{
    public static byte[]? FileHash(this string path)
    {
        if (!File.Exists(path))
            return null;
        using SHA512 sha512 = SHA512.Create();
        using FileStream fs = File.OpenRead(path);
        return sha512.ComputeHash(fs);
    }
    public static async Task<byte[]?> FileHashAsync(this string path) => await Task.Run(path.FileHash);
}