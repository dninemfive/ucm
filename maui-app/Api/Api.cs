using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using d9.utl;

namespace d9.ucm;
public abstract class ApiDef
{
#pragma warning disable CS1998 // "lacks await": intentionally not implemented
    public virtual async Task<string?> GetFileUrlAsync(string resourceUrl)
        => throw new NotImplementedException();
    public virtual async Task<IEnumerable<string>?> GetTagsAsync(string resourceUrl)
        => throw new NotImplementedException();
#pragma warning restore CS1998
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
    [JsonIgnore]
    private Dictionary<string, JsonDocument> _responses { get; set; } = new();
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
                Utils.Log($"Successfully added metadata {k} of type {type}.");
            }
            else
            {
                Utils.Log($"Unable to add metadata {k}: unrecognized type {v}.");
            } 
        }
    }
    public async Task<JsonDocument?> GetResponseDocument(string resourceUrl)
    {
        if (_responses.TryGetValue(resourceUrl, out JsonDocument? response))
            return response;
        response = await MauiProgram.HttpClient.GetFromJsonAsync<JsonDocument>(resourceUrl);
        if(response is not null)
        {
            _responses[resourceUrl] = response;
            string cacheFolder = Path.Join(MauiProgram.TEMP_SAVE_LOCATION, "cache", new Uri(resourceUrl).Host);
            _ = Directory.CreateDirectory(cacheFolder);
            string? id = UrlHandler.BestIdFor(resourceUrl);
            if (id is not null)
                File.WriteAllText(Path.Join(cacheFolder, $"{id}.json"), JsonSerializer.Serialize(response));
        }
        else
        {
            Utils.Log($"FileUrlAsync(): Failed to get response for {resourceUrl}");
        }
        return response;
    }
    public override async Task<string?> GetFileUrlAsync(string resourceUrl)
    {
        JsonDocument? response = await GetResponseDocument(resourceUrl);
        return response?.RootElement.GetProperty(FileUrlKey).GetString();
    }
    public override async Task<IEnumerable<string>?> GetTagsAsync(string resourceUrl)
    {
        JsonDocument? response = await GetResponseDocument(resourceUrl);
        return response?.RootElement.GetProperty(TagKey)
                                    .EnumerateArray()
                                    .Select(x => x.GetString())
                                    .OfType<string>();
    }
}