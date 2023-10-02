using d9.utl;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace d9.ucm;

public partial class AcquisitionPage : ContentPage
{
    #region fields
    
    private readonly List<string> _candidateLocations = new();
    // todo: rejected urls
    private readonly HashSet<string> _indexedHashes = new();
    #endregion
    private bool _alreadyAdding = false;
    private bool AlreadyAdding
    {
        get => _alreadyAdding || ((_alreadyAdding = true) && false);
    }
    private CandidateItem? _currentCandidate = null;
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
            }
        }
        else
        {
            _ = File.Create(MauiProgram.REJECTED_HASH_FILE);
        }
        if (_indexedHashes.Any())
            Utils.Log($"Ignoring the following hashes: {_indexedHashes.Order().ListNotation()}");
    }
    private HashSet<string> _locations = new();
    private async Task LoadCandidatesAsync()
    {
        _candidateLocations.Clear();
        _locations = ItemManager.AllLocations.ToHashSet();
        void logct() => Utils.Log($"There are now {_candidateLocations.Count} candidate locations.");
        foreach (string srcFolder in await File.ReadAllLinesAsync(@"C:\Users\dninemfive\Documents\workspaces\misc\ucm\maui-app\localFolderList.txt.secret"))
        {
            Utils.Log($"Loading candidate paths in {srcFolder}...");  
            foreach (string path in await Task.Run(srcFolder.EnumerateFilesRecursive))
            {
                if (_locations.Contains(path))
                    continue;
                _candidateLocations.Add(path);
            }
            logct();
        }
        Utils.Log($"Loading candidate urls in bookmarks...");
        List<string> bookmarks = JsonSerializer.Deserialize<List<string>>
            (File.ReadAllText(@"C:\Users\dninemfive\Documents\workspaces\misc\ucm\bookmark-plugin\ucm-bookmarks-firefox\bookmarks.json"))!;
        foreach(string? url in bookmarks.Select(x => UrlRule.BestFor(x)?.UrlFor(x)).Distinct())
        {
            if (url is null || _locations.Contains(url))
                continue;
            _candidateLocations.Add(url);
        }
        logct();
    }
    private async Task<CandidateItem?> MakeCandidateFor(string location)
    {
        void log(bool rejected, string? info = null)
        {
            string rejection = rejected ? "SKIP" : "";
            info = info is null ? "" : $" ({info})";
            Utils.Log($"\t{rejection,-4}\t{location,-180}\t{info}");        }
        
        if (_locations.Contains(location))
        {
            // log(true, "location already indexed");
            return null;
        }
        CandidateItem? candidate = await CandidateItem.MakeFromAsync(location);
        if (candidate is null)
        {
            log(true, "candidate was null");
            return null;
        }
        string? hash = candidate.Hash;
        if (await ItemManager.TryUpdateAnyMatchingItemAsync(hash, location))
        {
            log(true, "existing item with same hash");
            return null;
        }
        if(hash is null)
        {
            log(true, "null hash");
            return null;
        }
        if (hash is not null && _indexedHashes.Contains(hash))
        {
            log(true, $"hash already indexed");
            return null;
        }
        if(location.BestAvailableView() is null)
        {
            log(true, "no available view");
            return null;
        }
        log(false);
        return candidate;
    }
    private async void NextItem()
    {
        Utils.Log($"NextItem()");
        _currentCandidate = null;
        while(_currentCandidate is null)
        {
            Index++;
            if (!_candidateLocations!.Any())
                return;
            if (_index >= _candidateLocations!.Count)
            {
                await LoadCandidatesAsync();
                _index = 0;
            }
            if (_index >= _candidateLocations.Count)
            {
                Alert.IsVisible = true;
                return;
            }
            float progress = Index/(float)_candidateLocations.Count;
            ProgressBar.Progress = progress;
            _currentCandidate = await MakeCandidateFor(_candidateLocations[_index]);
            if (_currentCandidate is null)
                continue;
            _ = _indexedHashes.Add(_currentCandidate.Hash);
            ItemHolder.Content = (await _currentCandidate.GetSourceUrlAsync() ?? _currentCandidate.Location)?.BestAvailableView();
            Utils.Log($"ItemHolder content: {(ItemHolder.Content as Image)?.Source}");
            CurrentPendingItemInfo.Text = $"{Index}/{_candidateLocations?.Count.PrintNull()} ({progress:P1}) | {IdManager.CurrentId}\t{_currentCandidate?.Location.PrintNull()}";
        }
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
        await LoadCandidatesAsync();        
        NextItem();
        StartButton.IsVisible = false;
        AcquisitionBlock.IsVisible = true;
    }
    private async void Accept_Clicked(object sender, EventArgs e)
    {
        Utils.Log($"Accept_Clicked({_currentCandidate?.Location.PrintNull()})");
        _ = await _currentCandidate!.SaveAsync();
        NextItem();
    }
    private void Skip_Clicked(object sender, EventArgs e)
    {
        Utils.Log($"Skip_Clicked({_currentCandidate?.Location.PrintNull()})");
        NextItem();
    }
    private void Reject_Clicked(object sender, EventArgs e)
    {
        Utils.Log($"Reject_Clicked({_currentCandidate?.Location.PrintNull()})");
        File.AppendAllText(MauiProgram.REJECTED_HASH_FILE, $"{_currentCandidate!.Hash}\n");
        _ = _indexedHashes.Add(_currentCandidate.Hash);
        NextItem();
    }
    #endregion    
}