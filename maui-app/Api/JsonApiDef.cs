﻿using System;
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
    public JsonApiDef(string fileUrlKey, string tagKey, string tagDelimiter, Dictionary<string, string> metadata, string rootPath = "")
    {
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
    public async Task<JsonElement?> GetResponse(UrlSet urlSet)
    {
        if (!urlSet.IsFullyValid())
            return null;
        string apiUrl = urlSet.ApiUrl!;
        if (_responses.TryGetValue(apiUrl, out JsonElement response))
            return response;
        return await Cache(urlSet);
    }
    public async Task<JsonElement?> Cache(UrlSet urlSet)
    {
        JsonElement? response = null;
        try
        {
            response = GetRoot(await MauiProgram.HttpClient.GetFromJsonAsync<JsonDocument>(urlSet.ApiUrl));
        } 
        catch(Exception e)
        {
            Utils.Log($"Cache({urlSet.CanonUrl}): {e.GetType().Name} {e.Message}");
        }
        if(response is not null)
        {
            _responses[urlSet.ApiUrl!] = response.Value;
            _ = Directory.CreateDirectory(urlSet.CacheFolder);
            File.WriteAllText(Path.Join(urlSet.CacheFolder, $"{urlSet.Id}.json"), JsonSerializer.Serialize(response));
        } 
        else
        {
            Utils.Log($"Cache({urlSet.CanonUrl}): Failed to get response for {urlSet.ApiUrl}");
        }
        return response;
    }
    public override async Task<string?> GetFileUrlAsync(UrlSet urlSet)
    {
        if (urlSet.ApiUrl is null)
            return null;
        try
        {
            JsonElement? response = await GetResponse(urlSet);
            return response?.GetProperty(FileUrlKey).GetString();
        }
        catch (Exception e)
        {
            Utils.Log(e);
            return null;
        }
    }
    public override async Task<IEnumerable<string>?> GetTagsAsync(UrlSet urlSet)
    {
        if (urlSet.ApiUrl is null)
            return null;
        JsonElement? response = await GetResponse(urlSet);
        return response?.GetProperty(TagKey).GetString()?.Split(TagDelimiter);
    }
}