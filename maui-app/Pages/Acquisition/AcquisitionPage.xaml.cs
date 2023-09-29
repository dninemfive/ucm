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
    private async Task LoadPendingItemsAsync()
    {
        _candidateItems.Clear();
        HashSet<string> locations = ItemManager.AllLocations.ToHashSet();
        foreach (string s in await File.ReadAllLinesAsync(@"C:\Users\dninemfive\Documents\workspaces\misc\ucm\maui-app\localFolderList.txt.secret"))
        {
            Utils.Log(s);
            string[] split = s.Split("\t");
            string srcFolder = split[0];
            string? destFolder = split.Length > 1 ? split[1] : null;
            foreach (string path in await Task.Run(srcFolder.EnumerateFilesRecursive))
            {
                
                if (locations.Contains(path))
                    continue;
                CandidateItem? candidate = await CandidateItem.MakeFromAsync(path);
                if (candidate is null)
                    continue;                
                string? hash = candidate.Hash;
                Utils.Log($"\t{path} -> {candidate}\n\t\thash: {hash}");
                if (candidate.Hash is not null && 
                    ItemManager.ItemsByHash.TryGetValue(candidate.Hash, out Item? item) && 
                    !item.HasSourceInfoFor(path))
                {
                    ItemSource? source = path.ItemSource();
                    if (source is not null)
                    {
                        item.Sources.Add(source);
                        Utils.Log($"\t\tadding source {source} to existing item {item}.");
                        await item.SaveAsync();
                    }
                    continue;
                }
                if (hash is null || _indexedHashes.Contains(hash) || path.BestAvailableView() is null)
                {
                    Utils.Log($"\t\tcontinue");
                    continue;
                }
                Utils.Log($"\t\tindexing hash {hash}");
                _ = _indexedHashes.Add(hash);
                _candidateItems.Add(candidate);
            }
        }
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