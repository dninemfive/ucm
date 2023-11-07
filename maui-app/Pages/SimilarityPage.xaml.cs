using CoenM.ImageHash;
using CoenM.ImageHash.HashAlgorithms;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Diagnostics;
using Image = SixLabors.ImageSharp.Image;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Text.Json;

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
		Dictionary<ItemId, ulong> hashDict;
        int total = ItemManager.Items.Count(), ct = 0;
        if (File.Exists(path))
        {
            StatusLabel.Text = "Loading perceptual hashes...";
            hashDict = await Task.Run(() => JsonSerializer.Deserialize<Dictionary<ItemId, ulong>>(File.ReadAllText(path))!);
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
		// IEnumerable<(ItemId id, ulong hash)> hashes = await Task.WhenAll(GeneratePerceptualHashTasks());
        StatusLabel.Text = "Generating perceptual hashes... Done!";
        //  Utils.Log($"hashDict: {hashDict.Select(x => (x.Key, x.Value)).ListNotation()}");
        // Dictionary<ItemId, ulong> hashDict = hashes.ToDictionary();
        StatusLabel.Text = "Calculating similarities...";
		//IEnumerable<(ItemPair pair, double similarity)> similarities = await Task.WhenAll(GenerateSimilarityTasks(AllItemPairs(), hashDict));
		IEnumerable<Task<(ItemPair, double)>> similarityTasks = GenerateSimilarityTasks(await Task.Run(() => AllItemPairs(0, 20)), hashDict);
        total = similarityTasks.Count();
		ct = 0;
		HashSet<ItemPair> similarPairs = new();
        await foreach (Task<(ItemPair, double)> task in similarityTasks.ToAsyncEnumerable())
        {
            ((IProgress<(int, int, string)>)progress).Report((ct++, total, ""));
            (ItemPair pair, double similarity) = await task;
			if (similarity > 90)
				similarPairs.Add(pair);
		}
		// Dictionary<ItemPair, double> similarityDict = similarities.ToDictionary();
		log(nameof(similarPairs), similarPairs);
        StatusLabel.Text = "Finding similar pairs...";
        StatusLabel.Text = "Generating pools...";
		// now for the more linear task: go through pairs and find sets of similar items
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
	private static IEnumerable<ItemPair> AllItemPairs(ItemId? min = null, ItemId? max = null)
	{
		ItemId minId = min ?? ItemManager.ItemsById.Keys.Min(), 
			   maxId = max ?? ItemManager.ItemsById.Keys.Max();
		ItemId max2 = ItemManager.ItemsById.Keys.Max();
		for(ItemId idA = minId; idA < maxId; idA++)
		{
			if (!ItemManager.ItemsById.ContainsKey(idA))
				continue;
			for (ItemId idB = idA + 1; idB <= max2; idB++)
			{
				if (!ItemManager.ItemsById.ContainsKey(idB))
					continue;
				yield return new(idA, idB);
			}
		}
	}
	private static async Task<(ItemPair pair, double similarity)> CalculateSimilarityAsync(ItemPair pair, ulong hashA, ulong hashB)
	{
		return (pair, await Task.Run(() => CompareHash.Similarity(hashA, hashB)));
	}
	private static IEnumerable<Task<(ItemPair pair, double similarity)>> GenerateSimilarityTasks(IEnumerable<ItemPair> pairs, Dictionary<ItemId, ulong> hashDict)
	{
		foreach(ItemPair pair in pairs)
		{
			yield return CalculateSimilarityAsync(pair, hashDict[pair.IdA], hashDict[pair.IdB]);
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