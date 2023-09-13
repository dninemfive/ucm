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
    public IFileReference FileReference { get; private set; }
    [JsonInclude]
    public ItemId Id { get; private set; }
    [JsonIgnore]
    private Image? _image = null;
    [JsonIgnore]
    public View View
    {
        get
        {
            _image ??= new() { Source = FileReference.Location };
            return _image;
        }
    }
    public ImageItem(IFileReference fileReference, ItemId? id = null)
    {
        FileReference = fileReference;
        Id = IdManager.Register(id);
    }
    public void Save()
    {
        //await System.IO.File.WriteAllTextAsync(@"C:\Users\dninemfive\Pictures\misc\ucm\data", JsonSerializer.Serialize(this));
        DirectoryInfo dinfo = new(@"C:\Users\dninemfive\Pictures\misc\ucm\data");
        File.Create(@"C:\Users\dninemfive\Pictures\misc\ucm\data\fuck you.txt");
        File.WriteAllText(@"C:\Users\dninemfive\Pictures\misc\ucm\data", JsonSerializer.Serialize(this));
    }
}
