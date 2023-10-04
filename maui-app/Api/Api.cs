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
    public virtual async Task<string?> GetFileUrlAsync(ApiUrl apiUrl)
        => throw new NotImplementedException();
    public virtual async Task<IEnumerable<string>?> GetTagsAsync(ApiUrl apiUrl)
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
    public string TagDelimiter { get; private set; }
    [JsonInclude]
    public Dictionary<string, string> Metadata { get; private set; }
    [JsonIgnore]
    public Dictionary<string, Type> MetadataTypes { get; private set; } = new();
    [JsonIgnore]
    private Dictionary<string, JsonElement> _responses { get; set; } = new();
    [JsonInclude]
    public string RootPath { get; private set; }
    [JsonConstructor]
    public JsonApiDef(string fileUrlKey, string tagKey, string tagDelimiter, Dictionary<string, string> metadata, string rootPath = "")
    {
        FileUrlKey = fileUrlKey;
        TagKey = tagKey;
        TagDelimiter = tagDelimiter;
        Metadata = metadata;
        foreach((string k, string v) in metadata)
        {
            Type? type = v.ToType();
            if(type is not null)
            {
                MetadataTypes[k] = type;
            }
        }
        RootPath = rootPath;
    }
    private JsonElement? GetRoot(JsonDocument? doc)
    {
        if (doc is null)
            return null;
        if (RootPath.Length == 0)
            return doc.RootElement;
        JsonElement root = doc.RootElement;
        foreach(string s in RootPath.Split("/", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            Utils.Log(s);
            root = root.ValueKind switch
            {
                JsonValueKind.Object => root.GetProperty(s),
                JsonValueKind.Array => root.EnumerateArray().First()
            };
        }
        return root;
    }
    public async Task<JsonElement?> GetResponse(ApiUrl url)
    {
        string apiUrl = url.Value;
        if (_responses.TryGetValue(apiUrl, out JsonElement response))
            return response;
        JsonElement? response2 = null;
        try
        {
            response2 = GetRoot(await MauiProgram.HttpClient.GetFromJsonAsync<JsonDocument>(apiUrl));
        } 
        catch(Exception e)
        {
            Utils.Log($"GetResponse({url}): {e.GetType().Name} {e.Message}");
        }        
        if(response2 is not null)
        {
            _responses[apiUrl] = response2.Value;
            string cacheFolder = Path.Join(MauiProgram.TEMP_SAVE_LOCATION, "cache", new Uri(apiUrl).Host);
            _ = Directory.CreateDirectory(cacheFolder);
            string? id = UrlRule.BestIdFor(apiUrl);
            if (id is not null)
            {
                File.WriteAllText(Path.Join(cacheFolder, $"{id}.json"), JsonSerializer.Serialize(response2));
            }
            else
            {
                Utils.Log($"best id was null for {apiUrl}");
            }
        }
        else
        {
            Utils.Log($"FileUrlAsync(): Failed to get response for {apiUrl}");
        }
        return response2;
    }
    public override async Task<string?> GetFileUrlAsync(ApiUrl? apiUrl)
    {
        if (apiUrl is null)
            return null;
        JsonElement? response = await GetResponse(apiUrl);
        return response?.GetProperty(FileUrlKey).GetString();
    }
    public override async Task<IEnumerable<string>?> GetTagsAsync(ApiUrl? apiUrl)
    {
        if (apiUrl is null)
            return null;
        JsonElement? response = await GetResponse(apiUrl);
        return response?.GetProperty(TagKey).GetString()?.Split(TagDelimiter);
    }
}