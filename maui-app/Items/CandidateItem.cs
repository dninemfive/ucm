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
    public string Location => (LocalPath ?? .CanonUrl)!;
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
        if (File.Exists(location))
        {
            return await MakeFromLocalAsync(location);
        } 
        else if(TransformedUrl.For(location) is TransformedUrl summary)
        {
            return await MakeFromUrlAsync(location);
        }
        return null;
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
    private static async Task<CandidateItem?> MakeFromUrlAsync(string url)
    {
        TransformedUrl? summary = TransformedUrl.For(url);
        if (summary is null)
            return null;
        
    }
    private static async Task<CandidateItem?> MakeFromLocalAsync(string path)
    {
        if (!File.Exists(path) || !path.ExtensionIsSupported())
            return null;
        byte[]? data = await path.GetBytesAsync(LocationType.Path);
        if (data is null)
            return null;
        string? hash = await data.HashAsync();
        if(hash is null) 
            return null;
        return new(localPath: path, hash, data, await GetItemSourceAsync(path, null));
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
        => await .UrlRule.FileUrlFor(urlSet);
    public override string ToString()
        => $"Candidate Item @ {Location}";
    public static async Task<ItemSource> GetItemSourceAsync(string? localPath, UrlSet? urlSet)
    {
        if (localPath is not null)
            return new("Local Filesystem", localPath);
        else
            return new(..Name, .CanonUrl!, (await .UrlRule.TagsFor(urlSet))?.ToArray() ?? Array.Empty<string>());
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
