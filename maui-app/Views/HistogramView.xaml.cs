using Microsoft.Maui.Controls.Shapes;
using System.Numerics;
using d9.utl;

namespace d9.ucm;

public partial class HistogramView : ContentView
{
	private readonly List<double> _data = new();
	public IEnumerable<double> Data => _data;
	public Color ForegroundColor { get; set; } = Colors.White;
	public int? ForcedLowerBound { get; set; } = null;
	public int? ForcedUpperBound { get; set; } = null;
	public Func<int, double>? BinHeightFunction = null;
	public bool UseProportion { get; set; } = false;
	private double _binWidth = 0.1;
	public double BinWidth
	{
		get => _binWidth;
		set
		{
			if (value <= 0)
				throw new ArgumentOutOfRangeException(nameof(value));
			_binWidth = value;
			Update();
		}
	}
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
	public void AddData(IEnumerable<double> data, bool update = true)
	{
		foreach (double datum in data)
			_data.Add(datum);
		if(update) Update();
	}
	public void Clear()
    {
		_data.Clear();
		Update();
    }
	public void ReplaceData(IEnumerable<double> newData, double? newBinWidth = null)
	{
        _data.Clear();
		if (newBinWidth is double newValue)
		{
			// avoid double updates
			AddData(newData, false);
			BinWidth = newValue;
		}
		else
		{
			AddData(newData);
		}
	}
	public void Update()
	{
		Utils.Log($"Update()");
        Container.Children.Clear();
        if (!_data.Any())
			return;
		CountingDictionary<int, int> counter = new();
		foreach(double datum in _data)
		{
			counter.Increment((int)(datum / BinWidth));
		}
        int lowerBound = ForcedLowerBound ?? counter.Keys.Min(), upperBound = ForcedUpperBound ?? counter.Keys.Max();
		double bins = Math.Abs(upperBound - lowerBound) / BinWidth,
			   height = HeightRequest,
			   width = Math.Max(WidthRequest / bins, 10);
		double maxValue, total;
		if(BinHeightFunction is null)
		{
			maxValue = counter.Values.Max();
			total = counter.Values.Sum();
		}
		else
		{
			IEnumerable<double> adjustedValues = counter.Select(x => BinHeightFunction(x.Key) * x.Value);
			maxValue = adjustedValues.Max();
			total = adjustedValues.Sum();
		}
        for (int i = lowerBound; i <= upperBound; i++)
		{
			double count = counter[i];
			if(BinHeightFunction is not null)
            {
                double binHeightFactor = BinHeightFunction(i);
                Utils.Log($"{i,3}: {count,4}\t{binHeightFactor,8:F5}");
                count *= binHeightFactor;
			} else
			{
				Utils.Log($"{i,3}: {count,4}");
			}
			RatioBoxView box = new()
			{
				WidthRequest = width,
				HeightRequest = height,
				ForegroundColor = ForegroundColor,
				Margin = new(1, 0),
				Ratio = count / (double)(!UseProportion ? maxValue : total)
			};
			if(UseProportion)
			{
				double percentage = count / total;
                ToolTipProperties.SetText(box, BinWidth == 1 ? $"{i}: {percentage:P2}" : $"{i * BinWidth:F2}: {percentage:P2}");
            }
			else
			{
                ToolTipProperties.SetText(box, BinWidth == 1 ? $"{i}: {count}" : $"{i * BinWidth:F2}: {count}");
            } 
            Container.Add(box);
		}
	}
}