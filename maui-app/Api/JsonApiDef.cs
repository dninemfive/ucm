using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Json;

namespace d9.ucm;
public class JsonApiDef : ApiDef
{
    [JsonInclude]
    public string ApiUrlKey { get; private set; }
    [JsonInclude]
    public string FileUrlKey { get; private set; }
    [JsonInclude]
    public string TagKey { get; private set; }
    [JsonInclude]
    public string TagDelimiter { get; private set; }
    // todo: move metadata to api
    [JsonInclude]
    public Dictionary<string, string> Metadata { get; private set; }
    [JsonIgnore]
    public Dictionary<string, Type> MetadataTypes { get; private set; } = new();
    [JsonIgnore]
    // todo: purge this cache on occasion, or perhaps just load from file each time lol
    private Dictionary<string, JsonElement> _responses { get; set; } = new();
    [JsonInclude]
    public string RootPath { get; private set; }
    [JsonConstructor]
    public JsonApiDef(string apiUrlKey, string fileUrlKey, string tagKey, string tagDelimiter, Dictionary<string, string> metadata, string rootPath = "")
    {
        ApiUrlKey = apiUrlKey;
        FileUrlKey = fileUrlKey;
        TagKey = tagKey;
        TagDelimiter = tagDelimiter;
        Metadata = metadata;
        foreach ((string k, string v) in metadata)
        {
            Type? type = v.ToType();
            if (type is not null)
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
        foreach (string s in RootPath.Split("/", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            root = root.ValueKind switch
            {
                JsonValueKind.Object => root.GetProperty(s),
                JsonValueKind.Array => root.EnumerateArray().First(),
                JsonValueKind.Undefined => throw new NotImplementedException(),
                JsonValueKind.String => throw new NotImplementedException(),
                JsonValueKind.Number => throw new NotImplementedException(),
                JsonValueKind.True => throw new NotImplementedException(),
                JsonValueKind.False => throw new NotImplementedException(),
                JsonValueKind.Null => throw new NotImplementedException(),
                _ => throw new InvalidCastException(nameof(root))
            };
        }
        return root;
    }
    public async Task<JsonElement?> GetResponse(TransformedUrl tfedUrl)
    {
        if (!tfedUrl.Urls.TryGetValue(ApiUrlKey, out string? apiUrl))
            return null;
        if (_responses.TryGetValue(apiUrl, out JsonElement response))
            return response;
        return await Cache(tfedUrl, apiUrl);
    }
    public async Task<JsonElement?> Cache(TransformedUrl tfedUrl, string apiUrl)
    {
        JsonElement? response = null;        
        try
        {
            response = GetRoot(await MauiProgram.HttpClient.GetFromJsonAsync<JsonDocument>(apiUrl);
        } 
        catch(Exception e)
        {
            Utils.Log($"Cache({tfedUrl.Canonical}): {e.GetType().Name} {e.Message}");
        }
        if(response is not null)
        {
            _responses[apiUrl] = response.Value;
            _ = Directory.CreateDirectory(tfedUrl.CacheFolder);
            File.WriteAllText(Path.Join($"{tfedUrl.CacheFilePath}.json"), JsonSerializer.Serialize(response));
        } 
        else
        {
            Utils.Log($"Cache({tfedUrl.Canonical}): Failed to get response for {apiUrl}");
        }
        return response;
    }
    public override async Task<string?> GetFileUrlAsync(TransformedUrl tfedUrl)
    {
        try
        {
            JsonElement? response = await GetResponse(tfedUrl);
            return response?.GetProperty(FileUrlKey).GetString();
        }
        catch (Exception e)
        {
            Utils.Log(e);
            return null;
        }
    }
    public override async Task<IEnumerable<string>?> GetTagsAsync(TransformedUrl tfedUrl)
    {
        JsonElement? response = await GetResponse(tfedUrl);
        return response?.GetProperty(TagKey).GetString()?.Split(TagDelimiter);
    }
}