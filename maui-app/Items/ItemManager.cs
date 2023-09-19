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
    public static IEnumerable<Item> All => Items.Values;
    private static Dictionary<ItemId, Item>? _items = null;
    public static IReadOnlyDictionary<ItemId, Item> Items
    {
        get
        {
            if (_items is null)
                throw new InvalidOperationException("Can't get items before loading them!");
            return _items!;
        }
    }
    public static Item RandomItem => All.RandomElement();
    #endregion
    public static void Register(Item item) => _items![item.Id] = item;
    public static void Load()
    {
        if (_loaded)
            return;
        _loaded = true;
        _items = new();
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
    public static Item RandomItemWhere(Func<Item, bool> predicate) => All.Where(predicate).RandomElement();    
    public static async Task<Item> CreateAndSave(string path, string hash, ItemId? id = null)
    {
        Item item = id is null ? new(path, hash) : new(path, hash, id.Value);
        Register(item);
        await item.SaveAsync();
        return item;
    }
}
