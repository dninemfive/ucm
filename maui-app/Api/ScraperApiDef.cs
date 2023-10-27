using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;

namespace d9.ucm;
internal class ScraperApiDef : ApiDef
{
    [JsonInclude]
    public override string ApiUrlKey { get; protected set; }
    [JsonInclude]
    public string JsonStartString { get; private set; }
    [JsonInclude]
    public string JsonEndString { get; private set; }
    public ScraperApiDef(string apiUrlKey, string jsonStartString, string jsonEndString)
    {
        ApiUrlKey = apiUrlKey;
        JsonStartString = jsonStartString;
        JsonEndString = jsonEndString;
    }
    public override async Task<string?> GetFileUrlAsync(TransformedUrl tfedUrl)
    {
        // request + cache html
        // find/construct url
        // return it
        // possibly start a timeout to avoid getting rate-limited or blocked
        string? html = await Cache(tfedUrl);
        if(html is null)
        {
            return null;
        }
        string? doThing()
        {
            string? json = html.ItemBetween(JsonStartString, JsonEndString);
            if (json is null)
                return null;
            string unescaped = HttpUtility.HtmlDecode(json).Unescape();
            unescaped = Regex.Split(unescaped.Split($"\"{tfedUrl.Id}\":")[1], ",\"\\d{9}")[0];
            return unescaped;
        }
        string? unescaped = await Task.Run(doThing);
        if (unescaped is null)
            return null;
        //Utils.Log(unescaped);
        // .Split(",\"447510389")[0];
        Utils.Log(unescaped);
        try
        {
            JsonElement el = JsonSerializer.Deserialize<JsonElement>(unescaped)!;
            Utils.Log(el.PrettyPrint());
            JsonElement media = el.GetProperty("media");
            string baseUri = media.GetProperty("baseUri").GetString()!;
            string prettyName = media.GetProperty("prettyName").GetString()!;
            string token = media.GetProperty("token").EnumerateArray().First().GetString()!;
            JsonElement type = media.GetProperty("types").EnumerateArray().Where(x => x.GetProperty("t").GetString() == "preview").First();
            string resultUrl = $"{baseUri}{type.GetProperty("c").GetString()!.Replace("<prettyName>", prettyName)}?token={token}";
            Utils.Log(resultUrl);
            return resultUrl;
        }
        catch (Exception ex)
        {
            Utils.Log(ex);
            return null;
        }
    }
    public async Task<string?> Cache(TransformedUrl tfedUrl)
    {
        string? response = null;
        if(tfedUrl.Urls.TryGetValue(ApiUrlKey, out string? apiUrl))
        try
        {
            response = await MauiProgram.HttpClient.GetStringAsync(apiUrl);
        }
        catch (Exception e)
        {
            Utils.Log($"Cache({tfedUrl}): {e.GetType().Name} {e.Message}");
        }
        if (response is not null)
        {
            // _responses[urlSet.ApiUrl!] = response.Value;
            _ = Directory.CreateDirectory(tfedUrl.CacheFolder);
            File.WriteAllText($"{tfedUrl.CacheFilePath}.html", response);
        }
        else
        {
            Utils.Log($"Cache({tfedUrl}): Failed to get response for {apiUrl}");
        }
        return response;
    }
    public override Task<IEnumerable<string>?> GetTagsAsync(TransformedUrl tfedUrl)
    {
        // look at cached html
        // find tag info idk
        throw new NotImplementedException();
    }
}
