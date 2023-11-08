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
        UpdateButtons();
	}
	private CancellationTokenSource _cancellationTokenSource = new();
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
	private static IEnumerable<Task<(ItemId, ulong)>> GeneratePerceptualHashTasks(HashSet<ItemId> ignoreIds)
	{
		foreach(Item item in ItemManager.Items)
		{
            if (ignoreIds.Contains(item.Id))
                continue;
			yield return GeneratePerceptualHashAsync(item);
		}
	}
    private List<ItemId> _similarItemIds = new();
    private ConcurrentDictionary<ItemId, ulong>? _hashDict = null;
	private async Task SearchForSimilarImagesAsync()
	{
        void updateLabel(string msg)
        {
            StatusLabel.Text = msg;
            Utils.Log(msg);
        }
        ItemId maxId = ItemManager.ItemsById.Keys.Max();
        Progress<(int ct, int total, string desc)> progress = new(value =>
        {
            double pct = value.ct / (double)value.total;
            ProgressBar.Progress = pct;
            ProgressLabel.Text = $"{value.ct}/{value.total} ({pct:P2})";
        });
        StartTimer();
        string path = Path.Join(MauiProgram.TEMP_BASE_FOLDER, "perceptualHashes.json");
        if(_hashDict is null)
        {
            if (File.Exists(path))
            {
                updateLabel("Loading perceptual hashes...");
                _hashDict = await Task.Run(() => JsonSerializer.Deserialize<ConcurrentDictionary<ItemId, ulong>>(File.ReadAllText(path))!);
            }
            else
            {
                _hashDict = new();
            }
        } 
        updateLabel("Generating perceptual hashes...");
        HashSet<ItemId> idsToSkip = _hashDict.Keys.ToHashSet();
        IEnumerable<Task<(ItemId, ulong)>> perceptualTasks = GeneratePerceptualHashTasks(idsToSkip);
        int total = perceptualTasks.Count(), ct = 0;
        foreach (Task<(ItemId, ulong)> task in perceptualTasks)
        {
            ((IProgress<(int, int, string)>)progress).Report((ct++, total, ""));
            (ItemId id, ulong hash) = await task;
            _hashDict[id] = hash;
        }
        await Task.Run(() => File.WriteAllText(path, JsonSerializer.Serialize(_hashDict)));
        updateLabel("Calculating similarities...");
        _similarItemIds.Clear();
        if (ItemView.IItem is Item curItem)
        {
            foreach (Item otherItem in ItemManager.Items.Where(x => x != curItem))
            {
                await Task.Run(() =>
                {
                    double similarity = CompareHash.Similarity(_hashDict[curItem.Id], _hashDict[otherItem.Id]);
                    if (similarity > 90)
                        _similarItemIds.Add(otherItem.Id);
                });
            }
        }
        ProgressLabel.Text = "";
        _cancellationTokenSource.Cancel();
        TimeElapsed.Text = "";
        _similarItemIds = _similarItemIds.Order().ToList();
        updateLabel(_similarItemIds.ListNotation());
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

    private async void IdEntry_Completed(object sender, EventArgs e)
    {
		ItemId? id = null;
		try
		{
			id = (sender as Entry)!.Text.ToUpper();
		} catch { }
        await SwitchToItem(id);
        (sender as Entry)!.Text = "";
    }
    private ItemId _lastId = 0;
    private async Task SwitchToItem(ItemId? id, bool ignoreForHistory = false)
    {
        Item? item = ItemManager.TryGetItemById(id);
        ItemView.IItem = item;
        if(item is not null)
        {
            await SearchForSimilarImagesAsync();
        }
        IdEntry.Text = ItemView.IItemAs<Item>()?.Id ?? "";
        if (!ignoreForHistory)
        {
            NextItemButton.IsEnabled = ItemView.IItem is not null;
            if(ItemView.IItemAs<Item>() is Item curItem)
            {
                _lastId = curItem;
            }
        }
        UpdateButtons();
    }
    private void UpdateButtons()
    {
        // NextItemButton.IsEnabled = ItemView.IItem is not null;
        bool anySimilarImages = ItemView.IItem is not null && _similarItemIds.Any();
        PreviousButton.IsEnabled = anySimilarImages;
        NextButton.IsEnabled = anySimilarImages;
        MergeButton.IsEnabled = anySimilarImages;
    }
    private async void NextItemButton_Clicked(object sender, EventArgs e)
    {
        await NextSimilarSet();
    }
    private async Task NextSimilarSet(ItemId? currentId = null)
    {
        NextItemButton.IsEnabled = false;
        if (currentId is null && ItemView.IItem is null)
        {
            return;
        }
        else currentId ??= (ItemView.IItem as Item)!.Id + 1;
        _similarItemIds.Clear();
        while (!_similarItemIds.Any())
        {
            await SwitchToItem(currentId!++, ignoreForHistory: true);
        }
        _lastId = currentId.Value;
        NextItemButton.IsEnabled = true;
    }
    private async void Previous_Clicked(object sender, EventArgs e)
    {
        await SwitchToItem(_similarItemIds.Last(), ignoreForHistory: true);
    }

    private async void Next_Clicked(object sender, EventArgs e)
    {
        await SwitchToItem(_similarItemIds.First(), ignoreForHistory: true);
    }
    private async void MergeButton_Clicked(object sender, EventArgs e)
    {
        Item? curItem = ItemView.IItemAs<Item>();
        if(curItem is not null)
        {
            Item? nextItem = await Item.MergeAsync(curItem, _similarItemIds.Select(x => ItemManager.TryGetItemById(x)).Where(x => x is not null)!);
            if(nextItem is not null)
            {
                // await SwitchToItem(id);
                await NextSimilarSet(_lastId + 1);
            }
        }
    }
}