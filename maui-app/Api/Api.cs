using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using d9.utl;

namespace d9.ucm;
public abstract class ApiDef
{

}
public class JsonApiDef : ApiDef
{
    [JsonInclude]
    public string FileUrlKey { get; private set; }
    [JsonInclude]
    public string TagKey { get; private set; }
    [JsonInclude]
    public Dictionary<string, string> Metadata { get; private set; }
    [JsonIgnore]
    public Dictionary<string, Type> MetadataTypes { get; private set; } = new();
    [JsonConstructor]
    public JsonApiDef(string fileUrlKey, string tagKey, Dictionary<string, string> metadata)
    {
        FileUrlKey = fileUrlKey;
        TagKey = tagKey;
        Metadata = metadata;
        foreach((string k, string v) in metadata)
        {
            Type? type = v.ToType();
            if(type is not null)
            {
                MetadataTypes[k] = type;
                Utils.Log($"Successfully added metadata {k}<{type}>.");
            }
            else
            {
                Utils.Log($"Unable to add metadata {k}: type {v} not recognized!");
            } 
        }
    }
}