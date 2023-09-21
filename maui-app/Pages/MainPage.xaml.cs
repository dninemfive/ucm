using d9.utl;
using System.Text.Json;

namespace d9.ucm;

public partial class MainPage : ContentPage
{
	private HttpClient _client;
	public MainPage()
	{
        InitializeComponent();
		string rulePath = @"C:\Users\dninemfive\Documents\workspaces\misc\ucm\maui-app\urlrule.json.secret",
			   bookmarksPath = @"C:\Users\dninemfive\Documents\workspaces\misc\ucm\bookmark-plugin\ucm-bookmarks-firefox\bookmarks.json",
			   keyPath = @"C:\Users\dninemfive\Documents\workspaces\misc\ucm\maui-app\Keys\test.key";
        UrlRule rule = JsonSerializer.Deserialize<UrlRule>(File.ReadAllText(rulePath))!;
		List<string> bookmarks = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(bookmarksPath))!;
		string[] key = File.ReadAllLines(keyPath);
		_client = new();
		foreach(string s in bookmarks)
		{
			if (rule.Supports(s))
			{
				Text.Text = rule.ApiUrl(s, ("username", key[1]), ("key", key[0]));
			}
            break;
		}
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {

		Text.Text = await (await _client.GetAsync(Text.Text)).Content.ReadAsStringAsync();
    }
}

