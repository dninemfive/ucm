namespace d9.ucm;
public partial class ThumbnailView : ContentView
{
	private Item? _item = null;
	public Item? Item
	{
		get => _item;
		set
		{
			_item = value;
            Thumbnail.Source = value?.LocalPath.Value;
            IsVisible = value is not null;
            // todo: how tf do you clear a tooltip
            ToolTipProperties.SetText(this, value?.ToString() ?? "");
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
    private double _size = -1;
    public double Size
    {
        get => _size;
        set
        {
            Thumbnail.HeightRequest = value;
            Thumbnail.WidthRequest = value;
            Button.HeightRequest = value;
            Button.WidthRequest = value;
        }
    }
    public Action? OnClick = null;
    public ThumbnailView()
    {
        InitializeComponent();
	}

    private void Thumbnail_Clicked(object sender, EventArgs e)
    {
        if (OnClick is not null)
            OnClick();
    }
}