﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using d9.utl;

namespace d9.ucm;
/// <summary>
/// The base class for the concept of taking a url which has been parsed for information and getting the information required to present
/// the file, and if so desired download and acquire metadata for the file.
/// </summary>
public abstract class ApiDef
{
    public static readonly Dictionary<string, Type> Types = new()
    {
        { "JsonApiDef", typeof(JsonApiDef) },
        { "ScraperApiDef", typeof(ScraperApiDef) }
    };
    public abstract string ApiUrlKey { get; protected set; }
    public abstract Task<string?> GetFileUrlAsync(TransformedUrl tfedUrl);
    public abstract Task<IEnumerable<string>?> GetTagsAsync(TransformedUrl tfedUrl);
    public ApiDef(Dictionary<string, string> args) { }
}