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
    public string JsonStartString { get; private set; }
    [JsonInclude]
    public string JsonEndString { get; private set; }
    public ScraperApiDef(string jsonStartString, string jsonEndString)
    {
        JsonStartString = jsonStartString;
        JsonEndString = jsonEndString;
    }
    public override async Task<string?> GetFileUrlAsync(UrlSet urlSet)
    {
        // request + cache html
        // find/construct url
        // return it
        // possibly start a timeout to avoid getting rate-limited or blocked
        string? html = await Cache(urlSet);
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
            unescaped = Regex.Split(unescaped.Split($"\"{.Id}\":")[1], ",\"\\d{9}")[0];
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
    public static async Task<string?> Cache(UrlSet urlSet)
    {
        string? response = null;
        try
        {
            response = await MauiProgram.HttpClient.GetStringAsync();
        }
        catch (Exception e)
        {
            Utils.Log($"Cache({}): {e.GetType().Name} {e.Message}");
        }
        if (response is not null)
        {
            // _responses[urlSet.ApiUrl!] = response.Value;
            _ = Directory.CreateDirectory();
            File.WriteAllText(Path.Join(, $"{}.html"), response);
        }
        else
        {
            Utils.Log($"Cache({}): Failed to get response for {}");
        }
        return response;
    }
    public override Task<IEnumerable<string>?> GetTagsAsync(UrlSet urlSet)
    {
        // look at cached html
        // find tag info idk
        return base.GetTagsAsync(urlSet);
    }
}
