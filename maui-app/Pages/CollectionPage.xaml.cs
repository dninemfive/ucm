using d9.utl;
using System.Security.Cryptography.X509Certificates;

namespace d9.ucm;

public partial class CollectionPage : ContentPage
{
	public CollectionPage()
	{
        InitializeComponent();
        CompetitionSelector.CompetitionSelected += CompetitionSelected;
        SizeChanged += PageSizedChanged;
        NavigationButtons.Navigated += GoToPage;
        TagSearchBar.TagSearchedFor += TagSearchBar_TagSearchedFor;
    }
    private async void TagSearchBar_TagSearchedFor(string tag, bool invert)
        => await LoadItems(delegate (Item item)
        {
            bool result = false;
            foreach(ItemSource source in item.Sources) if(source.Tags.Contains(tag))
                {
                    result = true;
                }
            return invert ? !result : result;
        });
    private async Task LoadItems(Func<Item, bool>? func = null)
    {
        func ??= (_) => true;
        ItemsHolder.Children.Clear();
        if (Competition is null)
        {
            _items = await Task.Run(() => ItemManager.Items.Where(func).OrderBy(x => x.Id).ToList());
        }
        else
        {
            _items = await Task.Run(() => ItemManager.Items.Where(func)
                                                           .OrderBy(x => Competition?.IsIrrelevant(x) ?? false)
                                                           .ThenByDescending(x => Competition?.RatingOf(x)?.CiLowerBound)
                                                           .ToList());
        }
        _pageIndex = 0;
        LoadPage();
        NavigationButtons.IsVisible = true;
        NavigationButtons.MaxPage = MaxPage;
    }
    public Competition? Competition => CompetitionSelector.Competition;
    private List<Item>? _items = null;
    private bool _loading = false;
    private static int _itemsPerPage = 36;
    private static List<(int a, int b)> _itemsPerPageFactors = _itemsPerPage.Factors().ToList();
    public static int ItemsPerPage
    {
        get => _itemsPerPage;
        set
        {
            _itemsPerPage = value;
            _itemsPerPageFactors = value.Factors().ToList();
        }
    }
    public async void CompetitionSelected(object? sender, EventArgs e)
        => await LoadItems();
    private int _pageIndex = 0;
    public int MaxPage => (int)Math.Ceiling((double)_items!.Count / ItemsPerPage);
    private void GoToPage(NavigationView.EventArgs page)
    {
        Utils.Log($"GoToPage({page})");
        if (page < 0 || page >= MaxPage)
            return;
        _pageIndex = page;
        LoadPage();
    }
    private void LoadPage()
    {
        Utils.Log($"LoadPage({_pageIndex})");
        if (_loading)
            return;
        _loading = true;
        int start = _pageIndex * ItemsPerPage;
        ItemsHolder.Clear();
        for (int i = start; i < start + ItemsPerPage; i++)
        {
            if(i >= _items!.Count) break;
            Item item = _items![i];
            ItemsHolder.Add(new ThumbnailView()
            {
                Item = item,
                OverlayText = $"{Competition?.RatingOf(item)}" ?? item.Id.ToString(),
                IsIrrelevant = Competition?.IsIrrelevant(item) ?? false,
                Size = 100
            });
        }
        CalculateItemSize();
        _loading = false;
    }
    private double _itemSize = 100;
    public double ItemSize
    {
        get => _itemSize;
        set
        {
            _itemSize = value;
            Utils.Log($"ItemSize = {value}");
            foreach(IView? item in ItemsHolder)
            {
                if(item is ThumbnailView thumbnail)
                {
                    thumbnail.Size = value;
                }
            }
        }
    }
    public (double width, double height) ItemSpace
        => (Width 
            - Grid.Padding.HorizontalThickness, 
            Height 
            - SearchTerms.HeightRequest 
            - NavigationButtons.HeightRequest 
            - Grid.Padding.VerticalThickness * 2);
    private void CalculateItemSize()
    {
        // calculate best size for current thumbnailviews
        // "best" being the size which leaves the least empty space
        // which i guess is the one with the least remainder for screen width / n or screen height / n
        // where 0 < n < ItemsPerScreen with an additional constraint based on screen proportions
        // probably constrain size to avoid items which are too large or small  
        Utils.Log($"ItemSpace: {ItemSpace}, {Grid.RowDefinitions[1].Height}");
        double smallSize = Math.Min(ItemSpace.width, ItemSpace.height),
               largeSize = Math.Max(ItemSpace.width, ItemSpace.height),
               ratio = largeSize / smallSize;
        (int a, int b) closestPair = _itemsPerPageFactors.First();
        double closestDiff = double.MaxValue;
        
        foreach((int a, int b) pair in _itemsPerPageFactors)
        {            
            double diff = Math.Abs(ratio - (pair.b / (double)pair.a));
            if (diff < closestDiff)
            {
                closestPair = pair;
                closestDiff = diff;
            }
        }
        double thumbnailMargin = 5 * 2; // todo: actually sync this with the value used
        smallSize -= thumbnailMargin * closestPair.a;
        largeSize -= thumbnailMargin * closestPair.b;
        double size1 = largeSize / closestPair.b, 
               size2 = smallSize / closestPair.a,
               d1 = smallSize - (size1 * closestPair.a), d2 = largeSize - (size2 * closestPair.b);
        ItemSize = (d1 < d2 ? size1 : size2) * 0.98;
        // Grid.RowDefinitions[1].Height = ItemSpace.height;
    }
    private void PageSizedChanged(object? sender, EventArgs e) => CalculateItemSize();
}

