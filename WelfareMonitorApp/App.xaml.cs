using WelfareMonitorApp.Services;
using WelfareMonitorApp.Views;

namespace WelfareMonitorApp;

public partial class App : Application
{
    public static FirestoreService FirestoreServiceInstance { get; private set; }

    public App(FirestoreService firestoreService, LoginPage loginPage)
    {
        InitializeComponent();
        FirestoreServiceInstance = firestoreService;
        // MainPage = new AppShell();
        MainPage = new NavigationPage(loginPage);
    }

    public static async Task InitializeFirestoreAsync(string projectId)
    {
        FirestoreServiceInstance = await FirestoreService.CreateAsync(projectId);
    }
}
