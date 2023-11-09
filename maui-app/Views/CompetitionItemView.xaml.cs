namespace d9.ucm;

public partial class CompetitionItemView : ContentView
{
	public Item? Item => Competition?[Side];
	public Side Side { get; set; }
	private Competition? _competition = null;
    public Competition? Competition
	{
		get => _competition;
		set
		{
			if(value is not null)
			{
				// todo: ensure that this is subscribed to exactly one competition's event exactly once
				value.ItemsUpdated += (sender, e) => Update();
			}
			_competition = value;
		}
	}
    private bool _isIrrelevant = false;
	public bool IsIrrelevant
	{
		get => _isIrrelevant;
		set
		{
			_isIrrelevant = value;
			ItemHolder.Content.Opacity = value ? 0.42 : 1;			
			IrrelevantButton.Text = value ? "Mark relevant" : "Mark irrelevant";
            SelectButton.IsEnabled = !value && Selectable;
        }
	}
	private bool _selectable = true;
	public bool Selectable
	{
		get => _selectable;
		set
		{
			_selectable = value;
			SelectButton.IsEnabled = value;
		}
	}
	public CompetitionItemView()
	{
		InitializeComponent();
	}
	public void Update()
	{
		ItemHolder.Content = Item?.View;
		ItemHolder.WidthRequest = WidthRequest;
		ItemHolder.HeightRequest = HeightRequest - IrrelevantButton.HeightRequest - SelectButton.HeightRequest;
		ToolTipProperties.SetText(ItemHolder, $"{Item}\n\n{Competition?.RatingOf(Item)}");
    }
	public event EventHandler? IrrelevantButtonClicked;
    private void IrrelevantButton_Clicked(object sender, EventArgs e)
    {
		IsIrrelevant = !IsIrrelevant;
		Competition!.SetIrrelevant(Item?.Id, IsIrrelevant);
		IrrelevantButtonClicked?.Invoke(sender, e);
    }
    public event EventHandler? SelectButtonClicked;
    private void SelectButton_Clicked(object sender, EventArgs e)
    {
		Competition!.Choose(Item?.Id);
        SelectButtonClicked?.Invoke(sender, e);
    }
}