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
    private PendingItem CurrentPendingItem => _pendingItems[_index];
    private readonly HashSet<byte[]> _hashes = new();
    private int _index = -1;
    private class PendingItem
    {
        public enum PIStatus { Pending, Accepted, Rejected }
        public PIStatus Status;
        public byte[] Hash;
        public string Path;
        public PendingItem(string path, byte[] hash)
        {
            Status = PIStatus.Pending;
            Hash = hash;
            Path = path;
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
                _ = _hashes.Add(JsonSerializer.Deserialize<byte[]>(s)!);
            }
        } else
        {
            _ = File.Create(MauiProgram.RejectedHashFile);
        }
        await File.WriteAllLinesAsync(Path.Join(MauiProgram.TEMP_SAVE_LOCATION, "debug hashes.txt"), _hashes.Select(x => JsonSerializer.Serialize(x)));
        LoadPaths.Text = "Loading unadded items...";
        foreach(string s in await File.ReadAllLinesAsync(@"C:\Users\dninemfive\Documents\workspaces\misc\ucm\maui-app\localFolderList.txt.secret"))
        {
            foreach(string path in await Task.Run(() => Directory.EnumerateFiles(s.Split("\t")[0])))
            {
                byte[]? curHash = await path.FileHashAsync();
                if(curHash is null || _hashes.Contains(curHash))
                {
                    continue;
                }
                _ = _hashes.Add(curHash);
                _pendingItems.Add(new(path, curHash));
            }
        }
        NextImage();
        LoadPaths.IsVisible = false;
        InProgressItems.IsVisible = true;
    }
    private void NextImage()
    {
        _index++;
        float progress = _index/(float)_pendingItems.Count;
        ProgressLabel.Text = $"{_index}/{_pendingItems.Count} ({progress:P1})";
        ProgressBar.Progress = progress;
        Item.Source = CurrentPendingItem.Path;
        CurrentPath.Text = $"\t{CurrentPendingItem.Path} {JsonSerializer.Serialize(CurrentPendingItem.Hash)}";                
    }
    private async void Accept_Clicked(object sender, EventArgs e)
    {
        CurrentPendingItem.Status = PendingItem.PIStatus.Accepted;
        if (File.Exists(CurrentPendingItem.Path))
            await new ImageItem(CurrentPendingItem.Path, CurrentPendingItem.Hash).SaveAsync();
        NextImage();
    }
    private void Reject_Clicked(object sender, EventArgs e)
    {
        CurrentPendingItem.Status = PendingItem.PIStatus.Rejected;
        File.AppendAllText(MauiProgram.RejectedHashFile, $"{JsonSerializer.Serialize(CurrentPendingItem.Hash)}\n");
        NextImage();
    }
}

