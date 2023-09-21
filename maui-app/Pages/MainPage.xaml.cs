using System.Text.Json;

namespace d9.ucm;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
        InitializeComponent();
		string rulePath = @"C:\Users\dninemfive\Documents\workspaces\misc\ucm\maui-app\urlrule.json.secret",
			   bookmarksPath = @"C:\Users\dninemfive\Documents\workspaces\misc\ucm\bookmark-plugin\ucm-bookmarks-firefox\bookmarks.json";
        UrlRule rule = JsonSerializer.Deserialize<UrlRule>(File.ReadAllText(rulePath))!;
		List<string> bookmarks = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(bookmarksPath))!;
		foreach(string s in bookmarks)
		{
			if (rule.Supports(s))
				Text.Text = rule.ApiUrl(s);
			break;
		}
	}
}

