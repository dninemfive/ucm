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
    public string Location { get; private set; }
    public enum LocationType
    {
        Internet,
        Local
    }
    public LocationType Type { get; private set; }
    private CandidateItem(string path, string hash, LocationType type)
    {
        Hash = hash;
        Location = path;
        Type = type;
    }
    public static async Task<CandidateItem?> MakeFromAsync(string location)
    {
        string? uriScheme = location.UriScheme();
        if(uriScheme is "http" or "https")
        {
            try
            {
                byte[] data = await MauiProgram.HttpClient.GetByteArrayAsync(location);
                string? hash = await data.HashAsync();
                if(hash is not null)
                {
                    return new(location, hash, LocationType.Internet);
                } 
            } catch(Exception e)
            {
                Utils.Log($"Error creating CandidateItem from location `{location}`: {e.Message}");
                return null;
            }
        } else if(uriScheme is "file")
        {
            string? hash = await location.FileHashAsync();
            if (hash is not null)
            {
                return new(location, hash, LocationType.Local);
            }
        }
        Utils.Log($"Error creating CandidateItem from location `{location}`: Unrecognized scheme {uriScheme.PrintNull()}.");
        return null;
    }
    public async Task<bool> SaveAsync()
    {
        Item? result = Item.From(this);
        if(result is not null)
        {
            ItemManager.Register(result);
            await result.SaveAsync();
        }
        return result is not null;
    }
    public override string ToString()
        => $"CI({Location}, {Hash}, {Type})";
}
