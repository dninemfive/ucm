﻿using d9.utl;
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
    public static IEnumerable<Item> NonHiddenItems => AllItems.Where(x => !x.Hidden);
    public static IEnumerable<Item> AllItems => ItemsById.Values;
    public static IAsyncEnumerable<Item> GetItemsAsync() => NonHiddenItems.ToAsyncEnumerable();
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
    private static Dictionary<string, ItemId>? _itemsByHash = null;
    public static bool TryGetItemByHash(string hash, out Item? result)
    {
        if (_itemsByHash is null)
        {
            Utils.Log($"Attempted to access ItemsByHash before loading.");
            Load();
        }
        if(_itemsByHash!.TryGetValue(hash, out ItemId id))
        {
            return _itemsById!.TryGetValue(id, out result);
        }
        result = null;
        return false;
    }
    #endregion
    public static void Register(Item item)
    {
        _itemsById![item.Id] = item;
        _itemsByHash![item.Hash] = item.Id;
    }
    public static void Load()
    {
        if (_loaded)
            return;
        _loaded = true;
        _itemsById = new();
        _itemsByHash = new();
        foreach (Item item in Constants.Folders.TEMP_Data.LoadAll<Item>(x =>
        {
            // todo: check if item hash matches current hash
            // if not, update but log
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
    public static async Task<HashSet<string>> GetAllLocationsAsync()
    {
        HashSet<string> result = new();
        foreach (Item item in ItemsById.Values)
        {
            string localPath = item.LocalPath.Value;
            result.Add(localPath);
            foreach (string location in await item.GetLocationsAsync())
            {
                result.Add(location);
            }
        }
        return result;
    }
    public static async Task<bool> TryUpdateAnyMatchingItemAsync(CandidateItem? ci)
    {
        if (ci is null)
        {
            return true;
        }
        if(TryGetItemByHash(ci.Hash, out Item? item) && !(await item!.HasSourceInfoFor(ci.Location)))
        {
            await item.AddSource(ci.Source);
            // Utils.Log($"TryUpdateAnyMatchingItemAsnyc({ci}) -> {item}: {item.Sources.ListNotation()}");
            await item.SaveAsync();
            return true;
        }
        return false;
    }
    public static Item? TryGetItemById(ItemId? id, bool returnHidden = false)
    {
        if(id is not null && ItemsById.TryGetValue(id.Value, out Item? item) && (returnHidden || !item.Hidden))
        {
            return item;
        }
        return null;
    }
}
