using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.IO;
using d9.utl;

namespace d9.ucm;
public class Item
{
    #region properties
    /// <summary>
    /// The canonical path where this item will be loaded, as opposed to source locations
    /// </summary>
    [JsonInclude]
    public string Path { get; }
    [JsonInclude]
    public string Hash { get; }
    [JsonInclude]
    public ItemId Id { get; }
    [JsonIgnore]
    public View? View => Path.BestAvailableView();
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
    public List<ItemSource> Sources { get; private set; }
    #endregion
    #region constructors
    public Item(string path, string? hash)
    {
        Path = path;
        Sources = new() { new("Local Filesystem", path) };
        Hash = hash ?? path.FileHash()!;
        Id = IdManager.Register();        
    }
    public Item(string path, string hash, ItemId id, params ItemSource[] sources) : this(path, hash, id, sources.ToList()) { }
    [JsonConstructor]
    public Item(string path, string hash, ItemId id, List<ItemSource>? sources)
    {
        Path = path;
        Hash = hash;
        Id = IdManager.Register(id);
        Sources = sources?.ToList() ?? new() { new("Local Filesystem", path) };
    }
    #endregion
    public override string ToString()
        => $"Item {Id} @ {Path}";
    public async Task SaveAsync()
    {
        await File.WriteAllTextAsync(@$"{MauiProgram.TEMP_SAVE_LOCATION}\{Id}.json",
                                     JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }
    public bool HasSourceInfoFor(string location)
        => Sources.Any(x => x.Location == location);
    public IEnumerable<string> Locations
    {
        get
        {
            yield return Path;
            foreach (string loc in Sources.Select(x => x.Location))
                yield return loc;
        }
    }
}
