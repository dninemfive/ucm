using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace d9.ucm;
public static class ItemSourceManager
{
    public static async Task<List<ItemSource>?> GetSourcesAsync(this Item item)
        => await item.Id.GetSourcesAsync();
    public static async Task<List<ItemSource>?> GetSourcesAsync(this ItemId id)
    {
        if (_cache.TryGetValue(id, out List<ItemSource>? result))
            return result;
        string file = PathFor(id);
        List<ItemSource>? sources = null;
        if (Directory.Exists(file))
        {
            sources = await Task.Run(() => JsonSerializer.Deserialize<List<ItemSource>>(File.ReadAllText(file)));
        } 
        else
        {
            Utils.Log($"Found no sources for item {id}.");
        }
        _cache[id] = sources;
        return sources;
    }
    public static async Task AddSourceAsync(this ItemId id, ItemSource source, bool cache = true)
    {
        List<ItemSource> sources = await GetSourcesAsync(id) ?? new();
        sources.Add(source);
        await id.SaveSourcesAsync(sources, cache);
    }
    public static async Task AddSourcesAsync(this ItemId id, IEnumerable<ItemSource>? sources, bool cache = true)
    {
        if (sources is null)
            return;
        List<ItemSource> existingSources = await GetSourcesAsync(id) ?? new();
        sources = existingSources.Concat(sources).ToList();
        await id.SaveSourcesAsync(sources.ToList(), cache);
    }
    public static async Task SaveSourcesAsync(this ItemId id, List<ItemSource>? sources = null, bool cache = true)
    {
        sources ??= await GetSourcesAsync(id);
        await File.WriteAllTextAsync(PathFor(id), JsonSerializer.Serialize(sources, new JsonSerializerOptions() { WriteIndented = true }));
        if (cache)
        {
            _cache[id] = sources;
        }
        else
        {
            Uncache(id);
        } 
    }
    public static string PathFor(ItemId id) => Path.Join(Constants.Folders.TEMP_Sources, $"{id}.json");
    private static readonly Dictionary<ItemId, List<ItemSource>?> _cache = new();
    public static void Uncache(ItemId item) => _cache.Remove(item);
}
