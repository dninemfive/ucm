using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace d9.ucm;
public interface IItem
{
    [JsonInclude]
    public FileReference File { get; }
    [JsonIgnore]
    public byte[] Hash => File.Hash;
    [JsonInclude]
    public ItemId Id { get; }
    [JsonIgnore]
    public View View { get; }
}
