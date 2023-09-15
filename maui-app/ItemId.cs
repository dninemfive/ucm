using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace d9.ucm;
public readonly struct ItemId
{
    [JsonIgnore]
    public const string ALPHABET = "0123456789ABCDEFGHJKLMNPQRTVWXYZ";
    [JsonIgnore]
    public static ulong Base => (ulong)ALPHABET.Length;
    private static ulong ValueOf(char c) => (ulong)ALPHABET.IndexOf(c);
    [JsonInclude]
    public ulong Value { get; }
    [JsonConstructor]
    public ItemId(ulong value) { Value = value; }
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
    #endregion
}