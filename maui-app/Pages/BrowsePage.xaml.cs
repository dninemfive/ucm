using d9.utl;
namespace d9.ucm;
public partial class BrowsePage : ContentPage
{
	private Cycle<Item> _items = new();
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
        ItemView.IItem = _items.PreviousItem();
    }
    private void Next(object sender, EventArgs e)
    {
        ItemView.IItem = _items.NextItem();
    }
    private void UpdateVisibility()
    {
        Buttons.IsVisible = Selected.Competition is not null;
        ItemView.IsVisible = Selected.Competition is not null;
    }
    private void CompetitionSelected(object sender, EventArgs e)
    {
        if(Selected.Competition is null)
        {
            _items.Clear();
            ItemView.IItem = null;
        } else
        {
            List<(Item item, Competition.Rating? rating)> pairs = ItemManager.Items.Zip(ItemManager.Items.Select(Selected.Competition.RatingOf))
                                                                                   .ToList();
            _items = new(pairs.Where(x => x.rating?.CiLowerBound > 0.9)
                              .Shuffled()
                              .Select(x => x.item));
            ItemView.IItem = _items.First();
        }        
        UpdateVisibility();
    }
}