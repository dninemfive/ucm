using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.IO;

namespace d9.ucm;
public interface IItem
{
    [JsonInclude]
    public string Path { get; }
    [JsonInclude]
    public string Hash => Path.FileHash()!;
    [JsonInclude]
    public ItemId Id { get; }
    [JsonIgnore]
    public View View { get; }
    public async void SaveAsync()
    {
        await File.WriteAllTextAsync(MauiProgram.TEMP_SAVE_LOCATION, JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = true}));
    }
    public static async IAsyncEnumerable<T> LoadAllAsync<T>() where T : IItem
    {
        foreach(string path in await Task.Run(() => Directory.EnumerateFiles(MauiProgram.TEMP_SAVE_LOCATION)))
        {
            if (System.IO.Path.GetExtension(path) is not ".json")
                continue;
            T? item = await JsonSerializer.DeserializeAsync<T>(File.OpenRead(path));
            if (item is not null)
                yield return item;
        }
    }
}
