using d9.utl;
namespace d9.ucm;
public partial class BrowsePage : ContentPage
{
	private Cycle<Item> Items = new();
	public BrowsePage()
	{
		InitializeComponent();
        UpdateVisibility();
        for(int i = -10; i < 21; i++)
        {
            Utils.Log($"{i,-3} % 10 == {i % 10}");
        }
	}

    private void Previous(object sender, EventArgs e)
    {
        ItemView.Item = Items.PreviousItem();
    }
    private void Next(object sender, EventArgs e)
    {
        ItemView.Item = Items.NextItem();
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
            Items = new(pairs.Where(x => x.rating?.CiUpperBound > 0.7)
                             .WeightedShuffled(x => 1 / x.rating!.Weight)
                             .Select(x => x.item));
            ItemView.Item = Items.First();
        }
        
        UpdateVisibility();
    }
}