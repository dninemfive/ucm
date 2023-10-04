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
		Random random = new Random();
		List<double> data = new();
		for(int i = 0; i < 1000; i++)
		{
			data.Add(random.Next(0, 13));
		}
		Histogram.ReplaceData(data);
	}
}

