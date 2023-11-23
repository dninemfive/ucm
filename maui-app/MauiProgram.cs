using CommunityToolkit.Maui;
using d9.utl;
using Microsoft.Extensions.Logging;

namespace d9.ucm;

public static class MauiProgram
{

    public static readonly HttpClient HttpClient = new();
	public static readonly string ITEM_FILE_LOCATION = File.ReadAllText(@"C:\Users\dninemfive\Documents\workspaces\misc\ucm\maui-app\destFolder.txt.secret");
    public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.UseMauiCommunityToolkitMediaElement()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("cour.ttf", "CourierNew");
			});		
		Utils.DefaultLog = new(Constants.Files.TEMP_Log, mode: Log.Mode.WriteImmediate);
        ItemManager.Load();
		CompetitionManager.Load();
		string[] lines = File.ReadAllLines(@"C:\Users\dninemfive\Documents\workspaces\misc\ucm\maui-app\header.secret");
		HttpClient.DefaultRequestHeaders.Add(lines[0], lines[1]);
		// load urlrules early to make the log prettier
		foreach (UrlTransformerDef _ in UrlTransformerDef.List)
			break;
		Utils.Log("Built program.");
		return builder.Build();
	}
}
