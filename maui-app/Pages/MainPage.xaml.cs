using d9.utl;
using HtmlAgilityPack;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;

namespace d9.ucm;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        // OpenDebugConsole();
    }

    private async void ResaveAllItems(object sender, EventArgs e)
    {
        ResaveButton.Text = "Saving...";
        ResaveButton.IsEnabled = false;
        foreach ((Item item, int i) in ItemManager.Items.WithProgress())
        {
            await item.SaveAsync();
            ProgressBar.Progress = i / (double)ItemManager.Items.Count();
        }
        ResaveButton.IsEnabled = true;
        ResaveButton.Text = "Resave All Items";
    }
    private async void TestHashThing(object sender, EventArgs e)
    {
        Utils.Log($"Checking hash uniqueness...");
        (sender as Button)!.IsEnabled = false;
        static void checkHashes()
        {
            foreach (Item item in ItemManager.Items)
            {
                foreach (Item item2 in ItemManager.Items.Where(x => x.Id > item.Id))
                {
                    if (item.Hash == item2.Hash)
                        Utils.Log($"Conflicting hashes! Items:\n\t{item}\n\t{item2}");
                }
            }
        }
        await Task.Run(checkHashes);
        (sender as Button)!.IsEnabled = true;
        Utils.Log($"Done checking hash uniqueness.");
    }

    private void OpenDebugConsole(object? sender = null, EventArgs? e = null)
    {
        ConsolePage console = new();
        Window DebugConsole = new()
        {
            Page = console
        };
        Utils.DefaultLog = new(MauiProgram.TEMP_LOG_PATH, console, false);
        Application.Current?.OpenWindow(DebugConsole);
    }

    private async void TestUrlThing(object sender, EventArgs e)
    {
        (sender as Button)!.IsEnabled = false;
        static string doThing()
        {
            string html = File.ReadAllText(MauiProgram.TEMP_BASE_FOLDER + "img.html");
            string json = html.Split("window.__INITIAL_STATE__ = JSON.parse(\"")[1].Split("\");\n            window.__URL_CONFIG__")[0];
            string unescaped = HttpUtility.HtmlDecode(json)
                                          .Replace("\\\\u002F", "/")
                                          .Replace("\\\\u003C", "<")
                                          .Replace("\\\\u003E", ">")
                                          .Replace("\\","");
            return unescaped;
        }

//HtmlDocument doc = new();
//doc.LoadHtml(html);
//Utils.Log(doc.DocumentNode.SelectSingleNode("//script[@nonce="));
        string unescaped = await Task.Run(doThing);
        //Utils.Log(unescaped);
        unescaped = unescaped.Split("\"446163267\":")[1].Split(",\"447510389")[0];
        Utils.Log(unescaped);
        try
        {
            JsonElement el = JsonSerializer.Deserialize<JsonElement>(unescaped)!;
            Utils.Log(el.PrettyPrint());
            JsonElement media = el.GetProperty("media");
            string baseUri = media.GetProperty("baseUri").GetString()!;
            string prettyName = media.GetProperty("prettyName").GetString()!;
            string token = media.GetProperty("token").EnumerateArray().First().GetString()!;
            JsonElement type = media.GetProperty("types").EnumerateArray().Where(x => x.GetProperty("t").GetString() == "preview").First();
            string resultUrl = $"{baseUri}{type.GetProperty("c").GetString()!.Replace("<prettyName>", prettyName)}?token={token}";
            Utils.Log(resultUrl);
        } 
        catch(Exception ex)
        {
            Utils.Log(ex);
        }
        
        (sender as Button)!.IsEnabled = true;
    }
}

