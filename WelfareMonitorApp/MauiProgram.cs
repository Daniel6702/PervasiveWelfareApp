// MauiProgram.cs
using Microsoft.Extensions.Logging;
using WelfareMonitorApp.ViewModels;
using WelfareMonitorApp.Services;
using WelfareMonitorApp.Views;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using WelfareMonitorApp.Helpers; // Add this using directive

﻿using CommunityToolkit.Maui;
using Plugin.FirebasePushNotifications;
using Plugin.FirebasePushNotifications.Model.Queues;
using NLog.Extensions.Logging;
using WelfareMonitorApp.Services.Logging;

#if ANDROID
using Firebase;
using WelfareMonitorApp.Platforms.Notifications;
#elif IOS
using UserNotifications;
#endif

namespace WelfareMonitorApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseFirebasePushNotifications(o =>
                {
                    o.AutoInitEnabled = false;
                    o.QueueFactory = new PersistentQueueFactory();
                #if ANDROID
                    // You can configure Android-specific options under o.Android:
                    // o.Android.NotificationActivityType = typeof(MainActivity);
                    // o.Android.NotificationChannels = NotificationChannelSamples.GetAll().ToArray();
                    // o.Android.NotificationCategories = NotificationCategorySamples.GetAll().ToArray();

                    // If you don't want to use the google-services.json file,
                    // you can configure Firebase programmatically
                    // o.Android.FirebaseOptions = new FirebaseOptions.Builder()
                    //     .SetApplicationId("appId")
                    //     .SetProjectId("projectId")
                    //     .SetApiKey("apiKey")
                    //     .SetGcmSenderId("senderId")
                    //     .Build();
                #endif
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            #if DEBUG
            builder.Logging.AddDebug();
            #endif

            //######## Notification stuff ########
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<MainViewModel>();

            builder.Services.AddTransient<QueuesPage>();
            builder.Services.AddTransient<QueuesViewModel>();

            builder.Services.AddTransient<LogPage>();
            builder.Services.AddTransient<LogViewModel>();

            builder.Services.AddSingleton<INavigationService, MauiNavigationService>();
            builder.Services.AddSingleton<IDialogService, DialogService>();
            builder.Services.AddSingleton(_ => Launcher.Default);
            builder.Services.AddSingleton(_ => Share.Default);
            builder.Services.AddSingleton(_ => Preferences.Default);
            builder.Services.AddSingleton(_ => Email.Default);
            builder.Services.AddSingleton(_ => Clipboard.Default);
            builder.Services.AddSingleton(_ => AppInfo.Current);
            builder.Services.AddSingleton(_ => DeviceInfo.Current);
            builder.Services.AddSingleton(_ => FileSystem.Current);

            var logFileReader = new NLogFileReader(NLogLoggerConfiguration.LogFilePath);
            builder.Services.AddSingleton<ILogFileReader>(logFileReader);
            //####################################

            string projectId = "pigwelfaremonitoring";

            // Register FirestoreService as a singleton
            builder.Services.AddSingleton<FirestoreService>(provider =>
            {
                // Initialize FirestoreService asynchronously
                return Task.Run(() => FirestoreService.CreateAsync(projectId)).Result;
            });
            
            // Register FirebaseAuthService as a singleton
            builder.Services.AddHttpClient<FirebaseAuthService>();
            
            // Register ViewModels
            builder.Services.AddTransient<LiveFeedViewModel>();

            // Register Pages
            builder.Services.AddTransient<LiveFeedPage>();
            
            // Register Login
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<LoginPage>();
            
            var app = builder.Build();

            // Assign the service provider to the ServiceProviderAccessor
            ServiceProviderAccessor.Instance = app.Services;

            return app;
        }
    }
}