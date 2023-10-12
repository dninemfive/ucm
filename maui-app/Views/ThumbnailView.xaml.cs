namespace d9.ucm;
public partial class ThumbnailView : ContentView
{
	private Item _item;
	public Item Item
	{
		get => _item;
		set
		{
			_item = value;
            if (value.Thumbnail is not null)
            {
                Thumbnail.Source = value.LocalPath.Value;         
            }
            else
            {
                Label.IsVisible = true;
                Label.Text = "invalid item";
                Label.VerticalOptions = LayoutOptions.Center;
                Label.HorizontalOptions = LayoutOptions.Center;
                Label.TextColor = Colors.Red;
            }
            ToolTipProperties.SetText(this, value);
        }
	}
    private string? _overlayText = null;
    public string? OverlayText
    {
        get => _overlayText;
        set
        {
            _overlayText = value;
            Label.Text = value;     
            Label.IsVisible = !string.IsNullOrEmpty(value);
        }
    }
    private bool _isIrrelevant = false;
    public bool IsIrrelevant
    {
        get => _isIrrelevant;
        set
        {
            _isIrrelevant = value;
            Thumbnail.Opacity = value ? 0.42 : 1;
        }
    }
#pragma warning disable CS8618 // "_item must be non-null": get_Item sets it properly
    public ThumbnailView(Item item, double size = 100, object? sortItem = null, bool isIrrelevant = false)
#pragma warning restore CS8618
    {
        InitializeComponent();
		Thumbnail.HeightRequest = size;
		Thumbnail.WidthRequest = size;
        Item = item;
        OverlayText = sortItem?.ToString();
        IsIrrelevant = isIrrelevant;
	}
}