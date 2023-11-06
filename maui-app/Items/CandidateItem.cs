using d9.utl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace d9.ucm;
// todo: idisposable to make sure `Data` doesn't hang around after item is saved
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
    public TransformedUrl? TfedUrl { get; private set; }
    public byte[] Data { get; private set; }
    public string? SourceUrl { get; private set; }
    public string? LocalPath { get; private set; }
    public string Location => (LocalPath ?? TfedUrl!.Canonical)!;
    public ItemSource Source { get; private set; }
    private CandidateItem(string hash, byte[] data, ItemSource source, string? sourceUrl = null)
    {
        Hash = hash;
        SourceUrl = sourceUrl;
        Data = data;
        Source = source;
    }
    private CandidateItem(TransformedUrl tfedUrl, string hash, byte[] data, ItemSource source, string? sourceUrl = null) 
        : this(hash, data, source, sourceUrl)
    {
        TfedUrl = tfedUrl;        
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
        else if(TransformedUrl.For(location) is TransformedUrl tfedUrl)
        {
            return await MakeFromUrlAsync(tfedUrl);
        }
        return null;
    }
    private static async Task<CandidateItem?> MakeFromUrlAsync(TransformedUrl tfedUrl)
    {
        string? fileUrl = await tfedUrl.Api.GetFileUrlAsync(tfedUrl);
        if (fileUrl is null)
            return null;
        byte[]? data = await fileUrl.GetBytesAsync(LocationType.Url);
        if (data is null)
            return null;
        string? hash = await data.HashAsync();
        if (hash is null)
            return null;
        return new(tfedUrl, hash, data, await GetItemSourceAsync(null, tfedUrl), fileUrl);
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
    public static async Task<string?> GetFileUrlAsync(TransformedUrl tfedUrl)
        => await tfedUrl.Api.GetFileUrlAsync(tfedUrl);
    public override string ToString()
        => $"Candidate Item @ {Location}";
    public static async Task<ItemSource> GetItemSourceAsync(string? localPath, TransformedUrl? tfedUrl)
    {
        if (localPath is not null)
            return new("Local Filesystem", localPath);
        else if(tfedUrl is not null)
            return new(tfedUrl.Name, tfedUrl.Canonical, (await tfedUrl.Api.GetTagsAsync(tfedUrl))?.ToArray() ?? Array.Empty<string>());
        throw new Exception($"GetItemSourceAsync(): exactly one of `localPath` and `tfedUrl` must be non-null!");
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
