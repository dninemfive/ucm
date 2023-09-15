using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace d9.ucm;
public class LocalImageItem : IItem
{
    [JsonInclude]
    public LocalFileReference FileReference { get; private set; }
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
    [JsonIgnore]
    IFileReference IItem.FileReference => FileReference;
    public LocalImageItem(LocalFileReference fileReference)
    {
        FileReference = fileReference;
        Id = IdManager.Register();
    }
    [JsonConstructor]
    public LocalImageItem(LocalFileReference fileReference, ItemId id)
    {
        FileReference = fileReference;
        Id = IdManager.Register(id);
    }
    public async Task SaveAsync()
    {
        await File.WriteAllTextAsync(@$"{MauiProgram.TEMP_SAVE_LOCATION}\{Id}.json", JsonSerializer.Serialize(this));
    }
}
