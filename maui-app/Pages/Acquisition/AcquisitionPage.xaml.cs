using d9.utl;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace d9.ucm;

public partial class AcquisitionPage : ContentPage
{
    #region fields
    
    private readonly List<CandidateItem> _candidateItems = new();
    // todo: rejected urls
    private readonly HashSet<string> _indexedHashes = new();
    #endregion
    private CandidateItem? CurrentCandidate => _index < _candidateItems.Count ? _candidateItems[Index] : null;
    private bool _alreadyAdding = false;
    private bool AlreadyAdding
    {
        get => _alreadyAdding || ((_alreadyAdding = true) && false);
    }
    private int _index = -1;
    public int Index
    {
        get => _index;
        set => _index = value;
    }    
    public AcquisitionPage()
    {
        InitializeComponent();
    }
    #region misc methods
    private async Task LoadHashesAsync()
    {
        if (File.Exists(MauiProgram.REJECTED_HASH_FILE))
        {
            await foreach (string s in File.ReadLinesAsync(MauiProgram.REJECTED_HASH_FILE))
            {
                _ = _indexedHashes.Add(s);
                Utils.Log($"Ignoring {s}");
            }
        }
        else
        {
            _ = File.Create(MauiProgram.REJECTED_HASH_FILE);
        }
    }
    HashSet<string> _locations;
    private async Task LoadPendingItemsAsync()
    {
        _candidateItems.Clear();
        _locations = ItemManager.AllLocations.ToHashSet();
        List<Task<CandidateItem?>> tasks = new();
        foreach (string s in await File.ReadAllLinesAsync(@"C:\Users\dninemfive\Documents\workspaces\misc\ucm\maui-app\localFolderList.txt.secret"))
        {
            Utils.Log(s);
            string[] split = s.Split("\t");
            string srcFolder = split[0];
            string? destFolder = split.Length > 1 ? split[1] : null;            
            foreach (string path in await Task.Run(srcFolder.EnumerateFilesRecursive))
            {
                tasks.Add(MakeCandidateFor(path));
            }
        }
        List<string> bookmarks = JsonSerializer.Deserialize<List<string>>
            (File.ReadAllText(@"C:\Users\dninemfive\Documents\workspaces\misc\ucm\bookmark-plugin\ucm-bookmarks-firefox\bookmarks.json"))!;
        foreach(string url in bookmarks)
        {
            tasks.Add(MakeCandidateFor(url));
        }
        CandidateItem?[] items = await Task.WhenAll(tasks);
        foreach (CandidateItem? ci in items) AddCandidate(ci);
    }
    private async Task<CandidateItem?> MakeCandidateFor(string location)
    {
        if (_locations.Contains(location))
            return null;
        CandidateItem? candidate = await CandidateItem.MakeFromAsync(location);
        if (candidate is null)
            return null;
        string? hash = candidate.Hash;
        Utils.Log($"\t{location} -> {candidate}\n\t\thash: {hash}");
        if (candidate.Hash is not null &&
            ItemManager.ItemsByHash.TryGetValue(candidate.Hash, out Item? item) &&
            !item.HasSourceInfoFor(location))
        {
            ItemSource? source = location.ItemSource();
            if (source is not null)
            {
                item.Sources.Add(source);
                Utils.Log($"\t\tadding source {source} to existing item {item}.");
                await item.SaveAsync();
            }
            return null;
        }
        if (hash is null || _indexedHashes.Contains(hash) || location.BestAvailableView() is null)
        {
            Utils.Log($"\t\tcontinue");
            return null;
        }        
        return candidate;
    }
    private void AddCandidate(CandidateItem? ci)
    {
        if (ci is null)
            return;
        Utils.Log($"AddCandidate({ci})");
        _ = _indexedHashes.Add(ci.Hash);
        _candidateItems.Add(ci);
    }
    private async void NextItem()
    {
        Index++;
        if(_index >= _candidateItems.Count)
        {
            await LoadPendingItemsAsync();
            _index = 0;
        }
        if (CurrentCandidate is null)
        {
            Alert.IsVisible = true;
            return;
        }
        float progress = Index/(float)_candidateItems.Count;
        ProgressBar.Progress = progress;
        ItemHolder.Content = CurrentCandidate.Location.BestAvailableView();
        CurrentPendingItemInfo.Text = $"{Index}/{_candidateItems.Count} ({progress:P1}) | {IdManager.CurrentId}\t{CurrentCandidate.Location}";
    }
    #endregion
    #region button events
    private async void StartButton_Clicked(object sender, EventArgs e)
    {
        if (AlreadyAdding)
            return;
        StartButton.Text = "Loading hashes...";
        await LoadHashesAsync();
        StartButton.Text = "Loading pending items...";
        await LoadPendingItemsAsync();        
        NextItem();
        StartButton.IsVisible = false;
        AcquisitionBlock.IsVisible = true;
    }
    private async void Accept_Clicked(object sender, EventArgs e)
    {
        _ = await CurrentCandidate!.SaveAsync();
        NextItem();
    }
    private void Skip_Clicked(object sender, EventArgs e)
    {
        NextItem();
    }
    private void Reject_Clicked(object sender, EventArgs e)
    {
        File.AppendAllText(MauiProgram.REJECTED_HASH_FILE, $"{CurrentCandidate!.Hash}\n");
        _ = _indexedHashes.Add(CurrentCandidate.Hash);
        NextItem();
    }
    #endregion    
}