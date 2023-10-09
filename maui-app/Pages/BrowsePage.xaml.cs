namespace d9.ucm;

public partial class BrowsePage : ContentPage
{
	private List<Item> Items = new();
	private int index = 0;
	public BrowsePage()
	{
		InitializeComponent();
        UpdateVisibility();
	}

    private void Previous(object sender, EventArgs e)
    {
		index--;
		if (index < 0)
			index = Items.Count - 1;
		ItemView.Item = Items[index];
    }
    private void Next(object sender, EventArgs e)
    {
		index++;
		if (index >= Items.Count)
			index = 0;
		ItemView.Item = Items[index];
    }
    private void UpdateVisibility()
    {
        Buttons.IsVisible = CompetitionSelector.Competition is not null;
        ItemView.IsVisible = CompetitionSelector.Competition is not null;
    }
    private void CompetitionSelector_CompetitionSelected(object sender, EventArgs e)
    {
        if(CompetitionSelector.Competition is null)
        {
            Items.Clear();
            ItemView.Item = null;
        } else
        {
            List<(Item item, Competition.Rating? rating)> pairs = ItemManager.Items.Zip(ItemManager.Items.Select(CompetitionSelector.Competition.RatingOf))
                                                                              .ToList();
            Items = pairs.Where(x => x.rating?.CiUpperBound > 0.7)
                         .WeightedShuffled(x => 1 / x.rating!.Weight)
                         .Select(x => x.item)
                         .ToList();
            ItemView.Item = Items.First();
        }
        
        UpdateVisibility();
    }
}