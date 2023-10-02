using d9.utl;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace d9.ucm;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
        InitializeComponent();
        string type = "Int32";
        Text.Text = (Type.GetType(type) 
                  ?? Type.GetType($"System.{type}") 
                  ?? Type.GetType($"d9.ucm.{type}"))?.Name.PrintNull();
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
        UrlHandler? urlRule = JsonSerializer.Deserialize<UrlHandler>(
            File.ReadAllText(@"C:\Users\dninemfive\Documents\workspaces\misc\ucm\common\urlrule\secret1.json.secret"));
        if(urlRule is null)
        {
            Text.Text = "UrlRule was null";
            return;
        }
        IEnumerable<string> supportedUrls = JsonSerializer.Deserialize<List<string>>
            (File.ReadAllText(@"C:\Users\dninemfive\Documents\workspaces\misc\ucm\bookmark-plugin\ucm-bookmarks-firefox\bookmarks.json"))!
            .Where(urlRule.Supports);
        MauiProgram.HttpClient.DefaultRequestHeaders.Add("User-Agent", urlRule.Headers["User-Agent"]);
        foreach(string baseUrl in supportedUrls.Take(10))
        {
            string url = urlRule.UrlFor(baseUrl);
            Text.Text = url;
            try
            {
                string body = await MauiProgram.HttpClient.GetStringAsync(url);
                File.WriteAllText(Path.Join(@"C:\Users\dninemfive\Pictures\misc\ucm\testdata", $"{urlRule.IdFor(baseUrl)}.json"), 
                    JsonSerializer.Deserialize<JsonDocument>(body).PrettyPrint());
            } 
            catch (Exception ex)
            {
                Utils.Log($"{url}: {ex.Message}");
            }   
        }
        Text.Text = "done";
    }
}

