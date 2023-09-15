using System.Collections.ObjectModel;
using System.Text;

namespace d9.ucm;

public partial class AddItems : ContentPage
{
	public AddItems()
	{
        InitializeComponent();
	}
    private bool _adding = false;
    private readonly List<PendingItem> _pendingItems = new();
    private readonly HashSet<byte[]> _hashes = new();
    private int _index = 0;
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
        await foreach(LocalImageItem ii in IItem.LoadAllAsync<LocalImageItem>())
        {
            _ = _hashes.Add(ii.FileReference.Hash);
        }
        LoadPaths.Text = "Loading rejected file hashes...";
        await foreach(string s in File.ReadLinesAsync(MauiProgram.RejectedHashFile))
        {
            _ = _hashes.Add(Encoding.UTF8.GetBytes(s));
        }
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
        float progress = _index/(float)_pendingItems.Count;
        ProgressLabel.Text = $"{_index}/{_pendingItems.Count} ({progress:P1})";
        ProgressBar.Progress = progress;
        Item.Source = _pendingItems[_index].Path;
        _index++;
        CurrentPath.Text = _pendingItems[_index].Path;
    }
    private async void Accept_Clicked(object sender, EventArgs e)
    {
        _pendingItems[_index].Status = PendingItem.PIStatus.Accepted;
        LocalFileReference? fr = LocalFileReference.TryLoad(_pendingItems[_index].Path);
        if (fr is not null)
            await new LocalImageItem(fr).SaveAsync();
        NextImage();
    }
    private void Reject_Clicked(object sender, EventArgs e)
    {
        _pendingItems[_index].Status = PendingItem.PIStatus.Rejected;
        File.AppendAllText(MauiProgram.RejectedHashFile, Encoding.UTF8.GetString(_pendingItems[_index].Hash));
        NextImage();
    }
}

