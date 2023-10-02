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

    private void Button_Clicked(object sender, EventArgs e)
    {
    }
}

