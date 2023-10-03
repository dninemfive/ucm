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
public class Item
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
    public View? View => LocalPath.Value.BestAvailableView();
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
    [JsonInclude]
    public List<ItemSource> Sources { get; private set; } = new();
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
    public static async Task<Item?> FromAsync(CandidateItem ci)
    {        
        if(File.Exists(ci.CanonicalLocation))
        {
            return new(ci.CanonicalLocation, ci.Hash, IdManager.Register(), new ItemSource("Local Filesystem", ci.CanonicalLocation));
        }        
        UrlRule? urlRule = UrlRule.BestFor(ci.CanonicalLocation);
        if (urlRule is not null)
        {
            ItemId id = IdManager.Register();
            if (ci.Data is not null)
            {
                string? newPath = Path.Join(MauiProgram.ITEM_FILE_LOCATION, $"{id}{Path.GetExtension(ci.SourceUrl)}");
                File.WriteAllBytes(newPath, ci.Data);
                return new(newPath,
                       ci.Hash,
                       id,
                       new ItemSource(urlRule.Name,
                                      ci.CanonicalLocation,
                                      (await urlRule.TagsFor(ci.ApiUrl))?.ToArray() ?? Array.Empty<string>()));
            }            
        }
        Utils.Log($"Failed to make item for {ci}.");
        return null;
    }
}
