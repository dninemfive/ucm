using d9.utl;
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
    public static IAsyncEnumerable<Item> GetItemsAsync() => Items.ToAsyncEnumerable();
    private static Dictionary<ItemId, Item>? _itemsById = null;
    public static IReadOnlyDictionary<ItemId, Item> ItemsById
    {
        get
        {
            if (_itemsById is null)
            {
                Utils.Log($"Attempted to access ItemsById before loading.");
                Load();
            }
            return _itemsById!;
        }
    }
    private static Dictionary<string, Item>? _itemsByHash = null;
    public static IReadOnlyDictionary<string, Item> ItemsByHash
    {
        get
        {
            if(_itemsByHash is null)
            {
                Utils.Log($"Attempted to access ItemsByHash before loading.");
                Load();
            }
            return _itemsByHash!;
        }
    }
    #endregion
    public static void Register(Item item)
    {
        _itemsById![item.Id] = item;
        _itemsByHash![item.Hash] = item;
    }
    public static void Load()
    {
        if (_loaded)
            return;
        _loaded = true;
        _itemsById = new();
        _itemsByHash = new();
        foreach (Item item in MauiProgram.TEMP_SAVE_LOCATION.LoadAll<Item>(x =>
        {
            (string src, Item item) = x;
            bool result = File.Exists(item.Path);
            if (!result)
            {
                src.MoveFileTo(Path.Join(src.DirectoryName(), "missing", src.FileName()));
            }
            return result;
        }))
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
    public static IEnumerable<string> AllLocations
    {
        get
        {
            foreach(Item item in Items)
            {
                foreach (string location in item.Locations)
                    yield return location;
            }
        }
    }
}
