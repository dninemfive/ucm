using CommunityToolkit.Maui;
using d9.utl;
using Microsoft.Extensions.Logging;

namespace d9.ucm;

public static class MauiProgram
{
	public const string TEMP_SAVE_LOCATION = "C:/Users/dninemfive/Pictures/misc/ucm/data";
	public const string TEMP_COMP_LOCATION = "C:/Users/dninemfive/Pictures/misc/ucm/competitions";
	public const string TEMP_RULE_LOCATION = @"C:\Users\dninemfive\Documents\workspaces\misc\ucm\common\urlrule";
	public const string REJECTED_HASH_FILE = "C:/Users/dninemfive/Pictures/misc/ucm/rejected hashes.txt";
	public static readonly HttpClient HttpClient = new();
	public static readonly string ITEM_FILE_LOCATION = File.ReadAllText(@"C:\Users\dninemfive\Documents\workspaces\misc\ucm\maui-app\destFolder.txt.secret");
    public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkitMediaElement()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});		
		Utils.DefaultLog = new(@"C:\Users\dninemfive\Pictures\misc\ucm\general.log", mode: Log.Mode.WriteImmediate);
        ItemManager.Load();
		CompetitionManager.Load();
		string[] lines = File.ReadAllLines(@"C:\Users\dninemfive\Documents\workspaces\misc\ucm\maui-app\header.secret");
		HttpClient.DefaultRequestHeaders.Add(lines[0], lines[1]);
		Utils.Log("Built program.");
		return builder.Build();
	}
}
