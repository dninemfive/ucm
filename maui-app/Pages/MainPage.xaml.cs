using d9.utl;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
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
        UrlRule? urlRule = JsonSerializer.Deserialize<UrlRule>(
            File.ReadAllText(@"C:\Users\dninemfive\Documents\workspaces\misc\ucm\common\urlrule\secret1.json.secret"));
        if(urlRule is null)
        {
            Text.Text = "UrlRule was null";
            return;
        }
        string supportedUrl = JsonSerializer.Deserialize<List<string>>
            (File.ReadAllText(@"C:\Users\dninemfive\Documents\workspaces\misc\ucm\bookmark-plugin\ucm-bookmarks-firefox\bookmarks.json"))!
            .Where(urlRule.Supports)
            .First();        
        HttpRequestMessage msg = urlRule.RequestMessageFor(supportedUrl);
        Text.Text = msg.RequestUri!.AbsoluteUri;
        Utils.Log(JsonSerializer.Serialize(msg, new JsonSerializerOptions() { WriteIndented = true }));
        HttpClient client = new();
        HttpResponseMessage response = await client.SendAsync(msg);
        Utils.Log(JsonSerializer.Serialize(response, new JsonSerializerOptions() { WriteIndented = true }));
        Text.Text = JsonSerializer.Serialize(response.Content, new JsonSerializerOptions() { WriteIndented = true });
        client.DefaultRequestHeaders.Add("User-Agent", urlRule.Headers["User-Agent"]);
        Utils.Log(JsonSerializer.Serialize(await client.GetStringAsync(supportedUrl)));
    }
}

