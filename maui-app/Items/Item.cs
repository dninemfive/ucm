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
            _thumbnail ??= View as Image;
            return _thumbnail;
        }
    }
    /// <summary>
    /// DO NOT USE THIS USE ItemSources
    /// </summary>
    [JsonInclude]
    public List<ItemSource> Sources { get; private set; } = new();
    [JsonIgnore]
    public IEnumerable<ItemSource> ItemSources => Sources;
    #endregion
    #region constructors
    public Item(string path, string hash, ItemId id, params ItemSource[] sources) : this(new(path), hash, id, sources.ToList()) { }
    [JsonConstructor]
    public Item(LocalPath localPath, string hash, ItemId id, List<ItemSource>? sources)
    {
        LocalPath = localPath;
        Hash = hash;
        Id = IdManager.Register(id);
        Sources = sources?.ToList() ?? new() { new("Local Filesystem", localPath.Value) };
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
