using d9.utl;

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
    public const int ITEMS_PER_ROW = 7;
    public const double ITEM_SIZE = 1920 / (double)ITEMS_PER_ROW - 10;
    public async void CompetitionSelected(object? sender, EventArgs e)
    {
        ItemsHolder.Children.Clear();
        if(Competition is null)
        {
            _items = await Task.Run(() => ItemManager.Items.OrderBy(x => x.Id).ToList());
        } 
        else
        {
            _items = await Task.Run(() => Competition?.RelevantItems.OrderByDescending(x => Competition?.RatingOf(x)?.CiLowerBound).ToList());
        }
        LoadItems(sender, e);
    }
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
    private void LoadOneRow()
    {
        for (int i = 0; i < ITEMS_PER_ROW; i++)
        {
            Utils.Log($"Item {i,-2}:\tScrollSpace\t{ScrollView.ScrollSpace()}");
            if (!_items!.Any())
                break;
            Item item = _items!.First();
            ItemsHolder.Add(new ThumbnailView(item, ITEM_SIZE, $"{Competition?.RatingOf(item)}" ?? item.Id.ToString()));
            _items!.RemoveAt(0);
        }
    }
    private void ScrollView_Scrolled(object sender, ScrolledEventArgs e)
    {
        Utils.Log(ScrollView.ScrollSpace());
        if (e.ScrollY >= ScrollView.ScrollSpace())
            LoadItems(sender, e);
    }
}

