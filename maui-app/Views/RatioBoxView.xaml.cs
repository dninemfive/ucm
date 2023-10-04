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
			_ratio = value;
			Rectangle.HeightRequest = HeightRequest * _ratio;
			Utils.Log($"{HeightRequest} {Rectangle.HeightRequest}");
		}
	}
	public Color ForegroundColor { get => Rectangle.BackgroundColor; set => Rectangle.BackgroundColor = value; }
	public RatioBoxView()
	{
		InitializeComponent();
	}
}