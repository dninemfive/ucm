using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using d9.utl;

namespace d9.ucm;
public interface IApiHandler
{
    public async Task<string> FileUrlAsync(string resourceUrl, UrlRule urlRule)
    {
        throw new NotImplementedException();
    }
}
public class JsonApiHandler : IApiHandler
{
    public async Task<string?> FileUrlAsync(string resourceUrl)
    {
        Utils.Log($"\t\tFileUrlAsync({resourceUrl})");
        JsonDocument? response = await MauiProgram.HttpClient.GetFromJsonAsync<JsonDocument>(resourceUrl);
        if(response is null)
        {
            Utils.Log($"\t\tFailed to get response for {resourceUrl}");
            return null;
        }
        string? result = response.RootElement.GetProperty("file_url").GetString();
        Utils.Log($"\t\tResult: {result.PrintNull()}");
        _ = Directory.CreateDirectory(Path.Join(MauiProgram.TEMP_SAVE_LOCATION, "cache", new Uri(resourceUrl).Host));
        string? id = UrlRule.BestFor(resourceUrl)?.IdFor(resourceUrl);
        if(id is not null)
        {
            File.WriteAllText(Path.Join(MauiProgram.TEMP_SAVE_LOCATION, "cache", new Uri(resourceUrl).Host, $"{id}.json"),
                JsonSerializer.Serialize(response));
        }        
        return result;
    }
}