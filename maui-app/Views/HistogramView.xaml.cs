using Microsoft.Maui.Controls.Shapes;
using System.Numerics;

namespace d9.ucm;

public partial class HistogramView : ContentView
{
	private readonly List<double> _data = new();
	public IEnumerable<double> Data => _data;
	public Color ForegroundColor { get; set; } = Colors.White;
	public int? ForcedLowerBound { get; set; } = null;
	public int? ForcedUpperBound { get; set; } = null;
	public HistogramView()
	{
		InitializeComponent();
	}
	public void AddData(params double[] data)
	{
		foreach (double datum in data)
			_data.Add(datum); 
		Update();
	}
	public void AddData(IEnumerable<double> data) => AddData(data.ToArray());
	public void Clear()
    {
		_data.Clear();
		Update();
    }
	public void ReplaceData(IEnumerable<double> newData)
	{
		_data.Clear();
		AddData(newData);
	}
	public void Update()
	{
        Container.Children.Clear();
        if (!_data.Any())
			return;
		CountingDictionary<int, int> counter = new();
		foreach(double datum in _data)
		{
			counter.Increment((int)datum);
		}
        int lowerBound = ForcedLowerBound ?? counter.Keys.Min(), upperBound = ForcedUpperBound ?? counter.Keys.Max();
		double maxValue = counter.Values.Max(),
			   bins = Math.Abs(upperBound - lowerBound) + 1,
			   height = HeightRequest,
			   width = Math.Max(WidthRequest / bins, 10);
		for(int i = lowerBound; i <= upperBound; i++)
		{
			RatioBoxView box = new()
			{
				WidthRequest = width,
				HeightRequest = height,
				ForegroundColor = ForegroundColor,
				BackgroundColor = Colors.Magenta,
				Margin = new(1, 0),
				Ratio = counter[i] / maxValue
			};
			ToolTipProperties.SetText(box, $"{i}: {counter[i]}");
			box.SetTooltip("");
            Container.Add(box);
		}
	}
}