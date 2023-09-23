namespace d9.ucm;

public partial class CollectionPage : ContentPage
{
	public CollectionPage()
	{
        InitializeComponent();
	}
    Competition? _competition = null;
    private List<Item>? _items = null;
    private bool _loading = false;
	public async void LoadItems(object? sender, EventArgs e)
	{
        if (_loading)
            return;
        _loading = true;
        LoadButton.Text = "Loading...";
        Competition? old = _competition;
        _competition = CompetitionSelector.Competition;
        if(_competition is null)
        {
            _items = await Task.Run(() => ItemManager.Items.OrderBy(x => x.Id).ToList());
        }
        if(_competition?.Name != old?.Name)
        {
            _items = await Task.Run(() => _competition?.RelevantItems.OrderByDescending(x => ratingof(x)?.CiLowerBound).ToList());
        }
        Competition.Rating? ratingof(Item item) => _competition?.RatingOf(item);        
        int horizontalItems = 7;
        for(int i = 0; i < 21; i++)
        {
            if (!_items!.Any())
                break;
            Item item = _items!.First();
            ItemsHolder.Add(new ThumbnailView(item, 1920 / horizontalItems - 10, $"{ratingof(item)}" ?? item.Id.ToString()));
            _items!.RemoveAt(0);
        }
        LoadButton.Text = "Load";
        _loading = false;
    }
}

