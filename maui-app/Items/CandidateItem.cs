using d9.utl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace d9.ucm;
public class CandidateItem : IItemViewable
{
    public View View => throw new NotImplementedException();
    public IEnumerable<ItemSource> ItemSources => throw new NotImplementedException();
    public string Hash { get; private set; }
    public UrlSet? UrlSet { get; private set; }
    public byte[]? Data { get; private set; }
    public string? SourceUrl { get; private set; }
    public string? LocalPath { get; private set; }
    public string Location => (LocalPath ?? UrlSet?.CanonUrl)!;
    private CandidateItem(string hash, string? sourceUrl = null, byte[]? data = null)
    {
        Hash = hash;
        SourceUrl = sourceUrl;
        Data = data;
    }
    private CandidateItem(UrlSet urlSet, string hash, string? sourceUrl = null, byte[]? data = null) 
        : this(hash, sourceUrl, data)
    {
        UrlSet = urlSet;        
    }
    private CandidateItem(string localPath, string hash, string? sourceUrl = null, byte[]? data = null)
        : this(hash, sourceUrl, data)
    {
        LocalPath = localPath;
    }
    public static async Task<CandidateItem?> MakeFromAsync(string location)
    {
        string? uriScheme = location.UriScheme();
        if(uriScheme is "http" or "https")
        {
            UrlSet? urlSet = UrlSet.From(location);
            if (!urlSet.IsFullyValid())
                return null;
            try
            {             
                string? sourceUrl = await GetFileUrlAsync(urlSet!);
                if(sourceUrl is null || sourceUrl.FileExtension() is ".mp4" or ".zip") return null;
                byte[] data = await MauiProgram.HttpClient.GetByteArrayAsync(sourceUrl);
                string? hash = await data.HashAsync();
                if(hash is not null)
                {
                    return new(urlSet!, hash, sourceUrl, data);
                } 
            } catch(Exception e)
            {
                Utils.Log($"Error creating CandidateItem from location `{location}`: {e.Message}");
                return null;
            }
        } else if(uriScheme is "file")
        {
            if (!File.Exists(location))
                return null;
            string? hash = await location.FileHashAsync();
            if (hash is not null)
            {
                return new(localPath: location, hash);
            }
        }
        Utils.Log($"Error creating CandidateItem from location `{location}`: Unrecognized scheme {uriScheme.PrintNull()}.");
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
    public static async Task<string?> GetFileUrlAsync(UrlSet urlSet)
        => await urlSet.UrlRule.FileUrlFor(urlSet);
    public override string ToString()
        => $"CI {Hash} @ {Location}";
    public async Task<ItemSource?> GetItemSourceAsync()
    //    => File.Exists(location) ? new("Local Filesystem", location) : await UrlRule.BestItemSourceFor(location);
    {
        if (LocalPath is not null)
            return new("Local Filesystem", LocalPath);
        else
            return new(UrlSet!.UrlRule.Name, UrlSet!.CanonUrl!, (await UrlSet.UrlRule.TagsFor(UrlSet))?.ToArray() ?? Array.Empty<string>());
    }
}
