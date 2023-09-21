namespace d9.ucm;

public partial class CollectionPage : ContentPage
{
	public CollectionPage()
	{
        InitializeComponent();
	}
    Competition? competition = null;
    private List<Item>? _items = null;
    private int _index = 0;
	public async void LoadItems(object? sender, EventArgs e)
	{
        Competition? old = competition;
        competition = CompetitionSelector.Competition;
        if(competition?.Name != old?.Name)
        {
            _index = 0;
            _items = await Task.Run(() => competition?.RelevantItems.OrderByDescending(x => ratingof(x)?.CiLowerBound).ToList()
                                                ?? ItemManager.Items.OrderBy(x => x.Id).ToList());
        }
        Competition.Rating? ratingof(Item item) => competition?.RatingOf(item);        
        int horizontalItems = 7;
        foreach (Item item in _items!.Skip(_index))
        {
            if (_index++ % 21 == 0)
                break;
            ItemsHolder.Add(new ThumbnailView(item, 1920 / horizontalItems - 10, $"{ratingof(item)}" ?? item.Id.ToString()));
        }
    }
}

