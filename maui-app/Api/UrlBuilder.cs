﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace d9.ucm;
public class UrlPattern
{
    [JsonInclude]
    public string Pattern { get; private set; }
    [JsonConstructor]
    public UrlPattern(string pattern)
    {
        Pattern = pattern;
    }
    public string? For(UrlInfoSet? info)
    {
        if (info is null)
            return null;
        string result = Pattern;
        foreach((string key, string? value) in info)
        {
            result = result.Replace($"{{{key}}}", value);
        }
        return result;
    }
}
