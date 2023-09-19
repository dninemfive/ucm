using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.IO;

namespace d9.ucm;
public class Item
{
    #region properties
    [JsonInclude]
    public string Path { get; }
    [JsonInclude]
    public string Hash { get; }
    [JsonInclude]
    public ItemId Id { get; }
    [JsonIgnore]
    public View? View => Path.BestAvailableView();
    #endregion
    #region constructors
    public Item(string path, string? hash)
    {
        Path = path;
        Hash = hash ?? Path.FileHash()!;
        Id = IdManager.Register();
    }
    [JsonConstructor]
    public Item(string path, string hash, ItemId id)
    {
        Path = path;
        Hash = hash;
        Id = IdManager.Register(id);
    }
    #endregion
    public static async IAsyncEnumerable<Item> LoadAllAsync()
    {
        foreach (string path in await Task.Run(() => Directory.EnumerateFiles(MauiProgram.TEMP_SAVE_LOCATION)))
        {
            if (System.IO.Path.GetExtension(path) is not ".json")
                continue;
            Item? item = await JsonSerializer.DeserializeAsync<Item>(File.OpenRead(path));
            if (item is not null)
                yield return item;
        }
    }
    public static IEnumerable<Item> LoadAll()
    {
        foreach(string path in Directory.EnumerateFiles(MauiProgram.TEMP_SAVE_LOCATION))
        {
            if (System.IO.Path.GetExtension(path) is not ".json")
                continue;
            Item? item = JsonSerializer.Deserialize<Item>(File.ReadAllText(path));
            if (item is not null)
                yield return item;
        }
    }
    public async Task SaveAsync()
    {
        await File.WriteAllTextAsync(@$"{MauiProgram.TEMP_SAVE_LOCATION}\{Id}.json",
                                     JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }
}
