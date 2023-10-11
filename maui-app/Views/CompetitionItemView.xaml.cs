namespace d9.ucm;

public partial class CompetitionItemView : ContentView
{
	public Item? Item { get; private set; }
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
	public Competition? Competition { get; set; } = null;
	public CompetitionItemView()
	{
		InitializeComponent();
	}
	public void UpdateWith(Item item, string? extraTooltipInfo, bool isIrrelevant = false)
	{
		Item = item;
		ItemHolder.Content = item.View;
		ItemHolder.WidthRequest = WidthRequest;
		ItemHolder.HeightRequest = HeightRequest - IrrelevantButton.HeightRequest - SelectButton.HeightRequest;
		if (extraTooltipInfo is not null)
			extraTooltipInfo = $"\n\n{extraTooltipInfo}";
		ToolTipProperties.SetText(ItemHolder, $"{item}{extraTooltipInfo}");
		IsIrrelevant = isIrrelevant;
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