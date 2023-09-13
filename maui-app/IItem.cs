using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.IO;

namespace d9.ucm;
public interface IItem
{
    [JsonInclude]
    public IFileReference FileReference { get; }
    [JsonIgnore]
    public byte[] Hash => FileReference.Hash;
    [JsonInclude]
    public ItemId Id { get; }
    [JsonIgnore]
    public View View { get; }
    public async void Save()
    {
        await System.IO.File.WriteAllTextAsync("C:/Users/dninemfive/Pictures/misc/ucm", JsonSerializer.Serialize(this));
    }
}
