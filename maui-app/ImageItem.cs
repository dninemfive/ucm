using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace d9.ucm;
public class ImageItem : IItem
{
    [JsonInclude]
    public string Path { get; private set; }
    [JsonInclude]
    public string Hash { get; private set; }
    [JsonInclude]
    public ItemId Id { get; private set; }
    [JsonIgnore]
    private Image? _image = null;
    [JsonIgnore]
    public View View
    {
        get
        {
            _image ??= new() { Source = Path };
            _image.IsAnimationPlaying = System.IO.Path.GetExtension(Path) == ".gif";
            return _image;
        }
    }
    public ImageItem(string path, string hash)
    {
        Path = path;
        Hash = hash ?? Path.FileHash()!;
        Id = IdManager.Register();
    }
    [JsonConstructor]
    public ImageItem(string path, string hash, ItemId id)
    {
        Path = path;
        Hash = hash ?? Path.FileHash()!;
        Id = IdManager.Register(id);
    }
    public async Task SaveAsync()
    {
        await File.WriteAllTextAsync(@$"{MauiProgram.TEMP_SAVE_LOCATION}\{Id}.json", 
                                     JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }
}
