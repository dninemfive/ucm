using CoenM.ImageHash;
using CoenM.ImageHash.HashAlgorithms;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Diagnostics;
using Image = SixLabors.ImageSharp.Image;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Concurrent;

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
		static void log<T>(string varName, IEnumerable<T> enumerable)
		{
			Utils.Log($"{varName}:");
			foreach(T item in enumerable)
			{
				Utils.Log($"\t{item}");
			} 
		} 
		StartSearchButton.IsEnabled = false;
		ItemId maxId = ItemManager.ItemsById.Keys.Max();
		Progress<(int ct, int total, string desc)> progress = new(value =>
		{
			double pct = value.ct / (double)value.total;
            ProgressBar.Progress = pct;
			ProgressLabel.Text = $"{value.ct}/{value.total} ({pct:P2})";
		});
		StartTimer();
		string path = Path.Join(MauiProgram.TEMP_BASE_FOLDER, "perceptualHashes.json");
		ConcurrentDictionary<ItemId, ulong> hashDict;
        int total = ItemManager.Items.Count(), ct = 0;
        if (File.Exists(path))
        {
            StatusLabel.Text = "Loading perceptual hashes...";
            hashDict = await Task.Run(() => JsonSerializer.Deserialize<ConcurrentDictionary<ItemId, ulong>>(File.ReadAllText(path))!);
            StatusLabel.Text = "Loading perceptual hashes... Done!";
        }
		else
		{
            StatusLabel.Text = "Generating perceptual hashes...";
            hashDict = new();
            foreach (Task<(ItemId, ulong)> task in GeneratePerceptualHashTasks())
            {
                ((IProgress<(int, int, string)>)progress).Report((ct++, total, ""));
                (ItemId id, ulong hash) = await task;
                hashDict[id] = hash;
            }
            File.WriteAllText(path, JsonSerializer.Serialize(hashDict));
        } 
        StatusLabel.Text = "Calculating similarities...";
		int min = 0, max = 20;
		total = (int)(Math.Pow(ItemManager.Items.Count() - min, 2) / 2) - max * max / 2;
        ct = 0;
        StatusLabel.Text = "Calculating similarities...2";
		HashSet<ItemPair> similarPairs = new();
        foreach (Task<(ItemPair, double)> task in GenerateSimilarityTasks(hashDict, min, max))
        {
            StatusLabel.Text = "Calculating similarities...3";
            ((IProgress<(int, int, string)>)progress).Report((ct++, total, ""));
            StatusLabel.Text = "Calculating similarities...4";
			(ItemPair pair, double similarity) = await task;
            StatusLabel.Text = "Calculating similarities...5";
            if (similarity > 90)
				similarPairs.Add(pair);
            StatusLabel.Text = "Calculating similarities...6";
        }
        StatusLabel.Text = "Calculating similarities...7";
        log(nameof(similarPairs), similarPairs);
        StatusLabel.Text = "Generating pools...";
		ct = 0;
		total = similarPairs.Count;
        List<HashSet<ItemId>> pools = await Task.Run(() =>
        {
            ((IProgress<(int, int, string)>)progress).Report((ct++, total, ""));
            List<HashSet<ItemId>> result = new();
            foreach(ItemPair pair in similarPairs)
            {
                bool found = false;
                foreach(HashSet<ItemId> pool in result)
                {
                    if(pool.Contains(pair.IdA) || pool.Contains(pair.IdB))
                    {
                        _ = pool.Add(pair.IdA);
                        _ = pool.Add(pair.IdB);
                        found = true;
                        break;
                    }
                }
                if(!found)
                {
                    result.Add(new() { pair.IdA, pair.IdB });
                }
            }
			return result;
        });
        StatusLabel.Text = "Outputting pools...";
		Utils.Log($"Found {pools.Count} sets of potentially duplicate items:");
        foreach (HashSet<ItemId> pool in pools)
		{
			Utils.Log(pool.Order().ListNotation());
		}
		_cancellationTokenSource.Cancel();
        StatusLabel.Text = "Done!";
        StartSearchButton.IsEnabled = true;
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
	private static async Task<(ItemId id, ulong hash)> GeneratePerceptualHashAsync(Item item)
	{
        DifferenceHash hashAlgorithm = new();
		Image<Rgba32> image = await Task.Run(() => Image.Load<Rgba32>(item.LocalPath.Value));
        ulong result = hashAlgorithm.Hash(image);
		image.Dispose();
		return (item.Id, result);
    }
	private static IEnumerable<Task<(ItemId, ulong)>> GeneratePerceptualHashTasks()
	{
		foreach(Item item in ItemManager.Items)
		{
			yield return GeneratePerceptualHashAsync(item);
		}
	}
	private static async Task<(ItemPair pair, double similarity)> CalculateSimilarityAsync(ItemId idA, ItemId idB, ConcurrentDictionary<ItemId, ulong> hashes)
	{
		return (new(idA, idB), await Task.Run(() => CompareHash.Similarity(hashes[idA], hashes[idB])));
	}
	private static IEnumerable<Task<(ItemPair, double)>> GenerateSimilarityTasks(ConcurrentDictionary<ItemId, ulong> hashes, ItemId? min = null, ItemId? max = null)
	{
		int skipA = min is null ? 0 : (int)min.Value.Value;
		List<ItemId> orderedIds = ItemManager.ItemsById.Keys.Order().ToList();
		int skipB = 0;
		foreach(ItemId idA in orderedIds.Skip(skipA).SkipLast(1))
		{
			skipB++;
			if(idA > max) break;
			foreach(ItemId idB in orderedIds.Skip(skipB))
			{
				yield return CalculateSimilarityAsync(idA, idB, hashes);
			}
		}
	}
	private readonly struct ItemPair
	{
		public readonly ItemId IdA, IdB;
		public ItemPair(ItemId a, ItemId b)
		{
			IdA = a < b ? a : b;
			IdB = b < a ? a : b;
		}
		public override bool Equals([NotNullWhen(true)] object? obj)
			=> obj is ItemPair other && this == other;
		public static bool operator ==(ItemPair a, ItemPair b)
			=> a.IdA == b.IdA && a.IdB == b.IdB;
		public static bool operator !=(ItemPair a, ItemPair b)
			=> !(a == b);
		public override int GetHashCode()
			=> HashCode.Combine(IdA, IdB);
		public static implicit operator ItemPair((ItemId a, ItemId b) tuple) => new(tuple.a, tuple.b);
		public override string ToString()
			=> $"pair({IdA}, {IdB})";
    }
}