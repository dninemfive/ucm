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
#pragma warning disable CS1998
    public async Task<string> FileUrlAsync(string resourceUrl, UrlRule urlRule)
#pragma warning restore CS1998
    {
        throw new NotImplementedException();
    }
}
public class JsonApiHandler : IApiHandler
{
    public async Task<string?> FileUrlAsync(string resourceUrl)
    {
        JsonDocument? response = await MauiProgram.HttpClient.GetFromJsonAsync<JsonDocument>(resourceUrl);
        if(response is null)
        {
            Utils.Log($"FileUrlAsync(): Failed to get response for {resourceUrl}");
            return null;
        }
        string? result = response.RootElement.GetProperty("file_url").GetString();
        _ = Directory.CreateDirectory(Path.Join(MauiProgram.TEMP_SAVE_LOCATION, "cache", new Uri(resourceUrl).Host));
        string? id = UrlRule.BestIdFor(resourceUrl);
        if(id is not null)
        {
            File.WriteAllText(Path.Join(MauiProgram.TEMP_SAVE_LOCATION, "cache", new Uri(resourceUrl).Host, $"{id}.json"),
                JsonSerializer.Serialize(response));
        }        
        return result;
    }
}