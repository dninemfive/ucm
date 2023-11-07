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
    private ulong? _perceptualHash = null;
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
    [JsonInclude]
    public List<ItemSource> Sources { get; private set; } = new();
    [JsonIgnore]
    public IEnumerable<ItemSource> ItemSources => Sources;
    [JsonInclude]
    public bool Deleted { get; private set; } = false;
    [JsonIgnore]
    public bool Hidden => Deleted || MergeInfo is not null;
    [JsonInclude]
    public ItemMergeInfo? MergeInfo { get; private set; } = null;
    #endregion
    #region constructors
    public Item(string path, string hash, ItemId id, params ItemSource[] sources) : this(new(path), hash, id, sources.ToList()) { }
    [JsonConstructor]
    public Item(LocalPath localPath, string hash, ItemId id, List<ItemSource>? sources, ItemMergeInfo? mergeInfo = null, bool deleted = false)
    {
        LocalPath = localPath;
        Hash = hash;
        Id = IdManager.Register(id);
        Sources = sources?.ToList() ?? new() { new("Local Filesystem", localPath.Value) };
        Deleted = deleted;
        MergeInfo = mergeInfo;
    }
    #endregion
    public override string ToString()
        => $"Item {Id} @ {LocalPath}";
    public async Task SaveAsync()
    {
        await File.WriteAllTextAsync(@$"{MauiProgram.TEMP_SAVE_LOCATION}\{Id}.json",
                                     JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }
    public bool HasSourceInfoFor(string location)
        => Sources.Any(x => x.Location == location);
    [JsonIgnore]
    public IEnumerable<string> Locations
        => Sources.Select(x => x.Location);
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
    public static bool TryMerge(Item chosenItem, IEnumerable<Item> otherItems, out Item? result)
    {
        result = null;
        // if already merged return false
        List<ItemSource> sources = chosenItem.ItemSources.ToList();
        ItemId resultId = IdManager.Register();
        ItemMergeInfo mergeInfo = new(resultId, otherItems.Select(x => x.Id).ToList());
        foreach (Item otherItem in otherItems)
        {
            sources = sources.Concat(otherItem.ItemSources).ToList();
            otherItem.MergeInfo = mergeInfo;
        }
        result = new(chosenItem.LocalPath, chosenItem.Hash, IdManager.Register(), sources, mergeInfo);
        return true;
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
    [JsonIgnore]
    public HashSet<string> AllTags
    {
        get
        {
            if(_allTags is null)
            {
                _allTags = new();
                foreach(ItemSource source in Sources)
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
    }
    public void AddSource(ItemSource source)
    {
        Sources.Add(source);
        _allTags = null;
    }
}
