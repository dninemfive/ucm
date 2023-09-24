using CommunityToolkit.Maui;
using d9.utl;
using Microsoft.Extensions.Logging;

namespace d9.ucm;

public static class MauiProgram
{
	public const string TEMP_SAVE_LOCATION = "C:/Users/dninemfive/Pictures/misc/ucm/data";
	public const string TEMP_COMP_LOCATION = "C:/Users/dninemfive/Pictures/misc/ucm/competitions";
	public const string REJECTED_HASH_FILE = "C:/Users/dninemfive/Pictures/misc/ucm/rejected hashes.txt";
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
		Utils.Log("Built program.");
		return builder.Build();
	}
}
