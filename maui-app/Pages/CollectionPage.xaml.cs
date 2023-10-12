using d9.utl;
using ModelIO;

namespace d9.ucm;

public partial class CollectionPage : ContentPage
{
	public CollectionPage()
	{
        InitializeComponent();
        CompetitionSelector.CompetitionSelected += CompetitionSelected;
	}
    public Competition? Competition => CompetitionSelector.Competition;
    private List<Item>? _items = null;
    private bool _loading = false;
    public static int ItemsPerPage { get; private set; } = 28;
    public async void CompetitionSelected(object? sender, EventArgs e)
    {
        ItemsHolder.Children.Clear();
        if(Competition is null)
        {
            _items = await Task.Run(() => ItemManager.Items.OrderBy(x => x.Id).ToList());
        } 
        else
        {
            _items = await Task.Run(() => Competition?.RelevantItems.OrderBy(x => Competition?.IsIrrelevant(x) ?? false)
                                                                    .ThenByDescending(x => Competition?.RatingOf(x)?.CiLowerBound)
                                                                    .ToList());
        }
        LoadItems(sender, e);
    }
    // todo: paginate, then load items for current page
	public void LoadItems(object? sender, EventArgs e)
	{
        if (_loading)
            return;
        _loading = true;
        double totalHeight = 0, targetHeight = ScrollView.ScrollSpace() <= 0 ? ScrollView.Height : 1;
        while(totalHeight <= targetHeight)
        {
            LoadOneRow();
            totalHeight += ITEM_SIZE;
        }        
        _loading = false;
    }
    private int _pageIndex = 0;
    public int PageIndex => _pageIndex;
    public async void GoToPage(int page)
    {
        if (page < 0 || page > _items!.Count / ItemsPerPage)
            return;

    }
    private async Task LoadPage()
    {
        int start = _pageIndex * ItemsPerPage;
        ItemsHolder.Clear();
        for (int i = start; i < start + ItemsPerPage; i++)
        {
            if(i > _items!.Count) break;
            Item item = _items![i];
            await Task.Run(() => ItemsHolder.Add(new ThumbnailView()
            {
                Item = item,
                OverlayText = $"{Competition?.RatingOf(item)}" ?? item.Id.ToString(),
                IsIrrelevant = Competition?.IsIrrelevant(item) ?? false,
                Size = 100
            }));
            _items!.RemoveAt(0);
        }
    }
    private void ScrollView_Scrolled(object sender, ScrolledEventArgs e)
    {
        if (e.ScrollY >= ScrollView.ScrollSpace())
            LoadItems(sender, e);
    }
    private double _itemSize = 100;
    public double ItemSize
    {
        get => _itemSize;
        set
        {
            _itemSize = value;
            foreach(IView? item in ItemsHolder)
            {
                if(item is ThumbnailView thumbnail)
                {
                    thumbnail.Size = value;
                }
            }
        }
    }
    private void ItemsHolder_SizeChanged(object sender, EventArgs e)
    {
        // calculate best size for current thumbnailviews
        // "best" being the size which leaves the least empty space
        // which i guess is the one with the least remainder for screen width / n or screen height / n
        // where 0 < n < ItemsPerScreen with an additional constraint based on screen proportions
        // set all views to this size
    }
}

