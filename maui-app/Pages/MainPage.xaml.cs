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
    }

    private async void ResaveAllItems(object sender, EventArgs e)
    {
        ResaveButton.Text = "Saving...";
        ResaveButton.IsEnabled = false;
        foreach (Item item in ItemManager.Items)
            await item.SaveAsync();
        ResaveButton.IsEnabled = true;
        ResaveButton.Text = "Resave All Items";
    }
    private async void TestHashThing(object sender, EventArgs e)
    {
        Utils.Log($"Comparing hash methods...");
        (sender as Button)!.IsEnabled = false;
        int ct = 0;
        IEnumerable<string> filePaths = ItemManager.Items.Select(x => x.LocalPath.Value);
        foreach (string filePath in filePaths)
        {            
            List<Task<string?>> tasks = new()
            {
                filePath.FileHashAsync(),
                File.ReadAllBytes(filePath).HashAsync()
            };
            string?[] hashes = await Task.WhenAll(tasks);
            string? hash1 = hashes[0], hash2 = hashes[1];
            if(hash1 != hash2)
            {
                Utils.Log($"Difference in hashes for {filePath}:\n\t{hash1}\n\t{hash2}");
            }
            ProgressBar.Progress = ++ct / (double)filePaths.Count();
        }
        (sender as Button)!.IsEnabled = true;
        Utils.Log($"Done!");
    }
}

