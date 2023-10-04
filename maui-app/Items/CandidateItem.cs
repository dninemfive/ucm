using d9.utl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace d9.ucm;
public class CandidateItem
{
    public string Hash { get; private set; }
    public string CanonicalLocation { get; private set; }
    public ApiUrl? ApiUrl { get; private set; }
    public byte[]? Data { get; private set; }
    public string? SourceUrl { get; private set; }
    public enum LocationType
    {
        Internet,
        Local
    }
    public LocationType Type { get; private set; }
    private CandidateItem(string canonicalLocation, ApiUrl? apiUrl, string hash, LocationType type, string? sourceUrl = null, byte[]? data = null)
    {
        CanonicalLocation = canonicalLocation;
        ApiUrl = apiUrl;
        Hash = hash;
        Type = type;
        SourceUrl = sourceUrl;
        Data = data;
    }
    public static async Task<CandidateItem?> MakeFromAsync(string canonicalLocation)
    {
        string? uriScheme = canonicalLocation.UriScheme();
        if(uriScheme is "http" or "https")
        {
            PageUrl pageUrl = new(canonicalLocation);
            UrlRule? urlRule = UrlRule.BestFor(pageUrl);
            if (urlRule is null)
                return null;
            try
            {
                ApiUrl? apiUrl = UrlRule.BestApiUrlFor(pageUrl);                
                string? sourceUrl = await GetFileUrlAsync(pageUrl!);
                if(sourceUrl is null || sourceUrl.FileExtension() is ".mp4" or ".zip") return null;
                byte[] data = await MauiProgram.HttpClient.GetByteArrayAsync(sourceUrl);
                string? hash = await data.HashAsync();
                if(hash is not null)
                {
                    return new(canonicalLocation, apiUrl, hash, LocationType.Internet, sourceUrl, data);
                } 
            } catch(Exception e)
            {
                Utils.Log($"Error creating CandidateItem from location `{canonicalLocation}`: {e.Message}");
                return null;
            }
        } else if(uriScheme is "file")
        {
            string? hash = await canonicalLocation.FileHashAsync();
            if (hash is not null)
            {
                return new(canonicalLocation, null, hash, LocationType.Local);
            }
        }
        Utils.Log($"Error creating CandidateItem from location `{canonicalLocation}`: Unrecognized scheme {uriScheme.PrintNull()}.");
        return null;
    }
    public async Task<bool> SaveAsync()
    {
        Item? result = await Item.FromAsync(this);
        if(result is not null)
        {
            ItemManager.Register(result);
            await result.SaveAsync();
        }
        return result is not null;
    }
    public static async Task<string?> GetFileUrlAsync(PageUrl url)
        => await UrlRule.BestFileUrlFor(url);
    public override string ToString()
        => $"CI({CanonicalLocation}, {Hash}, {Type})";
}
