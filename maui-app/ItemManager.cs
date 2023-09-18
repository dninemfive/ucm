using d9.utl;
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
    public static async void Load()
    {
        _dict = new();
        await foreach (Item item in Item.LoadAllAsync())
        {
            _dict[item.Id] = item;
        }
    }
    public static Item RandomItem => All.RandomElement();
}
