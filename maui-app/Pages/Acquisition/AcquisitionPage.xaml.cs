﻿using d9.utl;
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
        /*
        if (_indexedHashes.Any())
            Utils.Log($"Ignoring the following hashes:\n\t{_indexedHashes.Distinct()
                                                                         .Order()
                                                                         .Aggregate((x, y) => $"{x}\n\t{y}")}");
        */
    }
    private HashSet<string> _locations = new();
    private async Task LoadCandidatesAsync()
    {
        _candidateLocations.Clear();
        _locations = ItemManager.AllLocations.ToHashSet();
        int previousCount = 0;
        void logct(string msg)
        {
            Utils.Log($"Added {_candidateLocations.Count - previousCount} candidate locations from {msg}.");
            previousCount = _candidateLocations.Count;
        }        
        foreach (string srcFolder in await File.ReadAllLinesAsync(@"C:\Users\dninemfive\Documents\workspaces\misc\ucm\maui-app\localFolderList.txt.secret"))
        {
            foreach (string path in await Task.Run(srcFolder.EnumerateFilesRecursive))
            {
                if (_locations.Contains(path))
                    continue;
                _candidateLocations.Add(path);
            }
            logct(srcFolder);
        }
        List<string> bookmarks = JsonSerializer.Deserialize<List<string>>
            (File.ReadAllText(@"C:\Users\dninemfive\Documents\workspaces\misc\ucm\bookmark-plugin\ucm-bookmarks-firefox\bookmarks.json"))!;
        foreach(string? url in bookmarks.Select(UrlRule.BestUrlFor).Distinct())
        {
            if (url is null || _locations.Contains(url))
                continue;
            _candidateLocations.Add(url);
        }
        logct("bookmarks");
        _candidateLocations = await Task.Run(() => _candidateLocations = _candidateLocations.Shuffled().ToList());
    }
    private async Task<CandidateItem?> MakeCandidateFor(string location)
    {
        void log(bool rejected, string? info = null)
        {
            string rejection = rejected ? "SKIP" : "keep";
            Utils.Log($"\t{rejection,-4}\t{location,-180}\t{info}");        
        }
        if (_locations.Contains(location))
            return null;
        CandidateItem? candidate = await CandidateItem.MakeFromAsync(location);
        string? hash = candidate?.Hash;
        List<(bool assertion, string msg)> assertions = new()
        {
            (candidate is null, "candidate is null"),
            (await ItemManager.TryUpdateAnyMatchingItemAsync(hash, location), "existing item"),
            (hash is null, "hash is null"),
            (hash is not null && _indexedHashes.Contains(hash), "indexed hash"),
            (location.BestAvailableView() is null, "no available view")
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
}