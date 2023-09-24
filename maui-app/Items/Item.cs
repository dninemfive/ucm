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
    #endregion
    #region constructors
    public Item(string path, string? hash)
    {
        Path = path;
        Hash = hash ?? Path.FileHash()!;
        Id = IdManager.Register();
    }
    [JsonConstructor]
    public Item(string path, string hash, ItemId id)
    {
        Path = path;
        Hash = hash;
        Id = IdManager.Register(id);
    }
    #endregion
    public override string ToString()
        => $"Item {Id} @ {Path}";
    public async Task SaveAsync()
    {
        await File.WriteAllTextAsync(@$"{MauiProgram.TEMP_SAVE_LOCATION}\{Id}.json",
                                     JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }
}
