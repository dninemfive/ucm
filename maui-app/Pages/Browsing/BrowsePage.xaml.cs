using d9.utl;

namespace d9.ucm;

public partial class BrowsePage : ContentPage
{
    public BrowseModel Model;
    public BrowsePage()
    {
        InitializeComponent();
        CompetitionSelector.CompetitionSelected += CompetitionSelected;
        SizeChanged += PageSizedChanged;
        NavigationButtons.Navigated += GoToPage;
        TagSearchBar.TagSearchedFor += TagSearchBar_TagSearchedFor;
        Model = new();
    }
    private void TagSearchBar_TagSearchedFor(IEnumerable<SearchToken> tokens, bool invert)
    {
        Model.SearchTokens = tokens.ToList();
        Model.InvertSearch = invert;
    }
    private async Task LoadItems(Func<Item, bool>? func = null)
    {
        func ??= (_) => true;
        ItemsHolder.Children.Clear();
        Func<IEnumerable<ItemId>, IEnumerable<ItemId>> sortOrder = Competition is null ?
            (x) => x.Order()
            : (x) => x.OrderBy(y => Competition?.IsIrrelevant(y) ?? false).ThenByDescending(y => Competition?.RatingOf(y)?.CiLowerBound);
        Model.SortOrder = sortOrder;
        await Model.Update();
        await LoadPage(0);
        NavigationButtons.IsVisible = true;
        NavigationButtons.MaxPage = Model.MaxPage;
    }
    public Competition? Competition => CompetitionSelector.Competition;
    private bool _loading = false;
    public async void CompetitionSelected(object? sender, EventArgs e)
        => await LoadItems();
    private async void GoToPage(NavigationView.EventArgs page)
    {
        Utils.Log($"GoToPage({page})");
        await LoadPage(page);
    }
    private ThumbnailView AddItem(Item? item, int _)
    {
        ThumbnailView thumbnail = new()
        {
            Item = item,
            OverlayText = $"{Competition?.RatingOf(item)}" ?? "",
            IsIrrelevant = item is not null && (Competition?.IsIrrelevant(item) ?? false),
            Size = 100
        };
        ItemsHolder.Add(thumbnail);
        return thumbnail;
    }
    private ThumbnailView UpdateItem(Item? item, int index)
    {
        ThumbnailView thumbnail = (ItemsHolder[index] as ThumbnailView)!;
        thumbnail.Item = item;
        if (item is not null)
        {
            thumbnail.OverlayText = $"{Competition?.RatingOf(item)?.ToString() ?? ""}";
            thumbnail.IsIrrelevant = Competition?.IsIrrelevant(item) ?? false;
        }
        else
        {
            thumbnail.OverlayText = null;
            thumbnail.IsIrrelevant = false;
        }
        return thumbnail;
    }
    private async Task LoadPage(int pageNumber)
    {
        Utils.Log($"LoadPage({pageNumber})");
        if (_loading)
            return;
        _loading = true;
        int index = 0;
        bool initializing = !ItemsHolder.Any();
        Func<Item?, int, ThumbnailView> addOrUpdate = initializing ? AddItem : UpdateItem;
        foreach(Item? item in await Model.GetPage(pageNumber))
        {
            ThumbnailView thumbnail = addOrUpdate(item, index++);
            if (Competition is not null && item is not null)
            {
                thumbnail.OnClick = async () =>
                {
                    Competition.ToggleIrrelevant(item.Id);
                    await Competition.SaveAsync();
                    thumbnail.IsIrrelevant = !thumbnail.IsIrrelevant;
                };
            }
            else
            {
                thumbnail.OnClick = null;
            }
        }
        if (initializing)
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
        double smallSize = Math.Min(ItemSpace.width, ItemSpace.height),
               largeSize = Math.Max(ItemSpace.width, ItemSpace.height),
               ratio = largeSize / smallSize;
        (int a, int b) closestPair = Model.ItemsPerPageFactors.First();
        double closestDiff = double.MaxValue;        
        foreach((int a, int b) pair in Model.ItemsPerPageFactors.Skip(1))
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
        ItemSize = (d1 < d2 ? size1 : size2) * 1;
    }
    private void PageSizedChanged(object? sender, EventArgs e) => CalculateItemSize();
}

