using d9.utl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        if(Uri.TryCreate(location, UriKind.Absolute, out Uri? uri))
        {
            if(uri.Scheme is "http" or "https")
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
                }
            } else if(uri.Scheme is "file")
            {
                string? hash = await location.FileHashAsync();
                if (hash is not null)
                {
                    return new(location, hash, LocationType.Internet);
                }
            }
            Utils.Log($"Error creating CandidateItem from location `{location}`: Unrecognized scheme {uri.Scheme}.");
        }
        Utils.Log($"Failed to create CandidateItem from location `{location}`.");
        return null;
    }
    public async Task Save()
    {
        if (File.Exists(Location))
        {
            _ = await ItemManager.CreateAndSave(Location, Hash);
        }
    }
}
