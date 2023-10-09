using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace d9.ucm;

public class ItemSource
{
    [JsonInclude]
    public string Location { get; private set; }
    [JsonInclude]
    public string SourceName { get; private set; }
    [JsonInclude]
    public List<string> Tags { get; private set; }
    [JsonInclude]
    public DateTime AcquiredAt { get; private set; }
    public ItemSource(string sourceName, string location, params string[] tags)
    {
        SourceName = sourceName;
        Location = location;
        Tags = tags.ToList();
        AcquiredAt = DateTime.Now;
    }
    [JsonConstructor]
    public ItemSource(string sourceName, string location, List<string> tags, DateTime acquiredAt)
    {
        SourceName = sourceName;
        Location = location;
        Tags = tags;
        AcquiredAt = acquiredAt;
    }
    [JsonIgnore]
    public string LabelText
        => $"Source: {Location} ({SourceName})\nAcquired at: {AcquiredAt:g}{Tags.AsBulletedList()}";
}