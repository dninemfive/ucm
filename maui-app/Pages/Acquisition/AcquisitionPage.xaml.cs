using d9.utl;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace d9.ucm;

public partial class AcquisitionPage : ContentPage
{
    #region fields
    
    private readonly List<PendingItem> _pendingItems = new();
    private readonly HashSet<string> _indexedHashes = new();
    #endregion
    private PendingItem? CurrentPendingItem => _index < _pendingItems.Count ? _pendingItems[Index] : null;
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
        _pendingItems.Clear();
        HashSet<string> locations = ItemManager.AllLocations.ToHashSet();
        foreach (string s in await File.ReadAllLinesAsync(@"C:\Users\dninemfive\Documents\workspaces\misc\ucm\maui-app\localFolderList.txt.secret"))
        {
            string[] split = s.Split("\t");
            string srcFolder = split[0];
            string? destFolder = split.Length > 1 ? split[1] : null;
            foreach (string path in await Task.Run(srcFolder.EnumerateFilesRecursive))
            {
                if (locations.Contains(path))
                    continue;
                string? curHash = await path.FileHashAsync();
                Utils.Log($"wtf?? {curHash} {ItemManager.ItemsByHash}");
                if(curHash is not null && 
                    ItemManager.ItemsByHash.TryGetValue(curHash, out Item? item) && 
                    !item.HasSourceInfoFor(path))
                {
                    item.Sources.Add(new("Local Filesystem", path));
                    continue;
                }
                if (curHash is null || _indexedHashes.Contains(curHash) || path.BestAvailableView() is null)
                {
                        continue;
                }
                _ = _indexedHashes.Add(curHash);
                _pendingItems.Add(new(path, curHash, destFolder));
            }
        }
    }
    private async void NextItem()
    {
        Index++;
        if(_index >= _pendingItems.Count)
        {
            await LoadPendingItemsAsync();
            _index = 0;
        }
        if (CurrentPendingItem is null)
        {
            Alert.IsVisible = true;
            return;
        }
        float progress = Index/(float)_pendingItems.Count;
        ProgressBar.Progress = progress;
        ItemHolder.Content = CurrentPendingItem.CurrentPath.BestAvailableView();
        CurrentPendingItemInfo.Text = $"{Index}/{_pendingItems.Count} ({progress:P1}) | {IdManager.CurrentId}\t{CurrentPendingItem.CurrentPath}";
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
        await CurrentPendingItem!.Save();
        NextItem();
    }
    private void Skip_Clicked(object sender, EventArgs e)
    {
        NextItem();
    }
    private void Reject_Clicked(object sender, EventArgs e)
    {
        File.AppendAllText(MauiProgram.REJECTED_HASH_FILE, $"{CurrentPendingItem!.Hash}\n");
        _ = _indexedHashes.Add(CurrentPendingItem.Hash);
        NextItem();
    }
    #endregion    
}