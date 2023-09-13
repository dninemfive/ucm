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
    public IFileReference File { get; private set; }
    [JsonInclude]
    public ItemId Id { get; private set; }
    [JsonIgnore]
    private Image? _image = null;
    [JsonIgnore]
    public View View
    {
        get
        {
            _image ??= new() { Source = File.Location };
            return _image;
        }
    }
    public ImageItem(IFileReference file, ItemId? id = null)
    {
        File = file;
        Id = IdManager.Register(id);
    }
    public async void SaveAsync()
    {
        //await System.IO.File.WriteAllTextAsync(@"C:\Users\dninemfive\Pictures\misc\ucm\data", JsonSerializer.Serialize(this));
        System.IO.File.WriteAllText(@"C:\Users\dninemfive\Pictures\misc\ucm\data", JsonSerializer.Serialize(this));
    }
}
