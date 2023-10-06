using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace d9.ucm;
[JsonConverter(typeof(ItemIdConverter))]
public readonly struct ItemId : IComparable<ItemId>
{
    [JsonIgnore]
    public const string ALPHABET = "0123456789ABCDEFGHJKLMNPQRTVWXYZ";
    [JsonIgnore]
    public static ulong Base => (ulong)ALPHABET.Length;    
    [JsonInclude]
    public ulong Value { get; }
    public string IntString => $"{Value}";
    [JsonConstructor]
    public ItemId(ulong value) { Value = value; }
    public static ItemId FromIntString(string str) => new(ulong.Parse(str));
    private static ulong ValueOf(char c) => (ulong)ALPHABET.IndexOf(c); 
    #region operators
    public static bool operator >(ItemId a, ItemId b) => a.Value > b.Value;
    public static bool operator <(ItemId a, ItemId b) => a.Value < b.Value;
    public static bool operator ==(ItemId a, ItemId b) => a.Value == b.Value;
    public static bool operator !=(ItemId a, ItemId b) => a.Value != b.Value;
    public static bool operator >=(ItemId a, ItemId b) => a > b || a == b;
    public static bool operator <=(ItemId a, ItemId b) => a < b || a == b;
    public static ItemId operator ++(ItemId id) => new(id.Value + 1);
    public static ItemId operator +(ItemId id, ulong val) => new(id.Value + val);
    #endregion
    #region overrides
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ItemId id && id == this;
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString()
    {
        if (Value == 0)
            return "0";
        ulong val = Value;
        string result = "";
        while (val > 0)
        {
            result = $"{ALPHABET[(int)(val % Base)]}{result}";
            val /= Base;
        }
        return result;
    }
    public int CompareTo(ItemId other) => Value.CompareTo(other.Value);
    #endregion
    #region implicit casts
    public static implicit operator ItemId(string s)
    {
        ulong result = 0, factor = 1;
        foreach (char c in s.Reverse())
        {
            result += factor * ValueOf(c);
            factor *= Base;
        }
        return result;
    }
    public static implicit operator ItemId(ulong ul) => new(ul);
    public static implicit operator ItemId(int z) => new((ulong)z);
    #endregion
}
// https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to?pivots=dotnet-7-0#sample-basic-converter
public class ItemIdConverter : JsonConverter<ItemId>
{
    public override ItemId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) 
        => ItemId.FromIntString(reader.GetString()!);
    public override void Write(Utf8JsonWriter writer, ItemId value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
    public override ItemId ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => ItemId.FromIntString(reader.GetString()!);
    public override void WriteAsPropertyName(Utf8JsonWriter writer, ItemId value, JsonSerializerOptions options)
        => writer.WritePropertyName(value.ToString());
}