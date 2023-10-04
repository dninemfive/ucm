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
			TopRectangle.HeightRequest = HeightRequest * (1 - _ratio);
			BottomRectangle.HeightRequest = HeightRequest * _ratio;
		}
	}
	public Color TopColor { get => TopRectangle.BackgroundColor; set => TopRectangle.BackgroundColor = value; }
	public Color BottomColor { get => BottomRectangle.BackgroundColor; set => BottomRectangle.BackgroundColor = value; }
	public RatioBoxView()
	{
		InitializeComponent();
	}
}