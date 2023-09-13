using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace d9.ucm;
public abstract class FileReference
{
    // "Consider marking these as nullable": implementors will make sure this is the case
    #pragma warning disable CS8618
    public string Location { get; private set; }
    public byte[] Hash { get; }
    #pragma warning restore CS8618
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
public class LocalFileReference
{
    [JsonInclude]
    public string Location { get; private set; }
    [JsonIgnore]
    private byte[]? _fileHash = null;
    [JsonInclude]
    public byte[] FileHash
    {
        get
        {
            _fileHash ??= Location.FileHash();
            return _fileHash!;
        }
    }
    private LocalFileReference(string path)
    {
        Location = path;
    }
    public static LocalFileReference? TryLoad(string path)
    {
        try
        {
            return new(path);
        } 
        catch
        {
            return null;
        }
    }
}
