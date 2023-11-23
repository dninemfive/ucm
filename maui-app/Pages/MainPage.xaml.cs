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
        foreach ((Item item, int i) in ItemManager.NonHiddenItems.WithProgress())
        {
            await item.SaveAsync();
            ProgressBar.Progress = i / (double)ItemManager.NonHiddenItems.Count();
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
            foreach (Item item in ItemManager.NonHiddenItems)
            {
                foreach (Item item2 in ItemManager.NonHiddenItems.Where(x => x.Id > item.Id))
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
        Utils.DefaultLog = new(Constants.Files.TEMP_Log, console, false, Log.Mode.WriteImmediate);
        Application.Current?.OpenWindow(DebugConsole);
    }

    private async void CheckDeletedItems(object sender, EventArgs e)
    {
        foreach(Item item in ItemManager.ItemsById.Values.OrderBy(x => x.Id))
        {
            if (item.Hidden)
                Utils.Log($"{item.Id,3}\t{item.Deleted}\t{item.MergeInfo?.ResultId.PrintNull()}");
        }
    }

    private async void SaveItemSources(object sender, EventArgs e)
    {
        SaveItemSourcesButton.IsEnabled = false;
        foreach((Item item, int i) in ItemManager.AllItems.WithProgress())
        {
            await Task.Run(async () 
                => File.WriteAllText(Path.Join(Constants.Folders.TEMP_Sources, $"{item.Id}.json"), 
                                     JsonSerializer.Serialize(await item.GetSourcesAsync())));
            ProgressBar.Progress = i / (double)ItemManager.NonHiddenItems.Count();
        }
        SaveItemSourcesButton.IsEnabled = true;
    }
}

