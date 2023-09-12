using System.Collections.ObjectModel;

namespace d9.ucm;

public partial class AddItems : ContentPage
{
	public AddItems()
	{
        InitializeComponent();
	}

    private async void StartAdding(object sender, EventArgs e)
    {
        ObservableCollection<string> items = new();
        Output.ItemsSource = items;
        foreach(string s in await File.ReadAllLinesAsync(@"C:\Users\dninemfive\Documents\workspaces\misc\ucm\maui-app\localFolderList.txt.secret"))
        {
            foreach(string t in await Task.Run(() => Directory.EnumerateFiles(s.Split("\t")[0])))
            {
                items.Add(t);
                Item.Source = t;
            }
        } 
    }
}

