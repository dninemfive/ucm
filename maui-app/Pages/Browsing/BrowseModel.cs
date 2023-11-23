using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Android.Content.ClipData;

namespace d9.ucm;
public class BrowseModel
{
    private int _itemsPerPage = 36;
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
    public List<(int a, int b)> ItemsPerPageFactors { get; private set; }
    public BrowseModel(int itemsPerPage)
    {
        ItemsPerPage = itemsPerPage;
    }
    public int MaxPage => (int)Math.Ceiling((double)_itemIds.Count / ItemsPerPage);
    public bool Invert { get; set; } = false;
    public List<SearchToken> SearchTokens { get; set; } = new();
    private bool MatchesSearch(Item item)
    {
        bool result = true;
        foreach (Func<Item, bool> match in SearchTokens)
            if (!match(item))
                result = false;
        return Invert ? !result : result;
    }
    private List<ItemId> _itemIds = new();
    public Func<IEnumerable<ItemId>, IEnumerable<ItemId>> SortOrder;
    /// <summary>
    /// Selects and sorts all items according to the specified <see cref="SearchCriteria"/> and <see cref="SortingCriteria"/>.
    /// </summary>
    /// <returns>The first page of results.</returns>
    public async Task Update()
    {
        _itemIds = await Task.Run(() => SortOrder(ItemManager.Items.Where(MatchesSearch).Select(x => x.Id)).ToList());
    }
    public async Task<IEnumerable<Item?>> GetPage(int pageIndex)
    {
        if(pageIndex < 0 || pageIndex >= MaxPage)
        {
            Utils.Log($"Tried to navigate to nonexistent page {pageIndex}.");
            return Enumerable.Empty<Item>();
        }
        return await Task.Run(() =>
        {
            List<Item?> result = new();
            foreach(ItemId id in _itemIds.Skip(pageIndex * ItemsPerPage).Take(ItemsPerPage))
            {
                result.Add(ItemManager.TryGetItemById(id));
            }
            return result;
        });
    }
}
