using d9.utl;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace d9.ucm;

public partial class AcquisitionPage : ContentPage
{
    #region fields
    
    private List<string> _candidateLocations = new();
    // todo: rejected urls
    private readonly HashSet<string> _indexedHashes = new();
    #endregion
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
    }
    private HashSet<string> _locations = new();
    private async Task LoadCandidatesAsync()
    {
        _candidateLocations.Clear();
        _locations = ItemManager.AllLocations.ToHashSet();
        Utils.Log($"All locations:");
        foreach (string location in _locations.Order())
            Utils.Log($"\t{location}");
        // load local paths first,
        _candidateLocations = await LoadLocalPathsAsync();
        // then add bookmarks after, shuffled
        _candidateLocations.AddRange(await LoadBookmarksAsync());
        Utils.Log($"Candidate count by best url rule:");
        foreach(UrlRule rule in UrlRuleManager.UrlRules)
        {
            Utils.Log($"\t{rule.Name}\t{_candidateLocations.Count(x => UrlRule.BestFor(x)?.Name == rule.Name)})");
        }
    }
    private static void LogCount(string location, int count)
    {
        Utils.Log($"Added {count} candidate locations from {location}.");
    }
    private async Task<List<string>> LoadLocalPathsAsync()
    {
        List<string> result = new();
        foreach (string srcFolder in await File.ReadAllLinesAsync(@"C:\Users\dninemfive\Documents\workspaces\misc\ucm\maui-app\localFolderList.txt.secret"))
        {
            int ct = 0;
            foreach (string path in await Task.Run(srcFolder.EnumerateFilesRecursive))
            {
                if (_locations.Contains(path))
                    continue;
                ct++;
                result.Add(path);
            }
            LogCount(srcFolder, ct);
        }
        return result;
    }
    private const string _bookmarksPath = @"C:\Users\dninemfive\Documents\workspaces\misc\ucm\bookmark-plugin\ucm-bookmarks-firefox\bookmarks.json";
    private async Task<List<string>> LoadBookmarksAsync()
    {        
        List<string> result = new(),
                     bookmarks = (await JsonSerializer.DeserializeAsync<List<string>>(File.OpenRead(_bookmarksPath)))!;
        foreach (string? url in await Task.Run(() => bookmarks.Select(x => UrlRule.BestCanonicalUrlFor(x)).Distinct()))
        {
            if (url is null || _locations.Contains(url))
                continue;
            result.Add(url);
        }
        return result.Shuffled().ToList();
    }
    private async Task<CandidateItem?> MakeCandidateFor(string candidateLocation)
    {
        void log(bool rejected, string? info = null)
        {
            string rejection = rejected ? "❌" : "✔";
            info = info is null ? "" : $"({info})";
            Utils.Log($"\t{info,-24} {rejection} {candidateLocation}");        
        }
        if (_locations.Contains(candidateLocation))
            return null;
        CandidateItem? candidate = await CandidateItem.MakeFromAsync(candidateLocation);
        string? hash = candidate?.Hash;
        List<(bool assertion, string msg)> assertions = new()
        {
            (candidate is null, "candidate is null"),
            (await ItemManager.TryUpdateAnyMatchingItemAsync(candidate!), "existing item"),
            (hash is null, "hash is null"),
            (hash is not null && _indexedHashes.Contains(hash), "indexed hash"),
            (candidateLocation.BestAvailableView() is null, "no available view")
        };
        foreach((bool assertion, string msg) in assertions) {
            if(assertion)
            {
                log(true, msg);
                return null;
            }
        }
        log(false);
        return candidate;
    }
    private async void NextItem()
    {
        SetButtonsActive(false);
        Utils.Log($"next");
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
            CurrentPendingItemInfo.Text = $"{Index}/{_candidateLocations.Count} ({progress:P1}) | {IdManager.CurrentId}";
            _currentCandidate = await MakeCandidateFor(_candidateLocations[_index]);
            if (_currentCandidate is null)
                continue;
            _ = _indexedHashes.Add(_currentCandidate.Hash);
            if(_currentCandidate.SourceUrl is not null)
            {
                try
                {
                    ItemHolder.Content = new Image()
                    {
                        Source = ImageSource.FromStream(() => new MemoryStream(_currentCandidate.Data!)),
                        IsAnimationPlaying = true,
                        Aspect = Aspect.AspectFit
                    };
                } catch(Exception e)
                {
                    Utils.Log($"Issue getting image data for {_currentCandidate}: {e.Message}");
                }
            } 
            else
            {
                ItemHolder.Content = _currentCandidate.Location.BestAvailableView();
            }
            string ct = (_candidateLocations?.Count).PrintNull(), location = (_currentCandidate?.Location).PrintNull();
            CurrentPendingItemInfo.Text = $"{Index}/{ct} ({progress:P1}) | {IdManager.CurrentId}\t{location}\t{_currentCandidate?.SourceUrl.FileExtension()}";
            SetButtonsActive(true);
        }
    }
    #endregion
    #region button events
    private async void StartButton_Clicked(object sender, EventArgs e)
    {
        StartButton.IsEnabled = false;
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
        Utils.Log($"accepted");
        _ = await _currentCandidate!.SaveAsync();
        NextItem();
    }
    private void Skip_Clicked(object sender, EventArgs e)
    {
        Utils.Log($"skipped");
        NextItem();
    }
    private void Reject_Clicked(object sender, EventArgs e)
    {
        Utils.Log($"rejected");
        File.AppendAllText(MauiProgram.REJECTED_HASH_FILE, $"{_currentCandidate!.Hash}\n");
        _ = _indexedHashes.Add(_currentCandidate.Hash);
        NextItem();
    }
    #endregion    
    private void SetButtonsActive(bool val)
    {
        Accept.IsEnabled = val;
        Skip.IsEnabled = val;
        Reject.IsEnabled = val;
    }
}