using System.Collections.ObjectModel;

namespace d9.ucm;

public partial class AddItems : ContentPage
{
	public AddItems()
	{
        InitializeComponent();
	}
    private bool _adding = false;
    private ObservableCollection<string> _items = new();
    private string _current;
    private async void StartAdding(object sender, EventArgs e)
    {
        if (_adding)
            return;
        _adding = true;
        Button_StartAdding.Text = "adding...";
        Output.ItemsSource = _items;
        void add(string t)
        {
            _items.Add(t);
            Button_StartAdding.Text = $"adding...{_items.Count}";
        }
        foreach(string s in await File.ReadAllLinesAsync(@"C:\Users\dninemfive\Documents\workspaces\misc\ucm\maui-app\localFolderList.txt.secret"))
        {
            foreach(string t in await Task.Run(() => Directory.EnumerateFiles(s.Split("\t")[0])))
            {
                await MainThread.InvokeOnMainThreadAsync(() => add(t));
            }
        }
        NextImage();
    }
    private void NextImage()
    {
        _current = _items.First();
        _items = new(_items.Skip(1));
        Output.ItemsSource = _items;
        Item.Source = _current;
    }
    private void Accept_Clicked(object sender, EventArgs e)
    {
        NextImage();
    }

    private void Reject_Clicked(object sender, EventArgs e)
    {
        NextImage();
    }
}

