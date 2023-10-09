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
    public View View => new Image()
    {
        Source = ImageSource.FromStream(() => new MemoryStream(Data)),
        IsAnimationPlaying = true,
        Aspect = Aspect.AspectFit
    };
    public IEnumerable<ItemSource> ItemSources
    {
        get
        {
            yield return Source;
        }
    }
    public string Hash { get; private set; }
    public UrlSet? UrlSet { get; private set; }
    public byte[] Data { get; private set; }
    public string? SourceUrl { get; private set; }
    public string? LocalPath { get; private set; }
    public string Location => (LocalPath ?? UrlSet?.CanonUrl)!;
    public ItemSource Source { get; private set; }
    private CandidateItem(string hash, byte[] data, ItemSource source, string? sourceUrl = null)
    {
        Hash = hash;
        SourceUrl = sourceUrl;
        Data = data;
        Source = source;
    }
    private CandidateItem(UrlSet urlSet, string hash, byte[] data, ItemSource source, string? sourceUrl = null) 
        : this(hash, data, source, sourceUrl)
    {
        UrlSet = urlSet;        
    }
    private CandidateItem(string localPath, string hash, byte[] data, ItemSource source, string? sourceUrl = null)
        : this(hash, data, source, sourceUrl)
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
            if(sourceLocation is null)
            {
                Utils.Log($"SourceLocation was null for UrlSet {urlSet}, which was allegedly fully valid.");
                return null;
            }
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
            return new(urlSet!, hash, data, await GetItemSourceAsync(null, urlSet), sourceLocation);
        return new(localPath: sourceLocation, hash, data, await GetItemSourceAsync(sourceLocation, null));
    }
    public async Task<bool> SaveAsync()
    {
        Item? result = Item.From(this);
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
    public static async Task<ItemSource> GetItemSourceAsync(string? localPath, UrlSet? urlSet)
    {
        if (localPath is not null)
            return new("Local Filesystem", localPath);
        else
            return new(urlSet!.UrlRule.Name, urlSet!.CanonUrl!, (await urlSet.UrlRule.TagsFor(urlSet))?.ToArray() ?? Array.Empty<string>());
    }
    [JsonIgnore]
    public Label InfoLabel => new()
    {
        Text = $"{this}",
        BackgroundColor = Colors.Transparent,
        TextColor = Colors.White,
        Padding = new(4)
    };
}
