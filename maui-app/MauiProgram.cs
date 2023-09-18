using Microsoft.Extensions.Logging;

namespace d9.ucm;

public static class MauiProgram
{
	public const string TEMP_SAVE_LOCATION = "C:/Users/dninemfive/Pictures/misc/ucm/data";
	public static string RejectedHashFile => Path.Join(TEMP_SAVE_LOCATION, "rejected hashes.txt");
    public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif
		ItemManager.Load();
		return builder.Build();
	}
}
