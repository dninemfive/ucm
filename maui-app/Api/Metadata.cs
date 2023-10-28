using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
/*
namespace d9.ucm;
internal class Metadata
{
    // todo: move metadata to api
    [JsonInclude]
    public Dictionary<string, string> Metadata { get; private set; }
    [JsonIgnore]
    public Dictionary<string, Type> MetadataTypes { get; private set; } = new();

    Metadata = metadata;
        foreach ((string k, string v) in metadata)
        {
            Type? type = v.ToType();
            if (type is not null)
            {
                MetadataTypes[k] = type;
            }
        }
}
*/