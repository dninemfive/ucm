using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace d9.ucm;
public class BrowseModel
{
    #region properties
    public bool InvertSearch { get; set; } = false;
    private List<ItemId> _itemIds = new();
    public IEnumerable<ItemId> ItemIds => _itemIds;
    private int _itemsPerPage = Constants.DefaultItemsPerPage;
    public int ItemsPerPage
    {
        get => _itemsPerPage;
        // todo: event telling the view to refresh when this is updated
        // (this requires an async thingy so maybe not?)
        set
        {
            if(value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "There must be at least 1 item per page!");
            }
            _itemsPerPage = value;
            ItemsPerPageFactors = value.Factors().ToList();
        }
    }
    public List<(int a, int b)> ItemsPerPageFactors { get; private set; } = Constants.DefaultItemsPerPage.Factors().ToList();
    public int MaxPage => (int)Math.Ceiling((double)_itemIds.Count / ItemsPerPage);
    public List<SearchToken> SearchTokens { get; set; } = new();
    public Func<IEnumerable<ItemId>, IEnumerable<ItemId>> SortOrder { get; set; } = (items) => items.Order();
    #endregion properties
    public BrowseModel() { }
    #region methods
    public async Task<IEnumerable<Item?>> GetPage(int pageIndex)
    {
        if (pageIndex < 0 || pageIndex >= MaxPage)
        {
            Utils.Log($"Tried to navigate to nonexistent page {pageIndex}.");
            return Enumerable.Empty<Item>();
        }
        return await Task.Run(() =>
        {
            List<Item?> result = new();
            foreach (ItemId id in _itemIds.Skip(pageIndex * ItemsPerPage).Take(ItemsPerPage))
            {
                result.Add(ItemManager.TryGetItemById(id));
            }
            return result;
        });
    }
    private async Task<bool> MatchesSearch(Item item)
    {
        bool result = true;
        foreach (Func<Item, Task<bool>> match in SearchTokens)
            if (!(await match(item)))
                result = false;
        return InvertSearch ? !result : result;
    }
    public async Task Update()
    {
        _itemIds = await Task.Run(() => SortOrder(ItemManager.NonHiddenItems.Where(async x => MatchesSearch(x)).Select(x => x.Id)).ToList());
    }
    #endregion methods
}
