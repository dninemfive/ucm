namespace d9.ucm;
public partial class RatioBoxView : ContentView
{
	private double _ratio = 0.5;
	public double Ratio 
	{ 
		get => _ratio; 
		set
		{
			if (value is < 0 or > 1)
				throw new ArgumentOutOfRangeException(nameof(value), 
													  "The ratio in a RatioBoxView must be between 0 and 1, inclusive.");
			IsVisible = value > 0;
			_ratio = value;
			Utils.Log($"set_Ratio({value})");
			Rectangle.HeightRequest = HeightRequest * _ratio;
			Utils.Log($"\t{HeightRequest} {Rectangle.HeightRequest}");
			Rectangle.WidthRequest = WidthRequest;
		}
	}
	public Color ForegroundColor { get => Rectangle.BackgroundColor; set => Rectangle.BackgroundColor = value; }
	public new Color BackgroundColor { get => BackgroundRectangle.BackgroundColor; set => BackgroundRectangle.BackgroundColor = value; }
	public RatioBoxView()
	{
		InitializeComponent();
	}
	public void SetTooltip(string tooltip)
	{
		ToolTipProperties.SetText(Rectangle, "rectangle");
		ToolTipProperties.SetText(BackgroundRectangle, "background");
	}
}