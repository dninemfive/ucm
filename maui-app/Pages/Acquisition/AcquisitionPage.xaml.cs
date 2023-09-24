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
    private readonly HashSet<string> _hashes = new();
    #endregion
    private class PendingItem
    {
        public string Hash;
        public string CurrentPath;
        public string? MoveToFolder;
        public PendingItem(string path, string hash, string? moveToFolder)
        {
            Hash = hash;
            CurrentPath = path;
            MoveToFolder = moveToFolder;
        }
        public async Task Save()
        {
            if (File.Exists(CurrentPath))
            {
                if (MoveToFolder is not null)
                {
                    ItemId id = IdManager.Register();
                    string newPath = Path.Join(MoveToFolder, $"{id}{Path.GetExtension(CurrentPath).ToLower()}");
                    CurrentPath.MoveFileTo(newPath);
                    _ = await ItemManager.CreateAndSave(newPath, Hash, id);
                }
                else
                {
                    _ = await ItemManager.CreateAndSave(CurrentPath, Hash);
                }
            }
        }
    }
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
        private set
        {
            _index = value;
            if (_index >= _pendingItems.Count)
                _index = 0;
        }
    }    
    public AcquisitionPage()
    {
        InitializeComponent();
    }
    #region misc methods
    private async Task LoadHashesAsync()
    {
        foreach (Item item in ItemManager.Items)
            _ = _hashes.Add(item.Hash);
        if (File.Exists(MauiProgram.REJECTED_HASH_FILE))
        {
            await foreach (string s in File.ReadLinesAsync(MauiProgram.REJECTED_HASH_FILE))
                _ = _hashes.Add(s);
        }
        else
        {
            _ = File.Create(MauiProgram.REJECTED_HASH_FILE);
        }
    }
    private async Task LoadPendingItemsAsync()
    {
        foreach (string s in await File.ReadAllLinesAsync(@"C:\Users\dninemfive\Documents\workspaces\misc\ucm\maui-app\localFolderList.txt.secret"))
        {
            string[] split = s.Split("\t");
            string srcFolder = split[0];
            string? destFolder = split.Length > 1 ? split[1] : null;
            foreach (string path in await Task.Run(srcFolder.EnumerateFilesRecursive))
            {
                string? curHash = await path.FileHashAsync();
                if (curHash is null || _hashes.Contains(curHash) || path.BestAvailableView() is null)
                {
                    continue;
                }
                _ = _hashes.Add(curHash);
                _pendingItems.Add(new(path, curHash, destFolder));
            }
        }
    }
    private void NextItem()
    {
        Index++;
        if (CurrentPendingItem is null)
            return;
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
        NextItem();
    }
    #endregion    
}