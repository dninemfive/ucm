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
            bool result = item.LocalPath.Exists;
            if (!result)
            {
                src.MoveFileTo(Path.Join(src.DirectoryName(), "missing", src.FileName()));
            }
            return result;
        }))
        {
            Register(item);
        }
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
                string localPath = item.LocalPath.Value;
                yield return localPath;
                foreach (string location in item.Locations)
                    if(location != localPath) yield return location;
            }
        }
    }
    public static async Task<bool> TryUpdateAnyMatchingItemAsync(CandidateItem? ci)
    {
        if (ci is null)
            return true;
        if(ItemsByHash.TryGetValue(ci.Hash, out Item? item) && !item.HasSourceInfoFor(ci.Location))
        {
            if (item.LocalPath.IsInFolder(MauiProgram.ITEM_FILE_LOCATION))
                return true;
            ItemSource? source = await ci.GetItemSourceAsync();
            if (source is not null)
            {
                item.Sources.Add(source);
                await item.SaveAsync();
            }
            return true;
        }
        return false;
    }
}
