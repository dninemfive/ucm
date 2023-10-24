using CommunityToolkit.Maui;
using d9.utl;
using Microsoft.Extensions.Logging;

namespace d9.ucm;

public static class MauiProgram
{
	public const string TEMP_BASE_FOLDER = "C:/Users/dninemfive/Pictures/misc/ucm/";
    public const string TEMP_SAVE_LOCATION = TEMP_BASE_FOLDER + "data";
	public const string TEMP_COMP_LOCATION = TEMP_BASE_FOLDER + "competitions";
	public const string TEMP_RULE_LOCATION = @"C:\Users\dninemfive\Documents\workspaces\misc\ucm\common\urlrule";
	public const string REJECTED_HASH_FILE = TEMP_BASE_FOLDER + "rejected hashes.txt";
    public const string TEMP_LOG_PATH = TEMP_BASE_FOLDER + "general.log";

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
			});		
		Utils.DefaultLog = new(TEMP_LOG_PATH, mode: Log.Mode.WriteImmediate);
        ItemManager.Load();
		CompetitionManager.Load();
		string[] lines = File.ReadAllLines(@"C:\Users\dninemfive\Documents\workspaces\misc\ucm\maui-app\header.secret");
		HttpClient.DefaultRequestHeaders.Add(lines[0], lines[1]);
		// load urlrules early to make the log prettier
		foreach (UrlRule _ in UrlRuleManager.UrlRules)
			break;
		Utils.Log("Built program.");
		return builder.Build();
	}
}
