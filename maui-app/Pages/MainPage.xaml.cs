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
}

