// MauiProgram.cs
using Microsoft.Extensions.Logging;
using WelfareMonitorApp.ViewModels;
using WelfareMonitorApp.Services;
using WelfareMonitorApp.Views;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using WelfareMonitorApp.Helpers; // Add this using directive

namespace WelfareMonitorApp
{
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

            string projectId = "pigwelfaremonitoring";

            // Register FirestoreService as a singleton
            builder.Services.AddSingleton<FirestoreService>(provider =>
            {
                // Initialize FirestoreService asynchronously
                return Task.Run(() => FirestoreService.CreateAsync(projectId)).Result;
            });

            // Register ViewModels
            builder.Services.AddTransient<LiveFeedViewModel>();

            // Register Pages
            builder.Services.AddTransient<LiveFeedPage>();

            var app = builder.Build();

            // Assign the service provider to the ServiceProviderAccessor
            ServiceProviderAccessor.Instance = app.Services;

            return app;
        }
    }
}