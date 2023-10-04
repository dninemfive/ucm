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
			TopRectangle.WidthRequest = WidthRequest;
			BottomRectangle.HeightRequest = HeightRequest * _ratio;
			TopRectangle.HeightRequest = HeightRequest - BottomRectangle.HeightRequest;
            BottomRectangle.WidthRequest = WidthRequest;
			Utils.Log($"{HeightRequest}\t{TopRectangle.HeightRequest}\t{TopRectangle.HeightRequest}\t{BottomRectangle.HeightRequest}\t{TopRectangle.HeightRequest + BottomRectangle.HeightRequest}");
		}
	}
	public Color ForegroundColor { get => BottomRectangle.BackgroundColor; set => BottomRectangle.BackgroundColor = value; }
	public RatioBoxView()
	{
		InitializeComponent();
	}
}