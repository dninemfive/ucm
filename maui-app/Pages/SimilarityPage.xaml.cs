using CoenM.ImageHash;
using System.Diagnostics;

namespace d9.ucm;

public partial class SimilarityPage : ContentPage
{
	public SimilarityPage()
	{
		InitializeComponent();
	}
	private CancellationTokenSource _cancellationTokenSource = new();
    private async void StartSearchButton_Clicked(object sender, EventArgs e)
    {
		
		Dictionary<ItemId, HashSet<ItemId>> matches = new();
		ItemId maxId = ItemManager.ItemsById.Keys.Max();
		int totalComparisons = (int)(maxId.Value * maxId.Value / 2);
		void add(ItemId a, ItemId b)
		{
			if(matches.TryGetValue(a, out HashSet<ItemId>? value))
			{
                value.Add(b);
			} else if(matches.TryGetValue(b, out HashSet<ItemId>? value2))
			{
				value2.Add(a);
			}
			else
			{
                matches[a] = new()
                {
                    b
                };
            } 
		}
		Progress<int> progress = new(value =>
		{
			double pct = value / (double)totalComparisons;
            ProgressBar.Progress = pct;
			ProgressLabel.Text = $"{value}/{totalComparisons} ({pct:P2})";
		});
		StartTimer();
		await Task.Run(() =>
		{
			int ct = 0;
			foreach (ItemId id in ItemManager.ItemsById.Keys)
			{
				if (id > 100)
					break;
				if (id == maxId || !ItemManager.ItemsById.TryGetValue(id, out Item? a))
					continue;
				for (ItemId i = id + 1; i < maxId; i++)
				{
					if (ItemManager.ItemsById.TryGetValue(i, out Item? b))
                    {
                        if (CompareHash.Similarity(a.PerceptualHash, b.PerceptualHash) > 0.9)
                            add(a.Id, b.Id);
                        ct++;
                        ((IProgress<int>)progress).Report(ct);

                    }
				}
			}
		});
		foreach((ItemId k, HashSet<ItemId> v) in matches)
		{
			v.Add(k);
			Utils.Log(v.Order().ListNotation());
		}
		_cancellationTokenSource.Cancel();
    }
    // https://mallibone.com/post/dotnetmaui-countdown-button
    private async void StartTimer()
    {
        DateTime startTime = DateTime.Now;
        while (!_cancellationTokenSource.IsCancellationRequested)
		{
			TimeElapsed.Text = $"{DateTime.Now - startTime:h\\:mm\\:ss}";
			await Task.Delay(500);
		}
	}
}