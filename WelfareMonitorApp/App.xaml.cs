using WelfareMonitorApp.Services;
using WelfareMonitorApp.Views;
using WelfareMonitorApp.Helpers;

namespace WelfareMonitorApp;

public partial class App : Application
{
    public static FirestoreService FirestoreServiceInstance { get; private set; }

    public App(FirestoreService firestoreService)
    {
        InitializeComponent();
        FirestoreServiceInstance = firestoreService;
        // MainPage = new AppShell();
        var loginPage = ServiceProviderAccessor.GetService<LoginPage>();
        MainPage = new NavigationPage(loginPage);
    }

    public static async Task InitializeFirestoreAsync(string projectId)
    {
        FirestoreServiceInstance = await FirestoreService.CreateAsync(projectId);
    }
}
