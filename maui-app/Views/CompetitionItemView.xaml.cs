namespace d9.ucm;

public partial class CompetitionItemView : ContentView
{
	public Item Item { get; private set; }
	private bool _isIrrelevant = false;
	public bool IsIrrelevant
	{
		get => _isIrrelevant;
		set
		{
			_isIrrelevant = value;
			ItemHolder.Content.Opacity = _isIrrelevant ? 0.42 : 1;
		}
	}
	public CompetitionItemView()
	{
		InitializeComponent();
	}
	public void UpdateWith(Item item, string? extraTooltipInfo, bool isIrrelevant = false)
	{
		Item = item;
		ItemHolder.Content = item.View;
		if (extraTooltipInfo is not null)
			extraTooltipInfo = $"\n\n{extraTooltipInfo}";
		ToolTipProperties.SetText(ItemHolder, $"Item {item.Id} @ {item.Path}{extraTooltipInfo}");
		IsIrrelevant = isIrrelevant;
    }
}