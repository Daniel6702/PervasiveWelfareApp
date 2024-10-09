using Microsoft.Extensions.Logging;
using WelfareMonitorApp.ViewModels;
using WelfareMonitorApp.Services;
using WelfareMonitorApp.Views;
namespace WelfareMonitorApp;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

public static class MauiProgram
{
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

        //string projectId = "pigwelfaremonitoring";
        //builder.Services.AddSingleton(new FirestoreService(projectId));

		return builder.Build();
	}
}
