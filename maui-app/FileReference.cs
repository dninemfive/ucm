using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ucm;
public abstract class FileReference
{
    // "Consider marking these as nullable": implementors will make sure this is the case
    #pragma warning disable CS8618
    public string Location { get; private set; }
    public byte[] Hash { get; }
    #pragma warning restore CS8618
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
            if(_fileHash is null)
            {
                using SHA512 sha512 = SHA512.Create();
                using FileStream fs = File.OpenRead(Location);
                _fileHash = sha512.ComputeHash(fs);
            }
            return _fileHash;
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
