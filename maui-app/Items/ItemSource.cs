﻿using System;
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
    public List<Tag> Tags { get; private set; }
    public ItemSource(string sourceName, string location, params Tag[] tags)
    {
        SourceName = sourceName;
        Location = location;
        Tags = tags.ToList();
    }
    [JsonConstructor]
    public ItemSource(string sourceName, string location, List<Tag> tags)
    {
        SourceName = sourceName;
        Location = location;
        Tags = tags;
    }
}