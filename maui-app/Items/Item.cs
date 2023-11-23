using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.IO;
using d9.utl;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using CoenM.ImageHash.HashAlgorithms;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;
using System.Collections;

namespace d9.ucm;
public class Item : IItemViewable
{
    #region properties
    /// <summary>
    /// The canonical path where this item will be loaded, as opposed to source locations
    /// </summary>
    [JsonInclude]
    public LocalPath LocalPath { get; }
    [JsonInclude]
    public string Hash { get; }
    [JsonInclude]
    public ItemId Id { get; }
    [JsonIgnore]
    public View View => LocalPath.Value.BestAvailableView()!;
    [JsonIgnore]
    private Image? _thumbnail = null;
    [JsonIgnore]
    public Image? Thumbnail
    {
        get
        {
            if (_thumbnail is null)
            {
                Microsoft.Maui.Graphics.IImage? thumbnailImage = View as Microsoft.Maui.Graphics.IImage;
                thumbnailImage = thumbnailImage?.Downsize(100);
                // todo: save
            }
            return _thumbnail;
        }
    }
    public async Task<IEnumerable<ItemSource>> GetItemSourcesAsync()
        => await this.GetSourcesAsync() ?? Enumerable.Empty<ItemSource>();
    [JsonInclude]
    public bool Deleted { get; private set; } = false;
    [JsonIgnore]
    public bool Hidden => Deleted || !(MergeInfo?.IsResult(Id) ?? true);
    [JsonInclude]
    public ItemMergeInfo? MergeInfo { get; private set; } = null;
    #endregion
    #region constructors
    public Item(string path, string hash, ItemId id, params ItemSource[] sources) : this(new(path), hash, id, sources.ToList()) { }
    public Item(LocalPath path, string hash, ItemId id, List<ItemSource>? sources, ItemMergeInfo? mergeInfo = null, bool deleted = false)
        : this(path, hash, id, mergeInfo, deleted)
    {
#pragma warning disable CS4014 // "this runs synchronously" bruh it's a constructor what do you want from me
        ItemSourceManager.SaveSourcesAsync(id, sources ?? new() { new("Local Filesystem", path.Value) });
#pragma warning restore CS4014
    }
    [JsonConstructor]
    public Item(LocalPath localPath, string hash, ItemId id, ItemMergeInfo? mergeInfo = null, bool deleted = false)
    {
        LocalPath = localPath;
        Hash = hash;
        Id = IdManager.Register(id);
        Deleted = deleted;
        MergeInfo = mergeInfo;
    }
    #endregion
    public override string ToString()
        => $"Item {Id} @ {LocalPath}";
    public async Task SaveAsync()
    {
        await File.WriteAllTextAsync(Path.Join(Constants.Folders.TEMP_Data, $"{Id}.json"),
                                     JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        await ItemSourceManager.SaveSourcesAsync(Id);
    }
    public async Task<bool> HasSourceInfoFor(string location)
        => (await this.GetSourcesAsync())?.Any(x => x.Location == location) ?? false;
    public async Task<IEnumerable<string>> GetLocationsAsync()
        => (await this.GetSourcesAsync())?.Select(x => x.Location) ?? Enumerable.Empty<string>();
    public static Item? From(CandidateItem ci)
    {        
        if(ci.LocalPath is not null)
        {
            return new(ci.LocalPath, ci.Hash, IdManager.Register(), ci.Source);
        }
        ApiDef? apiDef = ci.TfedUrl?.Api;
        if (apiDef is not null)
        {
            ItemId id = IdManager.Register();
            if (ci.Data is not null)
            {
                string? newPath = Path.Join(MauiProgram.ITEM_FILE_LOCATION, $"{id}{ci.SourceUrl?.FileExtension()}");
                File.WriteAllBytes(newPath, ci.Data);
                return new(newPath, ci.Hash, id, ci.Source);
            }            
        }
        Utils.Log($"Failed to make item for {ci}.");
        return null;
    }
    public static async Task<Item> MergeAsync(Item chosenItem, IEnumerable<Item> otherItems)
    {
        Utils.Log($"Merge({chosenItem}, {otherItems.ListNotation()})");
        List<ItemSource> sources = await chosenItem.GetSourcesAsync() ?? new();
        ItemId resultId = IdManager.Register();
        List<Item> relevantItems = otherItems.Append(chosenItem)
                                             .OrderBy(x => x.Id)
                                             .ToList();
        ItemMergeInfo mergeInfo = new(resultId, relevantItems.Select(x => x.Id).ToList());
        foreach (Item item in relevantItems)
        {
            await resultId.AddSourcesAsync(await item.GetSourcesAsync());
            item.MergeInfo = mergeInfo;
            await item.SaveAsync();
        }
        Item result = new(chosenItem.LocalPath, chosenItem.Hash, resultId, mergeInfo);
        ItemManager.Register(result);
        await result.SaveAsync();
        return result;
    }
    [JsonIgnore]
    public Label InfoLabel
    {
        get
        {
            string text = $"{this}\n\nRatings:{CompetitionManager.Competitions.OrderBy(x => x.Name)
                                                                              .Select(x => $"{x.Name}: {x.RatingOf(this)?.ToString() ?? "(no rating)"}")
                                                                              .AsBulletedList()}";
            return new()
            {
                Text = text,
                BackgroundColor = Colors.Transparent,
                TextColor = Colors.White,
                Padding = new(4)
            };
        }
    }
    public static implicit operator ItemId(Item item) => item.Id;
    [JsonIgnore]
    private HashSet<string>? _allTags = null;
    public async Task<HashSet<string>> GetAllTagsAsync()
    {
        if (_allTags is null)
        {
            _allTags = new();
            foreach (ItemSource source in (await ItemSourceManager.GetSourcesAsync(this))!)
            {
                foreach (string tag in source.Tags)
                {
                    if (tag.Contains(' '))
                        Utils.Log($"Item {Id} has tag `{tag}` from source {source.SourceName}. This tag includes spaces, meaning it cannot be searched for!");
                    _ = _allTags.Add(tag.TagNormalize());
                }
            }
        }
        return _allTags!;
    }
    public async Task AddSource(ItemSource source)
    {
        await Id.AddSourceAsync(source, true);
        _allTags = null;
    }
}
