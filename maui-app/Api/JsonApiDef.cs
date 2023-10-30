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
    public override string ApiUrlKey { get; protected set; }
    public string FileUrlKey { get; private set; }
    public string TagKey { get; private set; }
    public string TagDelimiter { get; private set; }
    public string RootPath { get; private set; }
    public JsonApiDef(string apiUrlKey, string fileUrlKey, string tagKey, string tagDelimiter, string rootPath = "")
        : base(new())
    {
        ApiUrlKey = apiUrlKey;
        FileUrlKey = fileUrlKey;
        TagKey = tagKey;
        TagDelimiter = tagDelimiter;
        RootPath = rootPath;
    }
    public JsonApiDef(Dictionary<string, string> args) : this(
        args[nameof(ApiUrlKey)],
        args[nameof(FileUrlKey)],
        args[nameof(TagKey)],
        args[nameof(TagDelimiter)],
        args.TryGetValue(nameof(RootPath), out string? rootPath) ? rootPath : "") { }
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
        if (File.Exists($"{tfedUrl.CacheFilePath}.json"))
            return await Task.Run(() => JsonSerializer.Deserialize<JsonElement>(File.ReadAllText($"{tfedUrl.CacheFilePath}.json")));
        return await Cache(tfedUrl, apiUrl);
    }
    public async Task<JsonElement?> Cache(TransformedUrl tfedUrl, string apiUrl)
    {
        JsonElement? response = null;        
        try
        {
            response = GetRoot(await MauiProgram.HttpClient.GetFromJsonAsync<JsonDocument>(apiUrl));
        } 
        catch(Exception e)
        {
            Utils.Log($"Cache({tfedUrl.Canonical}): {e.GetType().Name} {e.Message}");
        }
        if(response is not null)
        {
            _ = Directory.CreateDirectory(tfedUrl.CacheFolder);
            File.WriteAllText($"{tfedUrl.CacheFilePath}.json", JsonSerializer.Serialize(response));
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