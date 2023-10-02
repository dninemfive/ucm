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
        JsonDocument? response = await MauiProgram.HttpClient.GetFromJsonAsync<JsonDocument>(resourceUrl);
        if(response is null)
        {
            Utils.Log($"\tFailed to get response for {resourceUrl}");
            return null;
        }
        return response.RootElement.GetProperty("file_url").GetString()!;
    }
}