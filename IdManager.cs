using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d9.ucm;
public readonly struct ItemId
{
    public const string ALPHABET = "0123456789ABCDEFGHJKLMNPQRTVWXYZ";
    public static ulong Base => (ulong)ALPHABET.Length;
    private static ulong ValueOf(char c) => (ulong)ALPHABET.IndexOf(c);
    private readonly ulong _value;
    public ItemId(ulong value) { _value = value; }
    public static bool operator >(ItemId a, ItemId b) => a._value > b._value;
    public static bool operator <(ItemId a, ItemId b) => a._value < b._value;
    public static ItemId operator ++(ItemId id) => new(id._value + 1);
    public static implicit operator string(ItemId id)
    {
        ulong val = id._value;
        string result = "";
        while(val > 0)
        {
            result = $"{ALPHABET[(int)(val % Base)]}{result}";
            val /= Base;
        }
        return result;
    }
    public static implicit operator ItemId(string s)
    {
        ulong result = 0, factor = 1;
        foreach(char c in s.Reverse())
        {
            result += factor * ValueOf(c);
            factor *= Base;
        }
        return result;
    }
    public static implicit operator ItemId(ulong ul) => new(ul);
}
public static class IdManager
{    
    private static ItemId _id = 0;
    public static ItemId NextId => _id++;
    public static void Register(ItemId id)
    {
        if (id > _id)
            _id = id;
    }
}