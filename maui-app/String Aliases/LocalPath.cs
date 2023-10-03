using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using d9.utl;

namespace d9.ucm;
[JsonConverter(typeof(LocalPathConverter))]
public class LocalPath : StringAlias
{
    public LocalPath(string s) : base(s) { }
    public bool Exists => File.Exists(Value);
    public bool IsInFolder(string folderPath) => Value.IsInFolder(folderPath);
}
public class LocalPathConverter : JsonConverter<LocalPath>
{
    public override LocalPath Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(reader.GetString()!);
    public override void Write(Utf8JsonWriter writer, LocalPath value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
    public override LocalPath ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(reader.GetString()!);
    public override void WriteAsPropertyName(Utf8JsonWriter writer, LocalPath value, JsonSerializerOptions options)
        => writer.WritePropertyName(value.Value);
}