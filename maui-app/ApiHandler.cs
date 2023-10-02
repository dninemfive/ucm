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
        return result;
    }
}