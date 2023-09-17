using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;

namespace d9.ucm;

public partial class AddItems : ContentPage
{
    public AddItems()
    {
        InitializeComponent();
    }
    private bool _adding = false;
    private readonly List<PendingItem> _pendingItems = new();
    private PendingItem? CurrentPendingItem
    {
        get
        {
            if (_index >= _pendingItems.Count)
                return null;
            return _pendingItems[Index];
        }
    }
    private readonly HashSet<string> _hashes = new();
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
    private class PendingItem
    {
        public string Hash;
        public string Path;
        public string? MoveToFolder;
        public PendingItem(string path, string hash, string? moveToFolder)
        {
            Hash = hash;
            Path = path;
            MoveToFolder = moveToFolder;
        }
    }
    private async void LoadPaths_Clicked(object sender, EventArgs e)
    {
        if (_adding)
            return;
        _adding = true;
        LoadPaths.Text = "Loading existing item hashes...";
        await foreach(ImageItem ii in IItem.LoadAllAsync<ImageItem>())
        {
            _ = _hashes.Add(ii.Hash);
        }
        LoadPaths.Text = "Loading rejected file hashes...";
        if(File.Exists(MauiProgram.RejectedHashFile))
        {
            await foreach (string s in File.ReadLinesAsync(MauiProgram.RejectedHashFile))
            {
                _ = _hashes.Add(s);
            }
        } else
        {
            _ = File.Create(MauiProgram.RejectedHashFile);
        }
        await File.WriteAllLinesAsync(Path.Join(MauiProgram.TEMP_SAVE_LOCATION, "debug hashes.txt"), _hashes);
        LoadPaths.Text = "Loading unadded items...";
        static IEnumerable<string> enumerateFilesRecursive(string folder)
        {
            foreach (string s in Directory.EnumerateFiles(folder))
                yield return s;
            foreach (string s in Directory.EnumerateDirectories(folder))
                foreach (string t in enumerateFilesRecursive(s))
                    yield return t;
        };
        foreach(string s in await File.ReadAllLinesAsync(@"C:\Users\dninemfive\Documents\workspaces\misc\ucm\maui-app\localFolderList.txt.secret"))
        {
            string[] split = s.Split("\t");
            string srcFolder = split[0];
            string? destFolder = split.Length > 1 ? split[1] : null;
            foreach(string path in await Task.Run(() => enumerateFilesRecursive(srcFolder)))
            {
                string? curHash = await path.FileHashAsync();
                // await File.AppendAllTextAsync(Path.Join(MauiProgram.TEMP_SAVE_LOCATION, "log.log"), $"{curHash}\t{_hashes.Contains(curHash!)}\n");
                if (curHash is null || _hashes.Contains(curHash) || path.BestAvailableView() is null)
                {
                    continue;
                }
                _ = _hashes.Add(curHash);
                _pendingItems.Add(new(path, curHash, destFolder));
            }
        }
        NextItem();
        LoadPaths.IsVisible = false;
        InProgressItems.IsVisible = true;
    }
    private void NextItem()
    {        
        Index++;
        if (CurrentPendingItem is null)
            return;
        float progress = Index/(float)_pendingItems.Count;
        ProgressLabel.Text = $"{Index}/{_pendingItems.Count} ({progress:P1}) | {IdManager.CurrentId}";
        ProgressBar.Progress = progress;
        ItemHolder.Content = CurrentPendingItem.Path.BestAvailableView();
        CurrentPath.Text = $"\t{CurrentPendingItem.Path}";                
    }
    private async void Accept_Clicked(object sender, EventArgs e)
    {
        if (File.Exists(CurrentPendingItem!.Path))
        {
            if(CurrentPendingItem.MoveToFolder is not null)
            {
                ItemId id = IdManager.Register();
                string newPath = Path.Join(CurrentPendingItem.MoveToFolder, $"{id}{Path.GetExtension(CurrentPendingItem.Path).ToLower()}");
                File.Copy(CurrentPendingItem.Path, newPath);
                File.Delete(CurrentPendingItem.Path);
                await new ImageItem(newPath, CurrentPendingItem.Hash, id).SaveAsync();
            }
            else
            {
                await new ImageItem(CurrentPendingItem.Path, CurrentPendingItem.Hash).SaveAsync();
            }
        }
        NextItem();
    }
    private void Skip_Clicked(object sender, EventArgs e)
    {
        NextItem();
    }
    private void Reject_Clicked(object sender, EventArgs e)
    {
        File.AppendAllText(MauiProgram.RejectedHashFile, $"{CurrentPendingItem!.Hash}\n");
        NextItem();
    }
}

