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
		ToolTipProperties.SetText(Ratio, "aw;eitua;wgj");
		Ratio.Ratio = 0.44;
	}
}

