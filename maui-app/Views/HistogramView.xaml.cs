using Microsoft.Maui.Controls.Shapes;
using System.Numerics;

namespace d9.ucm;

public partial class HistogramView : ContentView
{
	private readonly List<double> _data = new();
	public IEnumerable<double> Data => _data;
	public Color ForegroundColor { get; private set; } = Colors.White;
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
        int minKey = counter.Keys.Min(), maxKey = counter.Keys.Max();
        double maxValue = counter.Values.Max(), height = HeightRequest, width = WidthRequest, bins = Math.Abs(minKey - maxKey) + 1;		
		for(int i = minKey; i <= maxKey; i++)
		{
			Container.Add(new Rectangle()
			{
				WidthRequest = width / bins,
				HeightRequest = counter[i] / maxValue * height,
				VerticalOptions = LayoutOptions.End,
				BackgroundColor = ForegroundColor
			});
		}
	}
}