using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace d9.ucm;
public class InfoKeyDef
{
    [JsonInclude]
    public string Key;
    [JsonInclude]
    public string Name;
    [JsonInclude]
    public InfoGetterType Type;
    [JsonConstructor]
    public InfoKeyDef(string key, string? name = null, InfoGetterType type = InfoGetterType.Path)
    {
        Key = key;
        Name = name ?? key;
        Type = type;
    }
    public void Deconstruct(out string key, out string name, out InfoGetterType type)
    {
        key = Key;
        name = Name;
        type = Type;
    }
}
