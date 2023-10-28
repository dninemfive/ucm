using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace d9.ucm;
[JsonConverter(typeof(UrlPatternConverter))]
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
    public static implicit operator UrlPattern(string s) => new(s);
}
public class UrlPatternConverter : JsonConverter<UrlPattern>
{
    public override UrlPattern Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.GetString()!;
    public override void Write(Utf8JsonWriter writer, UrlPattern value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
    public override UrlPattern ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.GetString()!;
    public override void WriteAsPropertyName(Utf8JsonWriter writer, UrlPattern value, JsonSerializerOptions options)
        => writer.WritePropertyName(value.ToString());
}
