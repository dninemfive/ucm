using d9.utl;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace d9.ucm;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
        InitializeComponent();		
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
        string rulePath = @"C:\Users\dninemfive\Documents\workspaces\misc\ucm\maui-app\urlrule.json.secret",
               bookmarksPath = @"C:\Users\dninemfive\Documents\workspaces\misc\ucm\bookmark-plugin\ucm-bookmarks-firefox\bookmarks.json",
               keyPath = @"C:\Users\dninemfive\Documents\workspaces\misc\ucm\maui-app\Keys\test.key";
        UrlRule rule = JsonSerializer.Deserialize<UrlRule>(File.ReadAllText(rulePath))!;
        List<string> bookmarks = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(bookmarksPath))!;
        string url = bookmarks.Where(rule.Supports).First();
        HttpClient client = new();
        Text.Text = await client.GetStringAsync(url);
    }
}

