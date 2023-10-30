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
    public override string ApiUrlKey { get; protected set; }
    public string JsonStartString { get; private set; }
    public string JsonEndString { get; private set; }
    public ScraperApiDef(string apiUrlKey, string jsonStartString, string jsonEndString)
        : base(new())
    {
        ApiUrlKey = apiUrlKey;
        JsonStartString = jsonStartString;
        JsonEndString = jsonEndString;
    }
    public ScraperApiDef(Dictionary<string, string> args) : this(
        args[nameof(ApiUrlKey)],
        args[nameof(JsonStartString)],
        args[nameof(JsonEndString)]) { }
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
        (string? unescaped, string? err) doThing()
        {
            string? json = html.ItemBetween(JsonStartString, JsonEndString);
            if (json is null)
                return (null, null);
            string unescaped = HttpUtility.HtmlDecode(json).Unescape();
            try
            {
                unescaped = Regex.Match(unescaped, $"\"{tfedUrl.Id}\":(.+\"entityId\":{tfedUrl.Id}}})").Groups.Values.ElementAt(1).Value;
            } 
            catch(Exception e)
            {
                return (null, $"{e.GetType().Name}: {e.Message}\n{unescaped}");
            }            
            // unescaped = Regex.Split(unescaped.Split($"\"{tfedUrl.Id}\":")[1], ",\"\\d{9}")[0];
            return (unescaped, null);
        }
        (string? unescaped, string? err) = await Task.Run(doThing);
        if (unescaped is null)
            return null;
        if(err is not null)
        {
            Utils.Log(err);
            return null;
        }
        //Utils.Log(unescaped);
        // .Split(",\"447510389")[0];
        //Utils.Log(unescaped);
        try
        {
            JsonElement el = JsonSerializer.Deserialize<JsonElement>(unescaped)!;
            //Utils.Log(el.PrettyPrint());
            JsonElement media = el.GetProperty("media");
            string baseUri = media.GetProperty("baseUri").GetString()!;
            string prettyName = media.GetProperty("prettyName").GetString()!;
            string token = media.GetProperty("token").EnumerateArray().First().GetString()!;
            JsonElement type = media.GetProperty("types").EnumerateArray().Where(x => x.GetProperty("t").GetString() == "preview").First();
            string resultUrl = $"{baseUri}{type.GetProperty("c").GetString()!.Replace("<prettyName>", prettyName)}?token={token}";
            //Utils.Log(resultUrl);
            return resultUrl;
        }
        catch (Exception ex)
        {
            Utils.Log($"{ex.GetType().Name}: {ex.Message}\n{unescaped}");
            return null;
        }
    }
    public async Task<string?> Cache(TransformedUrl tfedUrl)
    {
        if (File.Exists($"{tfedUrl.CacheFilePath}.html"))
            return await Task.Run(() => File.ReadAllText($"{tfedUrl.CacheFilePath}.html"));
        string? response = null;
        if (!tfedUrl.Urls.TryGetValue(ApiUrlKey, out string? apiUrl))
            return null;
        try
        {
            response = await MauiProgram.HttpClient.GetStringAsync(apiUrl);
        }
        catch (Exception e)
        {
            // Utils.Log($"Cache({tfedUrl}): {e.GetType().Name} {e.Message}");
        }
        if (response is not null)
        {
            // _responses[urlSet.ApiUrl!] = response.Value;
            _ = Directory.CreateDirectory(tfedUrl.CacheFolder);
            File.WriteAllText($"{tfedUrl.CacheFilePath}.html", response);
        }
        else
        {
            // Utils.Log($"Cache({tfedUrl}): Failed to get response for {apiUrl}");
        }
        return response;
    }
    public override async Task<IEnumerable<string>?> GetTagsAsync(TransformedUrl tfedUrl)
    {
        // look at cached html
        // find tag info idk
        return null;
    }
}
