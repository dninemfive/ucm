using d9.utl;
using Microsoft.UI.Xaml.Automation.Peers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d9.ucm;
public static class ItemManager
{
    public static IEnumerable<Item> All => Items.Values;
    private static Dictionary<ItemId, Item>? _dict = null;
    public static IReadOnlyDictionary<ItemId, Item> Items
    {
        get
        {
            if (_dict is null)
                throw new InvalidOperationException("Can't get items before loading them!");
            return _dict!;
        }
    }
    private static bool _loaded = false;
    public static void Load()
    {
        if (_loaded)
            return;
        _loaded = true;
        _dict = new();
        foreach (Item item in Item.LoadAll())
        {
            Register(item);
        }
    }
    public static void Reload()
    {
        _loaded = false;
        Load();
    }
    public static Item RandomItem => All.RandomElement();
    public static void Register(Item item) => _dict![item.Id] = item;
    public static async Task<Item> CreateAndSave(string path, string hash, ItemId? id = null)
    {
        Item item = id is null ? new(path, hash) : new(path, hash, id.Value);
        Register(item);
        await item.SaveAsync();
        return item;
    }
}
