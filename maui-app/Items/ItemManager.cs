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
    private static bool _loaded = false;
    #region properties
    public static IEnumerable<Item> Items => ItemsById.Values;
    private static Dictionary<ItemId, Item>? _itemsById = null;
    public static IReadOnlyDictionary<ItemId, Item> ItemsById
    {
        get
        {
            if (_itemsById is null)
                throw new InvalidOperationException("Can't get items before loading them!");
            return _itemsById!;
        }
    }
    public static Item RandomItem => Items.RandomElement();
    #endregion
    public static void Register(Item item) => _itemsById![item.Id] = item;
    public static void Load()
    {
        if (_loaded)
            return;
        _loaded = true;
        _itemsById = new();
        foreach (Item item in Item.LoadAll())
        {
            Register(item);
        }
        Utils.Log($"Loaded {_itemsById.Count} items.");
    }
    public static void Reload()
    {
        _loaded = false;
        Load();
    }
    public static async Task<Item> CreateAndSave(string path, string hash, ItemId? id = null)
    {
        Item item = id is null ? new(path, hash) : new(path, hash, id.Value);
        Register(item);
        await item.SaveAsync();
        return item;
    }
}
