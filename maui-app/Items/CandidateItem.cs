using d9.utl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace d9.ucm;
public class CandidateItem : IItemViewable
{
    public View View => throw new NotImplementedException();
    public IEnumerable<ItemSource> ItemSources => throw new NotImplementedException();
    public string Hash { get; private set; }
    public UrlSet? UrlSet { get; private set; }
    public byte[]? Data { get; private set; }
    public string? SourceUrl { get; private set; }
    public string? LocalPath { get; private set; }
    public string Location => (LocalPath ?? UrlSet?.CanonUrl)!;
    private CandidateItem(string hash, string? sourceUrl = null, byte[]? data = null)
    {
        Hash = hash;
        SourceUrl = sourceUrl;
        Data = data;
    }
    private CandidateItem(UrlSet urlSet, string hash, string? sourceUrl = null, byte[]? data = null) 
        : this(hash, sourceUrl, data)
    {
        UrlSet = urlSet;        
    }
    private CandidateItem(string localPath, string hash, string? sourceUrl = null, byte[]? data = null)
        : this(hash, sourceUrl, data)
    {
        LocalPath = localPath;
    }
    public static async Task<CandidateItem?> MakeFromAsync(string location)
    {
        LocationType locationType = LocationType.Invalid;
        UrlSet? urlSet = UrlSet.From(location);
        string? sourceLocation = null;
        if (urlSet.IsFullyValid())
        {
            sourceLocation = await GetFileUrlAsync(urlSet!);
            locationType = LocationType.Url;
        }
        else if(File.Exists(location))
        {
            sourceLocation = location;
            locationType = LocationType.Path;
        }
        if (sourceLocation is null || !sourceLocation.ExtensionIsSupported())
            return null;
        byte[]? data = await sourceLocation.GetBytesAsync(locationType);
        string? hash = await (data?.HashAsync() ?? Task.FromResult<string?>(null));
        if (data is null || hash is null)
            return null;
        if (urlSet is not null)
            return new(urlSet!, hash, sourceLocation, data);
        return new(localPath: sourceLocation, hash);
    }
    public async Task<bool> SaveAsync()
    {
        Item? result = await Item.FromAsync(this);
        if(result is not null)
        {
            ItemManager.Register(result);
            await result.SaveAsync();
        }
        return result is not null;
    }
    public static async Task<string?> GetFileUrlAsync(UrlSet urlSet)
        => await urlSet.UrlRule.FileUrlFor(urlSet);
    public override string ToString()
        => $"CI {Hash} @ {Location}";
    public async Task<ItemSource?> GetItemSourceAsync()
    {
        if (LocalPath is not null)
            return new("Local Filesystem", LocalPath);
        else
            return new(UrlSet!.UrlRule.Name, UrlSet!.CanonUrl!, (await UrlSet.UrlRule.TagsFor(UrlSet))?.ToArray() ?? Array.Empty<string>());
    }
}
