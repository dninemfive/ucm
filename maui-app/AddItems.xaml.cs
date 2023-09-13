using System.Collections.ObjectModel;

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
        LoadPaths.Text = "Adding...";
        await foreach(ImageItem ii in IItem.LoadAllAsync<ImageItem>())
        {
            _hashes.Add(ii.FileReference.Hash);
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
    private void Accept_Clicked(object sender, EventArgs e)
    {
        _pendingItems[_index].Status = PendingItem.PIStatus.Accepted;
        IFileReference? fr = LocalFileReference.TryLoad(_pendingItems[_index].Path);
        if (fr is not null)
            new ImageItem(fr).SaveAsync();
        NextImage();
    }
    private void Reject_Clicked(object sender, EventArgs e)
    {
        _pendingItems[_index].Status = PendingItem.PIStatus.Rejected;
        NextImage();
    }
}

