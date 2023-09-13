﻿using System.Collections.ObjectModel;

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
        ProgressLabel.Text = $"{_index}/{_pendingItems.Count}";
        ProgressBar.Progress = _index/(float)_pendingItems.Count;
        Item.Source = _pendingItems[_index].Path;
        _index++;
        CurrentPath.Text = _pendingItems[_index].Path;
    }
    private void Accept_Clicked(object sender, EventArgs e)
    {
        _pendingItems[_index].Status = PendingItem.PIStatus.Accepted;
        NextImage();
    }
    private void Reject_Clicked(object sender, EventArgs e)
    {
        _pendingItems[_index].Status = PendingItem.PIStatus.Rejected;
        NextImage();
    }
}

